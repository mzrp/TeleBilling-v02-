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
using TeleBilling_v02_.Models.Navision;

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
            this.agreementRepository = new AgreementRepository(new DBModelsContainer());
        }

        public DidwwController(IFileRepository fileRepository, IAgreementRepository agreementRepository)
        {
            this.fileRepository = fileRepository;
            this.agreementRepository = agreementRepository;
        }

        public ActionResult ViewDidww(int id=0)
        {
            DidwwDisplayOutboundExtended alldids = new DidwwDisplayOutboundExtended();
            DidwwDisplayOutbound item = new DidwwDisplayOutbound();
            alldids.alldidwws = new List<DidwwDisplayOutbound>();
            alldids.alldidwws.Add(item);
            alldids.pushresults = "Please upload outbound call list.";
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
            DidwwDisplayOutboundExtended alldids = new DidwwDisplayOutboundExtended();
            alldids.alldidwws = new List<DidwwDisplayOutbound>();

            string sInfoMsg = "";
            IEnumerable<Agreement> agreementList = null;

            string[] info = null;
            if (postedFile == null)
            {
                if (Session["sesFilePathInfo"] != null)
                {
                    info = (string[])Session["sesFilePathInfo"];
                }
            }
            else
            {
                info = GetFilePath(postedFile);
            }

            string filename = string.Empty;

            try
            {
                //string[] info = GetFilePath(postedFile);

                string filePath = info[0];
                filename = info[1];
                if (filePath != string.Empty)
                {
                    Session.Add("sesFilePathInfo", info);

                    // process csv file
                    var typeId = fileRepository.GetType("PriceFile").Id;
                    List<Supplier> list = fileRepository.GetSuppliers().ToList();
                    int supplierId = -1;
                    int csvFileId = -1;
                    foreach (var sup in list)
                    {
                        if (sup.Name == "Didww")
                        {
                            supplierId = sup.Id;
                            foreach (var csvfilefirst in sup.CSVFile)
                            {
                                csvFileId = fileRepository.GetFileByName(csvfilefirst.Name).Id;
                                break;
                            }                            
                            break;
                        }
                    }
                    CSVFile priceFile = fileRepository.GetFileBySupplierID(supplierId, typeId);

                    if (postedFile == null)
                    {
                        agreementList = agreementRepository.GetAgreements(csvFileId).ToList();
                    }

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
                            item.MinutePrice = "";
                            item.SecondPrice = "";
                            item.FinalChargeK = "";
                            item.FinalChargeO = "";

                            // calculate charges
                            string sDestination = parts[3].Replace("\"", "");
                            string sPrefix = "";
                            string sChargeLine = "";
                            string sChargeLineToShow = "";

                            if (iCounter != 0)
                            {
                                string sDestinationNetwork = "";
                                string sFinalChargeK = "";
                                string sFinalChargeO = "";
                                string sSecondPrice = "";
                                string sMinutePrice = "";

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

                                                sDestinationNetwork = sChargeLineArray[0] + "-" + sChargeLineArray[1];

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

                                                    sSecondPrice = dBillingCharge.ToString();
                                                    sMinutePrice = "0";
                                                }

                                                // minutes
                                                if (sBillingInterval == "60")
                                                {
                                                    double dMunutes = Math.Round((double)iDuration / 60, MidpointRounding.AwayFromZero);
                                                    dCost += dMunutes * dBillingCharge;
                                                    sChargeLineToShow = iBillingInitialCost + " + " + dMunutes.ToString() + " * " + dBillingCharge.ToString() + " = " + dCost.ToString() + " øre (" + (dCost / 100).ToString() + " krone" + ")";

                                                    sMinutePrice = dBillingCharge.ToString();
                                                    sSecondPrice = "0";
                                                }

                                                sFinalChargeK = (dCost / 100).ToString();
                                                sFinalChargeO = dCost.ToString();

                                            }

                                            break;
                                        }
                                    }
                                }

                                item.DestinationNetwork = sDestinationNetwork;
                                item.FinalChargeK = sFinalChargeK;
                                item.FinalChargeO = sFinalChargeO;
                                item.Prefix = sPrefix;
                                item.RackpeopleCharge = sChargeLineToShow;
                                item.MinutePrice = sMinutePrice;
                                item.SecondPrice = sSecondPrice;
                            }

                            alldids.alldidwws.Add(item);

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

            if (postedFile != null)
            {
                alldids.pushresults = "DIDWW outgoing call log processed.";
            }

            if (postedFile == null)
            {
                if (alldids.alldidwws.Count > 0) 
                {
                    List<InvoiceModel> appliedAgreements = new List<InvoiceModel>();
                    foreach (var singledid in alldids.alldidwws)
                    {

                        if ((singledid.MinutePrice != "") && (singledid.SecondPrice != ""))
                        {

                            long lLongCheck = -1;
                            try 
                            {
                                lLongCheck = Convert.ToInt64(singledid.Source);
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                                lLongCheck = -1;
                            }

                            if (lLongCheck != -1)
                            {
                                if (agreementList.Any(x => Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(singledid.Source)
                                                                            && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(singledid.Source)))
                                {
                                    var tempAgreement = agreementList.Single(x => Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(singledid.Source)
                                                   && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(singledid.Source));

                                    InvoiceModel temInvoice = new InvoiceModel();
                                    temInvoice.CVR = tempAgreement.Customer_cvr;

                                    var existedInvoice = appliedAgreements.Where(x => x.CVR == temInvoice.CVR).FirstOrDefault();

                                    if (existedInvoice == null)
                                    {
                                        temInvoice.LineCollections = new List<InvoiceLineCollectionModel>();

                                        InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
                                        {
                                            //Id = record.Id,
                                            StartDate = Convert.ToDateTime(singledid.TimeStart),
                                            EndDate = Convert.ToDateTime(singledid.TimeStart),
                                            Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
                                            Subscriber_Range_End = tempAgreement.Subscriber_range_end,
                                            Agreement_Description = tempAgreement.Description
                                        };

                                        ZoneLinesModel temZone = new ZoneLinesModel()
                                        {
                                            ZoneName = singledid.DestinationNetwork + " - " + singledid.Destination,
                                            ZoneCalls = 1,
                                            ZoneCallNo = "10036",
                                            ZoneSeconds = Convert.ToInt32(singledid.BillingDuration),
                                            ZoneMinuteNo = "10037",
                                            ZonePriceMinute = Convert.ToDecimal(singledid.FinalChargeK),
                                            ZonePriceCall = Convert.ToDecimal(singledid.FinalChargeK)
                                        };

                                        invoiceLine.ZoneLines = new List<ZoneLinesModel>();
                                        invoiceLine.ZoneLines.Add(temZone);

                                        temInvoice.LineCollections.Add(invoiceLine);
                                        appliedAgreements.Add(temInvoice);
                                    }
                                    else
                                    {
                                        var rangeExisted = existedInvoice.LineCollections.Find(a => Convert.ToInt64(a.Subscriber_Range_Start) <= Convert.ToInt64(singledid.Source)
                                                                                                 && Convert.ToInt64(a.Subscriber_Range_End) >= Convert.ToInt64(singledid.Source));

                                        if (rangeExisted == null)
                                        {
                                            InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
                                            {
                                                //Id = record.Id,
                                                StartDate = Convert.ToDateTime(singledid.TimeStart),
                                                EndDate = Convert.ToDateTime(singledid.TimeStart),
                                                Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
                                                Subscriber_Range_End = tempAgreement.Subscriber_range_end,
                                                Agreement_Description = tempAgreement.Description
                                            };

                                            ZoneLinesModel temZone = new ZoneLinesModel()
                                            {
                                                ZoneName = singledid.DestinationNetwork + " - " + singledid.Destination,
                                                ZoneCalls = 1,
                                                ZoneCallNo = "10036",
                                                ZoneSeconds = Convert.ToInt32(singledid.BillingDuration),
                                                ZoneMinuteNo = "10037",
                                                ZonePriceMinute = Convert.ToDecimal(singledid.FinalChargeK),
                                                ZonePriceCall = Convert.ToDecimal(singledid.FinalChargeK)
                                            };

                                            foreach (var i in appliedAgreements)
                                            {
                                                if (i.CVR == temInvoice.CVR)
                                                {
                                                    invoiceLine.ZoneLines = new List<ZoneLinesModel>();
                                                    invoiceLine.ZoneLines.Add(temZone);
                                                    i.LineCollections.Add(invoiceLine);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            DateTime StartDate;
                                            DateTime EndDate;

                                            if (DateTime.Compare(rangeExisted.StartDate, Convert.ToDateTime(singledid.TimeStart)) < 0) //is earlier than
                                            {
                                                StartDate = rangeExisted.StartDate;
                                            }
                                            else
                                            {
                                                StartDate = Convert.ToDateTime(singledid.TimeStart);
                                            }

                                            if (DateTime.Compare(rangeExisted.EndDate, Convert.ToDateTime(singledid.TimeStart)) > 0)// is later than
                                            {
                                                EndDate = rangeExisted.EndDate;
                                            }
                                            else
                                            {
                                                EndDate = Convert.ToDateTime(singledid.TimeStart);
                                            }

                                            ZoneLinesModel temZone = new ZoneLinesModel()
                                            {
                                                ZoneName = singledid.DestinationNetwork + " - " + singledid.Destination,
                                                ZoneCalls = 1,
                                                ZoneCallNo = "10036",
                                                ZoneSeconds = Convert.ToInt32(Convert.ToInt32(singledid.BillingDuration)),
                                                ZoneMinuteNo = "10037",
                                                ZonePriceMinute = Convert.ToDecimal(singledid.FinalChargeK),
                                                ZonePriceCall = Convert.ToDecimal(singledid.FinalChargeK)
                                            };
                                            foreach (var i in appliedAgreements)
                                            {
                                                if (i.CVR == temInvoice.CVR)
                                                {
                                                    foreach (var ii in i.LineCollections)
                                                    {
                                                        if (Convert.ToInt64(ii.Subscriber_Range_Start) <= Convert.ToInt64(singledid.Source)
                                                          && Convert.ToInt64(ii.Subscriber_Range_End) >= Convert.ToInt64(singledid.Source))
                                                        {
                                                            ii.StartDate = StartDate;
                                                            ii.EndDate = EndDate;
                                                            ii.ZoneLines.Add(temZone);
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

                    // push to nav now
                    if (appliedAgreements.Count > 0)
                    {
                        var ab = Accumulate(appliedAgreements);
                        List<string> errorMsg = InvoiceGenerator.BillDidww(ab);

                        if (errorMsg.Count == 0)
                        {
                            sInfoMsg = "";
                            foreach (var singleab in appliedAgreements)
                            {
                                sInfoMsg += "Customer NavId: " + singleab.CVR + " pushed to NAV.\n";
                            }                            
                        }
                        else
                        {
                            int i = 0;
                            foreach (string error in errorMsg)
                            {
                                i += 1;
                                sInfoMsg += i + ". " + error + ";\n";
                            }
                        }
                    }
                }

                alldids.pushresults = sInfoMsg;
            }

            return View(alldids);
            //return RedirectToAction("ViewDidww", new RouteValueDictionary(new { controller = "Didww", action = "ViewDidww", msg = msg }));
        }

        public List<InvoiceModel> Accumulate(List<InvoiceModel> billableList)
        {
            foreach (InvoiceModel invoice in billableList)
            {
                foreach (InvoiceLineCollectionModel line in invoice.LineCollections)
                {
                    line.Accumulated = new List<AccumulatedModel>();
                    foreach (ZoneLinesModel record in line.ZoneLines)
                    {
                        line.Accumulated.Add(
                                new AccumulatedModel()
                                {
                                    Subscriber = line.Subscriber_Range_Start,
                                    ZoneName = record.ZoneName,
                                    Call_No = record.ZoneCallNo,
                                    Minute_No = record.ZoneMinuteNo,
                                    Call_price = record.ZonePriceCall,
                                    Seconds = record.ZoneSeconds,
                                    Minute_price = record.ZonePriceCall,
                                    styk = 1,
                                    Total = record.ZonePriceCall
                                });
                    }
                }
            }
            return billableList;
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