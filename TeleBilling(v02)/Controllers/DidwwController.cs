using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeleBilling_v02_.Models;
using TeleBilling_v02_.Models.DisplayModels;
using TeleBilling_v02_.NavCustomerInfo;
using TeleBilling_v02_.Repository;
using TeleBilling_v02_.Repository.Navision;

using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Web.Routing;

namespace TeleBilling_v02_.Controllers
{
    public class DidwwController : Controller
    {
        private IFileRepository fileRepository;
        private IAgreementRepository agreementRepository;

        public DidwwController()
        {
            this.fileRepository = new FileRepository(new DBModelsContainer());
        }

        public DidwwController(IFileRepository fileRepository, IAgreementRepository agreementRepository)
        {
            this.fileRepository = fileRepository;
            this.agreementRepository = agreementRepository;
        }

        public ActionResult ViewDidww(int id=0)
        {
            List<DidwwDisplayOutbound> alldids = new List<DidwwDisplayOutbound>();
            DidwwDisplayOutbound item = new DidwwDisplayOutbound();
            alldids.Add(item);
            return View(alldids);
        }

        string[] GetFilePath(HttpPostedFileBase postedFile)
        {
            string path = string.Empty;
            string filename = string.Empty;
            string msg = string.Empty;
            if (postedFile != null && postedFile.ContentLength > 0)
            {
                if (postedFile.FileName.EndsWith(".csv"))
                {
                    filename = Path.GetFileName(postedFile.FileName);
                    path = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + filename;
                    postedFile.SaveAs(path);
                }
                else
                {
                    msg = "It is not a .csv file!";
                }
            }
            else
            {
                msg = "file is empty!";
            }
            return new[] { path, filename };
        }
       
        [HttpPost]
        public ActionResult ViewDidww(HttpPostedFileBase postedFile)
        {
            List<DidwwDisplayOutbound> alldids = new List<DidwwDisplayOutbound>();

            if (postedFile != null)
            {
                string filename = string.Empty;

                try
                {
                    string[] info = GetFilePath(postedFile);
                    string filePath = info[0];
                    filename = info[1];
                    if (filePath != string.Empty)
                    {
                        // process csv file
                        var typeId = fileRepository.GetType("PriceFile").Id;
                        List<Supplier> list = fileRepository.GetSuppliers().ToList();
                        int supplierId = -1;
                        foreach(var sup in list)
                        {
                            if (sup.Name == "Didww")
                            {
                                supplierId = sup.Id;
                                break;
                            }
                        }
                        CSVFile priceFile = fileRepository.GetFileBySupplierID(supplierId, typeId);

                        // load price list
                        List<string> priceList = new List<string>();
                        List<string> destList = new List<string>();
                        string priceFilePath = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + priceFile.Name;
                        using (StreamReader sr = new StreamReader(priceFilePath, System.Text.Encoding.GetEncoding("iso-8859-1")))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                priceList.Add(line);
                                destList.Add(line.Split(',')[2]);
                            }
                        }

                        // load didww call list
                        string callsFilePath = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + filename;
                        using (StreamReader sr = new StreamReader(callsFilePath, System.Text.Encoding.GetEncoding("iso-8859-1")))
                        {
                            string line;
                            int iCounter = 0;
                            while ((line = sr.ReadLine()) != null)
                            {
                                string[] parts = line.Split(',');
                                DidwwDisplayOutbound item = new DidwwDisplayOutbound();
                                item.Counter = iCounter.ToString();
                                item.TimeStart = parts[0].Replace("\"", "");
                                item.Source = parts[1].Replace("\"", "");
                                item.CLI = parts[2].Replace("\"", "");
                                item.Destination = parts[3].Replace("\"", "");
                                item.Duration = parts[4].Replace("\"", "");
                                item.BillingDuration = parts[5].Replace("\"", "");
                                item.DisconnectCode = parts[6].Replace("\"", "");
                                item.DisconnectReason = parts[7].Replace("\"", "");
                                item.Rate = parts[8].Replace("\"", "");
                                item.Charged = parts[9].Replace("\"", "");
                                item.CDRType = parts[10].Replace("\"", "");
                                item.CountryName = parts[11].Replace("\"", "");
                                item.NetworkName = parts[12].Replace("\"", "");
                                item.TrunkName = parts[13].Replace("\"", "");                                

                                // calculate charges
                                string sDestination = parts[3].Replace("\"", "");
                                string sPrefix = "";
                                string sChargeLine = "";
                                string sChargeLineToShow = "";

                                if (iCounter != 0)
                                {
                                    if (sDestination.Length > 0)
                                    {
                                        if (sDestination[0] == '+')
                                        {
                                            sDestination = sDestination.Substring(1);
                                        }

                                        // search destList
                                        for (int i = sDestination.Length; i > 1; i--)
                                        {
                                            string sMatchNumber = "," + sDestination.Substring(0, i) + ",";
                                            var matchingvalues = priceList.Where(x => x.Contains(sMatchNumber));
                                            if (matchingvalues.Count() > 0)
                                            {
                                                sPrefix = sDestination.Substring(0, i);
                                                sChargeLine = matchingvalues.First();
                                                string[] sChargeLineArray = sChargeLine.Split(',');

                                                string sDuration = parts[5].Replace("\"", "");
                                                int iDuration = 0;
                                                try
                                                {
                                                    iDuration = Convert.ToInt32(sDuration);
                                                }
                                                catch (Exception ex)
                                                {
                                                    ex.ToString();
                                                    iDuration = 0;
                                                }

                                                if (iDuration > 0)
                                                {
                                                    string sType = parts[10].Replace("\"", "");

                                                    string sBillingType = sChargeLineArray[3];
                                                    string sBillingInitialCost = "0";
                                                    int iBillingInitialCost = 0;
                                                    string sBillingInterval = "0";
                                                    if (sBillingType.IndexOf("/") != -1)
                                                    {
                                                        try
                                                        {
                                                            // 1/1 60/60
                                                            sBillingInitialCost = sBillingType.Substring(0, sBillingType.IndexOf("/"));
                                                            sBillingInterval = sBillingType.Substring(sBillingType.IndexOf("/") + 1);
                                                            iBillingInitialCost = Convert.ToInt32(sBillingInitialCost);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            ex.ToString();
                                                            iBillingInitialCost = 0;
                                                        }
                                                    }

                                                    string sBillingCharge = "0";
                                                    if (sType == "International") sBillingCharge = sChargeLineArray[7];
                                                    if (sType == "Origin") sBillingCharge = sChargeLineArray[8];
                                                    if (sType == "Local") sBillingCharge = sChargeLineArray[9];

                                                    double dBillingCharge = 0;
                                                    try
                                                    {
                                                        dBillingCharge = Convert.ToDouble(sBillingCharge);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ex.ToString();
                                                        dBillingCharge = 0;
                                                    }

                                                    double dCost = (double)iBillingInitialCost;

                                                    // seconds
                                                    if (sBillingInterval == "1")
                                                    {
                                                        dCost += (double)iDuration * dBillingCharge;
                                                        sChargeLineToShow = iBillingInitialCost + " + " + iDuration.ToString() + " * " + dBillingCharge.ToString() + " = " + dCost.ToString() + " øre (" + (dCost / 100).ToString() + " krone" + ")";
                                                    }

                                                    // minutes
                                                    if (sBillingInterval == "60")
                                                    {
                                                        double dMunutes = Math.Round((double)iDuration / 60, MidpointRounding.AwayFromZero);
                                                        dCost += dMunutes * dBillingCharge;
                                                        sChargeLineToShow = iBillingInitialCost + " + " + dMunutes.ToString() + " * " + dBillingCharge.ToString() + " = " + dCost.ToString() + " øre (" + (dCost / 100).ToString() + " krone" + ")";
                                                    }

                                                }

                                                break;
                                            }
                                        }
                                    }

                                    item.Prefix = sPrefix;
                                    item.RackpeopleCharge = sChargeLineToShow;
                                }

                                alldids.Add(item);

                                iCounter++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                }

                string msg = filename + " added successfully";
            }

            if (postedFile == null)
            {
                DidwwDisplayOutbound item = new DidwwDisplayOutbound();
                item.TimeStart = "4321";
                alldids.Add(item);
            }

            return View(alldids);
            //return RedirectToAction("ViewDidww", new RouteValueDictionary(new { controller = "Didww", action = "ViewDidww", msg = msg }));
        }


        /*
        [HttpPost]
        public ActionResult ViewDidww()
        {
            string sMonth = "";
            string sYear = "";

            bool bPushToNav = false;
            if (Request.Form["PushToNav"] != null)
            {
                bPushToNav = true;
            }

            if (Request.Form["DidwwMonth"] != null)
            {
                sMonth = Request.Form["DidwwMonth"].ToString();
            }

            if (Request.Form["DidwwYear"] != null)
            {
                sYear = Request.Form["DidwwYear"].ToString();
            }

            List<DidwwDisplay> alldids = new List<DidwwDisplay>();
            string API_KEY = "m5wv5bhvsagpqwwctgzkjef6oojsd1c5";

            var webRequestDIDS = WebRequest.Create("https://api.didww.com/v3/dids") as HttpWebRequest;
            if (webRequestDIDS != null)
            {
                webRequestDIDS.Method = "GET";
                webRequestDIDS.ContentType = "application/vnd.api+json";
                webRequestDIDS.Accept = "application/vnd.api+json";
                webRequestDIDS.Headers["Api-Key"] = API_KEY;

                List<string> sCDRExportedGUIDs = new List<string>();
                using (var s = webRequestDIDS.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var didsAsJson = sr.ReadToEnd();
                        var dids = JsonConvert.DeserializeObject<Dids>(didsAsJson);

                        for (int i = 0; i < dids.data.Count; i++)
                        {
                            if (dids.data[i].attributes.number != null)
                            {
                                string sDIDNumber = dids.data[i].attributes.number;
                                string sDIDDescription = dids.data[i].attributes.description;
                                if (sDIDNumber != "")
                                {
                                    // create CDR export now
                                    var webRequestCDRE = WebRequest.Create("https://api.didww.com/v3/cdr_exports") as HttpWebRequest;
                                    if (webRequestCDRE != null)
                                    {
                                        webRequestCDRE.Method = "POST";
                                        webRequestCDRE.ContentType = "application/vnd.api+json";
                                        webRequestCDRE.Accept = "application/vnd.api+json";
                                        webRequestCDRE.Headers["Api-Key"] = API_KEY;

                                        string sCDRR = "";
                                        sCDRR += "{";
                                        sCDRR += "\"data\": {";
                                        sCDRR += "   \"type\": \"cdr_exports\",";
                                        sCDRR += "   \"attributes\": {";
                                        sCDRR += "      \"filters\": {";
                                        sCDRR += "         \"year\": \"" + sYear + "\",";
                                        sCDRR += "         \"month\": \"" + sMonth + "\",";
                                        sCDRR += "         \"did_number\": \"" + sDIDNumber + "\"";
                                        sCDRR += "      }";
                                        sCDRR += "    }";
                                        sCDRR += "  }";
                                        sCDRR += "}";

                                        var data = Encoding.ASCII.GetBytes(sCDRR);
                                        webRequestCDRE.ContentLength = data.Length;

                                        using (var sW = webRequestCDRE.GetRequestStream())
                                        {
                                            sW.Write(data, 0, data.Length);
                                        }

                                        using (var rW = webRequestCDRE.GetResponse().GetResponseStream())
                                        {
                                            using (var srW = new StreamReader(rW))
                                            {
                                                var CDRExportAsJson = srW.ReadToEnd();
                                                var CDRExport = JsonConvert.DeserializeObject<CDRExportReportResponse>(CDRExportAsJson);

                                                string sCDRExportStatus = CDRExport.data.attributes.status;
                                                string sCDRExportGUID = CDRExport.data.id;

                                                if (sCDRExportGUID != null)
                                                {
                                                    if (sCDRExportGUID != "")
                                                    {
                                                        sCDRExportedGUIDs.Add(sCDRExportGUID + "ђ" + sDIDDescription + "ђ" + sDIDNumber);
                                                        //Console.WriteLine("Request pending for " + sDIDNumber + ", Guid: " + sCDRExportGUID + ".");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Getting CDR Exports now
                System.Threading.Thread.Sleep(3000);

                string sCSVContent = "";

                // now get all exports
                if (sCDRExportedGUIDs.Count > 0)
                {
                    int iCDRGuid = 0;

                    while (iCDRGuid < sCDRExportedGUIDs.Count)
                    {
                        var webRequestCDR = WebRequest.Create("https://api.didww.com/v3/cdr_exports/" + sCDRExportedGUIDs[iCDRGuid].Split('ђ')[0]) as HttpWebRequest;
                        if (webRequestCDR != null)
                        {
                            webRequestCDR.Method = "GET";
                            webRequestCDR.ContentType = "application/vnd.api+json";
                            webRequestCDR.Accept = "application/vnd.api+json";
                            webRequestCDR.Headers["Api-Key"] = API_KEY;

                            using (var s = webRequestCDR.GetResponse().GetResponseStream())
                            {
                                using (var sr = new StreamReader(s))
                                {
                                    var cdrSingleExportAsJson = sr.ReadToEnd();
                                    var cdrSingleExport = JsonConvert.DeserializeObject<CDRSingleExport>(cdrSingleExportAsJson);

                                    if (cdrSingleExport.data.attributes.url != null)
                                    {
                                        string sCDRUrl = cdrSingleExport.data.attributes.url;
                                        string sCDRStatus = cdrSingleExport.data.attributes.status;

                                        //Console.WriteLine("CSV Data request status for: " + sCDRExportedGUIDs[iCDRGuid] + ": " + sCDRStatus);

                                        if (sCDRStatus == "Completed")
                                        {
                                            if (sCDRUrl != "")
                                            {
                                                // now get csv
                                                var webRequestCSV = WebRequest.Create(sCDRUrl) as HttpWebRequest;
                                                if (webRequestCSV != null)
                                                {
                                                    webRequestCSV.Method = "GET";
                                                    webRequestCSV.Accept = "text/csv";
                                                    webRequestCSV.Headers["Api-Key"] = API_KEY;

                                                    using (var sCSV = webRequestCSV.GetResponse().GetResponseStream())
                                                    {
                                                        using (var srCSV = new StreamReader(sCSV))
                                                        {
                                                            if (iCDRGuid != 0)
                                                            {
                                                                srCSV.ReadLine();
                                                            }

                                                            sCSVContent += "DID:ђ" + sCDRExportedGUIDs[iCDRGuid].Split('ђ')[1] + "ђ" + sCDRExportedGUIDs[iCDRGuid].Split('ђ')[2] + "\n";
                                                            sCSVContent += srCSV.ReadToEnd();
                                                            sCSVContent += "\nDURATION:ђ\n";

                                                            //Console.WriteLine("CSV Data found for: " + sCDRExportedGUIDs[iCDRGuid]);
                                                            iCDRGuid++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // wait a little
                                            System.Threading.Thread.Sleep(2000);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (sCSVContent != "")
                {
                    string[] sCSVContentArray = sCSVContent.Split('\n');
                    int iCount = 0;
                    int iDurationSum = 0;
                    string sDIDDescription = "";
                    string sDIDNumber = "";
                    foreach (string sCSVContentLine in sCSVContentArray)
                    {
                        if (sCSVContentLine != "")
                        {
                            if (sCSVContentLine.IndexOf("DID:ђ") == 0)
                            {
                                // first did line
                                sDIDDescription = sCSVContentLine.Split('ђ')[1];
                                sDIDNumber = sCSVContentLine.Split('ђ')[2];
                                iCount = 0;
                                iDurationSum = 0;
                            }

                            if (sCSVContentLine.IndexOf("DURATION:ђ") == 0)
                            {
                                // sums
                                DidwwDisplay item = new DidwwDisplay();
                                item.DIDDate = sDIDDescription;
                                item.DIDSource = "SUMLINE";
                                item.DID = sDIDNumber;
                                item.DIDDuration = iDurationSum.ToString();
                                alldids.Add(item);
                            }

                            if ((sCSVContentLine.IndexOf("DID:ђ") != 0) && (sCSVContentLine.IndexOf("DURATION:ђ") != 0))
                            {
                                string[] sCSVContentLineArray = sCSVContentLine.Split(',');
                                DidwwDisplay item = new DidwwDisplay();
                                item.DIDCounter = iCount.ToString();
                                item.DIDDate = sCSVContentLineArray[0];
                                item.DIDSource = sCSVContentLineArray[1];
                                item.DID = sCSVContentLineArray[2];
                                item.DIDDestination = sCSVContentLineArray[3];
                                item.DIDDuration = sCSVContentLineArray[4];
                                item.DisconnectInitiator = sCSVContentLineArray[5];
                                item.DisconnectCode = sCSVContentLineArray[6];
                                item.Response = sCSVContentLineArray[7];
                                item.TollFreeAmount = sCSVContentLineArray[8];
                                item.TerminationAmount = sCSVContentLineArray[9];
                                item.MeteredChannelsAmount = sCSVContentLineArray[10];
                                alldids.Add(item);
                                iCount++;

                                int iDuration = 0;
                                try
                                {
                                    iDuration = Convert.ToInt32(sCSVContentLineArray[4]);
                                    iDurationSum += iDuration;
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                    iDuration = 0;
                                }

                            }
                            
                        }
                    }
                }
            }

            return View(alldids);
        }
        */

    }
}