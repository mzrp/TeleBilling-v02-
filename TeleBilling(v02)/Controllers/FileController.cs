using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeleBilling_v02_.Models;
using TeleBilling_v02_.Repository;
using CsvHelper;
using System.Globalization;
using TeleBilling_v02_.Repository.Navision;
using TeleBilling_v02_.Models.Navision;
using TeleBilling_v02_.Models.DisplayModels;
using System.Text;
using System.Web.Routing;

namespace TeleBilling_v02_.Controllers
{
    public class FileController : Controller
    {
        private IFileRepository fileRepository;
        private IAgreementRepository agreementRepository;

        public FileController()
        {
            this.fileRepository = new FileRepository(new DBModelsContainer());
            this.agreementRepository = new AgreementRepository(new DBModelsContainer());
        }

        public FileController(IFileRepository fileRepository, IAgreementRepository agreementRepository)
        {
            this.fileRepository = fileRepository;
            this.agreementRepository = agreementRepository;
        }

        public ActionResult ViewInvoiceFiles(string msg= "")
        {
            if (Session["UserId"] != null)
            {
                int fileId = fileRepository.GetType("InvoiceFile").Id;
                var files = fileRepository.GetCsvFileByTypeId(fileId).ToList();
                //List<CSVFile> files = fileRepository.GetInvoiceFiles(fileId).ToList();                

                if (msg != "")
                {
                    ViewBag.Message = msg;
                    //return HttpNotFound();
                }

                return View(files);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }  
        
        public ActionResult ViewPriceFiles(string msg = "")
        {
            //ViewBag.ZoneRecord = "AgreementZoneRecords";
            if (Session["UserId"] != null)
            {
                int fileId = fileRepository.GetType("PriceFile").Id;
                var files = fileRepository.GetCsvFileByTypeId(fileId).ToList();
                //List<CSVFile> files = fileRepository.GetPriceFiles(fileId).ToList();                
                if (msg != "")
                {
                    ViewBag.Message = msg;
                    //return HttpNotFound();
                }
                return View(files);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }


        public ActionResult ViewFilesDetails(int fileId, string fileName, DateTime fileDate, string fileSupplierName, string fileType)
        {
            CSVFile csvFile = new CSVFile();
            csvFile.Name = fileName;
            ViewBag.SupplierName = fileSupplierName;
            ViewBag.fileType = fileType;
            csvFile.Date = fileDate; 
            if (fileId.Equals(0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                if (fileType == "InvoiceFile")
                {
                    Session.Add("sesFilename", fileName);

                    var recordList = fileRepository.GetFileInvoiceDetails(fileId).Where(x => x.RPBilled == "No").ToList();
                    var sId = recordList.FirstOrDefault().CSVFile.SupplierId;
                    var typeId = fileRepository.GetType("PriceFile").Id;
                    var files = fileRepository.GetCsvFileByTypeId(typeId);
                    var fileNameZR = files.SingleOrDefault(f => f.SupplierId == sId && f.TypeId == typeId).Name;// here supplier id should be equal to the sId
                    Session.Add("sesFilenameZR", fileNameZR);

                    Session.Add("sesZoneRecords", fileRepository.GetFileByName(fileNameZR).ZoneRecords.ToList());

                    csvFile.InvoiceRecords = fileRepository.GetFileInvoiceDetails(fileId).ToList();
                    csvFile.BillableList = csvFile.InvoiceRecords.Where(x => x.RPBilled == "No").ToList();
                    //csvFile.ZoneRecords = fileRepository.GetFileByName(fileName).ZoneRecords.ToList();
                    //return View(csvFile.InvoiceRecords);
                }

                if (fileType == "InvoiceFileAlreadyDone")
                {
                    Session.Add("sesFilename", fileName);

                    var recordList = fileRepository.GetFileInvoiceDetails(fileId).Where(x => x.RPBilled == "Yes").ToList();
                    var sId = recordList.FirstOrDefault().CSVFile.SupplierId;
                    var typeId = fileRepository.GetType("PriceFile").Id;
                    var files = fileRepository.GetCsvFileByTypeId(typeId);
                    var fileNameZR = files.SingleOrDefault(f => f.SupplierId == sId && f.TypeId == typeId).Name;// here supplier id should be equal to the sId
                    Session.Add("sesFilenameZR", fileNameZR);

                    Session.Add("sesZoneRecords", fileRepository.GetFileByName(fileNameZR).ZoneRecords.ToList());

                    csvFile.InvoiceRecords = fileRepository.GetFileInvoiceDetails(fileId).ToList();
                    csvFile.BillableList = csvFile.InvoiceRecords.Where(x => x.RPBilled == "Yes").ToList();
                    //csvFile.ZoneRecords = fileRepository.GetFileByName(fileName).ZoneRecords.ToList();
                    //return View(csvFile.InvoiceRecords);
                }

                if (fileType == "PriceFile")
                {
                    csvFile.ZoneRecords = fileRepository.GetFileZoneDetails(fileId).ToList();
                    string sCSVFilePath = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + fileName;
                    csvFile.fileType = sCSVFilePath;
                    //return View(csvFile.AgreementZoneRecords);
                }      
                

            }
            return View(csvFile);
        }
        

        // GET: File/Details/5
        public ActionResult ViewInvoiceDetails(int fileId)
        {
            if (fileId.Equals(0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var fileDetails = fileRepository.GetFileInvoiceDetails(fileId);
            if (fileDetails == null)
            {
                return HttpNotFound();
            }
            return View(fileDetails);
        }

        public ActionResult UploadCsvFile(string fileType)
        {
            CSVFile csvFile = new CSVFile();
            //Csvfile.SupplierCollection = fileRepository.GetSuppliers().ToList();
            List<Supplier> list = fileRepository.GetSuppliers().ToList();
            ViewBag.SupplierList = new SelectList(list, "Id", "Name", "Type");
            ViewBag.fileType = fileType;
            return View(csvFile);
        }     

      
        //ICollection<InvoiceRecords> invoiceRecords = new List<InvoiceRecords>();
        //ICollection<AgreementZoneRecords> zoneRecords = new List<AgreementZoneRecords>();
        //// POST: File/Create
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase postedFile, CSVFile model)
        {
            string supplierName = fileRepository.GetSupplier(model.SupplierId).Name;
            ICollection<InvoiceRecords> invoiceRecords = new List<InvoiceRecords>();
            ICollection<Models.ZoneRecords> zoneRecords = new List<Models.ZoneRecords>();
            string filename = string.Empty;
            try
            {
                //TODO: Add insert logic here
                if (ModelState.IsValid)
                {
                    string[] info = GetFilePath(postedFile);
                    string filePath = info[0];
                    filename = info[1];
                    if (filePath != string.Empty)
                    {
                        model.Name = filename;
                        model.Date = DateTime.Now;
                        model.TypeId = fileRepository.GetType(model.fileType).Id;

                        if (Session["UserName"].ToString() == "")
                        {
                            model.UserId = 5;
                        }
                        else
                        {
                            model.UserId = fileRepository.GetUser(Session["UserName"].ToString()).Id;
                        }

                        if (model.fileType == "InvoiceFile")
                        {
                            //Handel the CDR Files
                            var result = HandleCDR(filePath, model.SupplierId);
                            invoiceRecords = result.Keys.ElementAt(0);
                            string errorMsg = result[result.Keys.ElementAt(0)];
                            if (!string.IsNullOrEmpty(errorMsg))
                            {
                                return Json(new { success = false, message = errorMsg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                if (notConvertAbleList.Count > 0)
                                {
                                    string error = "the following line number can not be converted: ";
                                    int i = 1;
                                    foreach (int linenumber in notConvertAbleList)
                                    {
                                        error += i + "-line number:  " + linenumber + "\n . ";
                                        i++;
                                    }
                                    return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
                                    
                                }
                                else
                                {
                                    if (invoiceRecords.ToList().Count() > 0)
                                    {
                                        model.InvoiceRecords = invoiceRecords;
                                        using (var db = new DBModelsContainer())
                                        {
                                            try
                                            {
                                                db.CSVFileSet.Add(model);
                                                db.SaveChanges();

                                                //return RedirectToAction("UploadInvoices");
                                            }
                                            catch (Exception ex)
                                            {
                                                //return View("Error");
                                                return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                                            }

                                        }

                                    }
                                }
                            }
                        }
                        else if(model.fileType == "PriceFile")
                        {                           
                            if (supplierName == "Didww")
                            {
                                model.ZoneRecords = zoneRecords;
                                using (var db = new DBModelsContainer())
                                {
                                    try
                                    {
                                        db.CSVFileSet.Add(model);
                                        db.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {
                                        //return View("Error");
                                        return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            else
                            {

                                zoneRecords = HandleZone(filePath, model.SupplierId);

                                if (zoneRecords.Count > 0)
                                {
                                    model.ZoneRecords = zoneRecords;
                                    using (var db = new DBModelsContainer())
                                    {
                                        try
                                        {
                                            db.CSVFileSet.Add(model);
                                            db.SaveChanges();
                                        }
                                        catch (Exception ex)
                                        {
                                            //return View("Error");
                                            return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }                            
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            
            if (invoiceRecords.Count > 0)
            {
                string msg = filename + " (with " + invoiceRecords.Count + " records) is added successfully";
                return RedirectToAction("ViewInvoiceFiles", new RouteValueDictionary(new { controller = "File", action = "ViewInvoiceFiles", msg = msg }));
                //return View(invoiceRecords);
            }
            else if (zoneRecords.Count > 0)
            {
                string msg = filename + " (with " + zoneRecords.Count + " records) is added successfully";
                return  RedirectToAction("ViewPriceFiles", new RouteValueDictionary(new { controller = "File", action = "ViewPriceFiles", msg = msg }));
            }
            else if (supplierName == "Didww")
            {
                string msg = filename + " is added successfully";
                return RedirectToAction("ViewPriceFiles", new RouteValueDictionary(new { controller = "File", action = "ViewPriceFiles", msg = msg }));
            }
            else
            {
                return View("Error");
            }
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

        List<int> notConvertAbleList = new List<int>();
        Dictionary<ICollection<InvoiceRecords>, string> HandleCDR(string path, int supplierId)
        {
            string errorMsg = string.Empty;
            ICollection<InvoiceRecords> invoiceRecords = new List<InvoiceRecords>();
            //var csv = new CsvReader(new StreamReader(path, Encoding.GetEncoding("windows-1252")));

            Encoding enc = new UTF8Encoding(true, true);
            TextReader tr = new StreamReader(path, enc);

            try
            {
                Supplier supplier = fileRepository.GetSupplier(supplierId);
                //invoiceRecords = new List<InvoiceRecords>();
                InvoiceRecords record;
                int rowNumber;
                if (supplier.Name == "IPVision")
                {
                    rowNumber = 0;
                    //var temp = csv.GetRecords<IPVision>(); 
                    string sHeaderLine = tr.ReadLine();
                    string line;
                    //foreach (var line in temp)
                    while ((line = tr.ReadLine()) != null)
                    {                        
                        rowNumber += 1;
                        string[] lineArray = line.ToString().Split(';');
                        try
                        {
                            //record = new InvoiceRecords();
                            //record.Id_call = line.id;
                            //record.Time = DateTime.ParseExact(line.time, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                            //record.Subscriber = line.subscriber;
                            //record.Aprefix = line.aprefix;
                            //record.Destination = line.destination;
                            //record.Invoice_group = line.invoicegroup;
                            //record.Prefix = line.prefix;
                            //record.Pbx = line.pbx;
                            //record.Direction = line.direction;
                            //record.Volume_time_secs = line.volumetimesecs;
                            //record.Price = line.price;
                            //record.Free = line.free;
                            //record.Forward = line.forward;
                            //record.Servingnetwork = line.servingnetwork;
                            //record.Reason = line.reason;
                            //record.Billed = line.billed;
                            //record.RPBilled = "No";
                            //record.ZoneName = line.zone;

                            record = new InvoiceRecords();
                            record.Id_call = lineArray[0];
                            record.Time = DateTime.ParseExact(lineArray[1], "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                            record.Subscriber = lineArray[2];
                            record.Aprefix = lineArray[3];
                            record.Destination = lineArray[4];
                            record.Invoice_group = lineArray[5];
                            record.Prefix = lineArray[6];
                            record.Pbx = lineArray[7];
                            record.Direction = lineArray[8];
                            record.Volume_time_secs = lineArray[9];
                            record.Price = lineArray[10];
                            record.Free = lineArray[11];
                            record.Forward = lineArray[12];
                            record.Servingnetwork = lineArray[13];
                            record.Reason = lineArray[14];
                            record.Billed = lineArray[15];
                            record.RPBilled = "No";
                            record.ZoneName = lineArray[16];
                            invoiceRecords.Add(record);
                        }
                        catch (Exception ex)
                        {
                            errorMsg += ex.ToString();
                            notConvertAbleList.Add(rowNumber);
                        }
                    }                    
                }
                else if(supplier.Name == "Unitel")
                {
                    var typeId = fileRepository.GetType("PriceFile").Id;
                    CSVFile priceFile = fileRepository.GetFileBySupplierID(supplierId, typeId);
                    //var temp = csv.GetRecords<Unitel>();

                    //var temp = tr.ReadToEnd();
                    string sHeaderLine = tr.ReadLine(); // this read first line

                    string line;
                    rowNumber = 0;
                    //foreach (var line in temp)
                    while((line = tr.ReadLine()) != null)
                    {
                        string[] lineArray = line.ToString().Split(';');
                        rowNumber += 1;
                        record = new InvoiceRecords();
                        //"CustomerId"; "Trunk"; "CallId"; "From"; "To"; "StartTime"; "Type"; "Duration"; "Price"; "BillingZoneId"; "BillingZonePrefix"; "BillingZoneDescription"; "BillingZoneCallFee"; "BillingZoneCallRate"
                        //record.Id_call = line.CallId;
                        record.Id_call = lineArray[2];

                        //if (!string.IsNullOrEmpty(line.StartTime))
                        if (!string.IsNullOrEmpty(lineArray[5]))
                        {
                            try
                            {
                                //record.Time = DateTime.ParseExact(line.StartTime, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                                record.Time = DateTime.ParseExact(lineArray[5], "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                errorMsg += rowNumber + " dataTime is not parseable";
                            }
                        }
                        else
                        {
                            errorMsg += rowNumber + " dataTime is not existed";
                        }


                        long intTem;
                        //if (Int64.TryParse(line.Trunk, out intTem))
                        if (Int64.TryParse(lineArray[1], out intTem))
                        {
                            record.Subscriber = intTem.ToString();
                        }
                        else
                        {
                            //errorMsg += "----row number ("+ rowNumber + ") subscriber (from:"+line.Trunk+") is not parseable.(price:"+line.Price+")";
                            errorMsg += "----row number (" + rowNumber + ") subscriber (from:" + lineArray[1] + ") is not parseable.(price:" + lineArray[8] + ")";
                        }
                        //record.Subscriber = line.From;
                        //record.Aprefix = line.CustomerId;// apprefix is used for CustomerId
                        record.Aprefix = lineArray[0];// apprefix is used for CustomerId
                        //record.Destination = line.To;
                        record.Destination = lineArray[4];
                        //record.Invoice_group = line.Type;
                        record.Invoice_group = lineArray[6];
                        record.Prefix = "-";
                        record.Pbx = "-";
                        record.Direction = "-";
                        //record.Volume_time_secs = line.Duration;
                        record.Volume_time_secs = lineArray[7];
                        //record.Price = line.Price;
                        record.Price = lineArray[8];
                        record.Free = "-";
                        record.Forward = "-";
                        //record.Servingnetwork = line.Trunk;// serviceing netwrok is used for the trunk.
                        record.Servingnetwork = lineArray[1];// serviceing netwrok is used for the trunk.
                        record.Reason = "-";
                        record.Billed = "-";
                        record.RPBilled = "No";

                        //string[] rst = GetUnitelZoneName(line.BillingZonePrefix, line.BillingZoneDescription, priceFile);
                        string[] rst = GetUnitelZoneName(lineArray[10], lineArray[11], priceFile);
                        string error = rst[1];
                        if (!string.IsNullOrEmpty(error))
                        {
                            errorMsg += " --line number: " + rowNumber + rst[1] ;
                        }
                        else
                        {
                            record.ZoneName = rst[0];
                        }

                        invoiceRecords.Add(record);
                    }
                }
                else
                {
                    errorMsg += "Supplier is not supported";
                }                
            }
            catch(Exception ex)
            {
                //csv.Dispose();
                errorMsg += ex.ToString();
            }
            //csv.Dispose();

            return new Dictionary<ICollection<InvoiceRecords>, string> {{ invoiceRecords, errorMsg }};

        }

        public string[] GetUnitelZoneName(string zonePrefix, string zoneDescription, CSVFile unitelPriceFile)
        {
            string errorMsg = string.Empty;
            string prefix = string.Empty;
            string zone = string.Empty;

            int i = 0;
            while (i < zonePrefix.Length)
            {                
                string temCode = zonePrefix.Substring(0, zonePrefix.Length - i);
                if (unitelPriceFile.ZoneRecords.Any(x => x.Country_code == temCode ))
                {
                    prefix = temCode;                    
                    break;
                }
                i++;
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                try
                {                    

                    if (!zoneDescription.ToLower().Contains("mobil"))
                    {
                        zone = unitelPriceFile.ZoneRecords.First(x => x.Country_code == prefix && !x.Name.Contains("Mobil")).Name;
                    }
                    else
                    {
                        zone = unitelPriceFile.ZoneRecords.First(x => x.Country_code == prefix && x.Name.Contains("Mobil")).Name;
                    }
                }
                catch
                {
                    errorMsg = " prefix ("+prefix+") cannot be found to get the Zonename. ";
                }
                
            }
            else
            {
                errorMsg = " prefix ("+ prefix +") cannot be converted to country code to find the Zonename. ";
            }

            return new string[] { zone, errorMsg };
        }

        ICollection<Models.ZoneRecords> HandleZone(string path, int supplierId)
        {
            List<Models.ZoneRecords> zoneList;
            var csv = new CsvReader(new StreamReader(path, Encoding.GetEncoding("windows-1254")));
            try
            {
                string supplierName = fileRepository.GetSupplier(supplierId).Name;
                zoneList = new List<Models.ZoneRecords>();
                Models.ZoneRecords zone;                

                if (supplierName == "IPVision")
                {
                    var priceFile = csv.GetRecords<IPVisionPriceFile>();
                    foreach(var line in priceFile)
                    {                       
                        zone = new Models.ZoneRecords();
                        zone.Name = line.zone;
                        zone.Call_price = line.price_call;
                        zone.Minute_price = line.price_minute;
                        zoneList.Add(zone);
                    }
                }
                if (supplierName == "Unitel")
                {
                    var temp = csv.GetRecords<UnitelPriceFile>();
                    foreach (var line in temp)
                    {
                        zone = new Models.ZoneRecords();
                        zone.Name = line.zone;
                        zone.Call_price = line.price_call;
                        zone.Minute_price = line.price_minute;
                        zone.Country_code = line.country_code;

                        zoneList.Add(zone);
                    }
                }
            }
            catch (Exception ex)
            {
                csv.Dispose();
                throw ex;
            }

            return zoneList;
        }

        // GET: File/Delete/5
        public ActionResult DeleteCsvFile(int fileId, string fileType)
        {
            try
            {
                using (var db = new DBModelsContainer())
                {
                    var csvFile = db.CSVFileSet.Where(f => f.Id == fileId).FirstOrDefault<CSVFile>();
                    if (csvFile != null)
                    {
                        bool x = false;
                        try
                        {
                            if (fileType == "InvoiceFile")
                            {
                                foreach (InvoiceRecords item in csvFile.InvoiceRecords.ToList())
                                {
                                    db.InvoiceRecordsSet.Remove(item);
                                    db.SaveChanges();
                                }
                                x = true;
                            }
                            else if(fileType == "PriceFile")
                            {
                                foreach(Models.ZoneRecords item in csvFile.ZoneRecords.ToList())
                                {
                                    db.ZoneRecordsSet.Remove(item);
                                    db.SaveChanges();
                                }
                                x = true;
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }

                        if (x == true)
                        {
                            string name = csvFile.Name;
                            db.CSVFileSet.Remove(csvFile);
                            db.SaveChanges();
                            string msg = name +" is deleted successfully.";
                            if (fileType == "InvoiceFile")
                            {
                                return RedirectToAction("ViewInvoiceFiles", new RouteValueDictionary(new { controller = "File", action = "ViewInvoiceFiles", msg = msg }));
                            }
                            else
                            {
                                return RedirectToAction("ViewPriceFiles", new RouteValueDictionary(new { controller = "File", action = "ViewPriceFiles", msg = msg }));
                            }
                        }
                        else
                        {
                            return View("Error");
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return RedirectToAction("Error");
        }
        
        public ActionResult BillCsvFile(int fileId)
        {
            var recordList = fileRepository.GetFileInvoiceDetails(fileId).Where(x => x.RPBilled == "No").ToList();
            var sId = recordList.FirstOrDefault().CSVFile.SupplierId;
            var typeId = fileRepository.GetType("PriceFile").Id;
            var files = fileRepository.GetCsvFileByTypeId(typeId);
            //var files = fileRepository.GetPriceFiles(sId);
            var fileName = files.SingleOrDefault(f => f.SupplierId == sId && f.TypeId== typeId).Name;// here supplier id should be equal to the sId
            
            // apply agreement now
            var billableList = ApplyAgrreement(recordList, fileName);            

            if (notVerifiedList.Count() > 0)
            {
                double totalPrice = 0;
                string error = "There is no agreement for the following Subscribers." +                    
                    "/ CustomerId ; Trunk ; From ; Price ; rowsnumber / ";
                
                foreach ( var item in notVerifiedList)
                {
                    // apprefix is used for saving customerId and servingnetwork is used to save the trunk.
                    error +=  item.Key.Aprefix + " ; " + item.Key.Servingnetwork + " ; "+ item.Key.Subscriber + " ; "  + item.Key.Price.Replace("." , ",") + " ; " + item.Value + " /";
                    totalPrice += Convert.ToDouble(item.Key.Price.Replace(".",","));
                }

                error += "/ TotalPrice ;  ;  ; " + totalPrice ;
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            }
                        
            if(notExistedZoneList.Count > 0)
            {
                string error = "Supplier does not provide the following Zones:";
                int i = 1;
                foreach (var item in notExistedZoneList)
                {
                    error += i + "- Subscriber " + item.Subscriber + " - " + item.ZoneName + "\n . ";
                    i++;
                }
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            }

            if (billableList.Count > 0)
            {
                var ab = Accumulate(billableList);
                string msg = string.Empty;
                List<string> errorMsg = InvoiceGenerator.Bill(ab);
                if (errorMsg.Count == 0)
                {
                    List<InvoiceRecords> notSavedInDB = new List<InvoiceRecords>();
                    using (var db = new DBModelsContainer())
                    {
                        foreach (var invoice in recordList)
                        {
                            try
                            {
                                var result = db.InvoiceRecordsSet.Where(x => x.Id == invoice.Id).FirstOrDefault();
                                result.RPBilled = "Yes";
                                db.SaveChanges();
                            }
                            catch
                            {
                                notSavedInDB.Add(invoice);
                            }
                        }

                        if (notSavedInDB.Count > 0)
                        {
                            string error = "the following subscriber is not updated in the db";
                            foreach (var invoice in notSavedInDB)
                            {
                                error += invoice;
                            }
                            return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (notSupplierPriceMachedList.Count > 0)
                            {
                                //return the error page
                                string error = "CSV file records are pushed to NAV. Prices that doesn't match criteria found.<br />";
                                error += "Price does not match with the supplier price for the following subscribers:<br /><br />";
                                int i = 1;
                                foreach (string errorMsgPrice in notSupplierPriceMachedList)
                                {
                                    error += i + ". " + errorMsgPrice + "<br />";
                                    i++;
                                }
                                //return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
                                return Content(error, "text/html");
                            }
                            else
                            {
                                msg = recordList.Count + "item is successfully invoiced to the Navision. and database is updated.";
                                return RedirectToAction("ViewInvoiceFiles", new RouteValueDictionary(new { controller = "File", action = "ViewInvoiceFiles", msg = msg }));
                            }
                        }
                    }
                }
                else
                {
                    int i = 0;
                    foreach (string error in errorMsg)
                    {
                        i += 1;
                        msg += i + "-" + error + "; ";
                    }
                    return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
                }
            }

            return View("Error");
        }

        

        Dictionary<InvoiceRecords, string> notVerifiedList = new Dictionary<InvoiceRecords, string>();        
        List<string> notSupplierPriceMachedList = new List<string>();
        List<InvoiceRecords> notExistedZoneList = new List<InvoiceRecords>();
        public List<InvoiceModel> ApplyAgrreement(List<InvoiceRecords> recordList, string fileName)
        {            
            List<InvoiceModel> appliedAgreements = new List<InvoiceModel>();
            int csvFileId = fileRepository.GetFileByName(fileName).Id;
            var agreementList = agreementRepository.GetAgreements(csvFileId).ToList();
            int rowNumber = 1;
            foreach (InvoiceRecords record in recordList)
            {
                rowNumber += 1;
                //check if there is an agreement for each record, else added to not verified list.
                if (agreementList.Any(x => Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(record.Subscriber)
                                                            && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(record.Subscriber)))
                {
                    var tempAgreement = agreementList.Single(x => Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(record.Subscriber)
                                                               && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(record.Subscriber));
                   

                    //check if the record zone name is in the priceList. else added to the not existed zone list
                    if (fileRepository.GetFileByName(fileName).ZoneRecords.Any(x => x.Name == record.ZoneName))
                    {
                        // get the zone from the priceFile 
                        var tempZone = fileRepository.GetFileByName(fileName).ZoneRecords.SingleOrDefault(x => x.Name == record.ZoneName);

                        //check if the record price is matched with the suppler price, else added to notMatched PriceList.
                        var rst = CheckSupplierPrice(tempZone, record, rowNumber);
                        bool tjeked = rst.Keys.ElementAt(0);

                        if (tjeked == false)
                        {
                            notSupplierPriceMachedList.Add(rst[rst.Keys.ElementAt(0)]);
                        }

                        if (tjeked == true)
                        {
                            // push to nav without price check
                        }

                        InvoiceModel temInvoice = new InvoiceModel();
                        temInvoice.CVR = tempAgreement.Customer_cvr;
                        //InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
                        //{
                        //    //Id = record.Id,
                        //    StartDate = Convert.ToDateTime(record.Time),
                        //    EndDate = Convert.ToDateTime(record.Time),
                        //    Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
                        //    Subscriber_Range_End = tempAgreement.Subscriber_range_end,
                        //    Agreement_Description = tempAgreement.Description
                        //};
                        //var tempAgreementZone = tempAgreement.ZoneRecords.Single(x => x.Name == record.ZoneName);

                        //ZoneLinesModel temZone = new ZoneLinesModel()
                        //{
                        //    ZoneName = tempAgreementZone.Name,
                        //    ZoneCalls = 1,
                        //    ZoneCallNo = "10036",
                        //    ZoneSeconds = Convert.ToInt32(record.Volume_time_secs),
                        //    ZoneMinuteNo = "10037",
                        //    ZonePriceMinute = tempAgreementZone.Minute_price,
                        //    ZonePriceCall = tempAgreementZone.Call_price
                        //};

                        try
                        {
                            var existedInvoice = appliedAgreements.Where(x => x.CVR == temInvoice.CVR).FirstOrDefault();
                            //check if there is an invoice for this record. else create a new invoice for the record
                            if (existedInvoice == null)
                            {
                                temInvoice.LineCollections = new List<InvoiceLineCollectionModel>();

                                InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
                                {
                                    //Id = record.Id,
                                    StartDate = Convert.ToDateTime(record.Time),
                                    EndDate = Convert.ToDateTime(record.Time),
                                    Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
                                    Subscriber_Range_End = tempAgreement.Subscriber_range_end,
                                    Agreement_Description = tempAgreement.Description
                                };
                                var tempAgreementZone = tempAgreement.ZoneRecords.Single(x => x.Name == record.ZoneName);
                                ZoneLinesModel temZone = new ZoneLinesModel()
                                {
                                    ZoneName = tempAgreementZone.Name,
                                    ZoneCalls = 1,
                                    ZoneCallNo = "10036",
                                    ZoneSeconds = Convert.ToInt32(record.Volume_time_secs),
                                    ZoneMinuteNo = "10037",
                                    ZonePriceMinute = tempAgreementZone.Minute_price,
                                    ZonePriceCall = tempAgreementZone.Call_price
                                };

                                invoiceLine.ZoneLines = new List<ZoneLinesModel>();
                                invoiceLine.ZoneLines.Add(temZone);
                                temInvoice.LineCollections.Add(invoiceLine);
                                appliedAgreements.Add(temInvoice);

                            }
                            else
                            {

                                var rangeExisted = existedInvoice.LineCollections.Find(a => Convert.ToInt64(a.Subscriber_Range_Start) <= Convert.ToInt64(record.Subscriber)
                                                                                          && Convert.ToInt64(a.Subscriber_Range_End) >= Convert.ToInt64(record.Subscriber));

                                //check if there is any invoiceLine within the subscriber range. else create a new invoiceLine(within subscriber range) for the customer.
                                if (rangeExisted == null)
                                {
                                    InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
                                    {
                                        //Id = record.Id,
                                        StartDate = Convert.ToDateTime(record.Time),
                                        EndDate = Convert.ToDateTime(record.Time),
                                        Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
                                        Subscriber_Range_End = tempAgreement.Subscriber_range_end,
                                        Agreement_Description = tempAgreement.Description
                                    };
                                    var tempAgreementZone = tempAgreement.ZoneRecords.Single(x => x.Name == record.ZoneName);
                                    ZoneLinesModel temZone = new ZoneLinesModel()
                                    {
                                        ZoneName = tempAgreementZone.Name,
                                        ZoneCalls = 1,
                                        ZoneCallNo = "10036",
                                        ZoneSeconds = Convert.ToInt32(record.Volume_time_secs),
                                        ZoneMinuteNo = "10037",
                                        ZonePriceMinute = tempAgreementZone.Minute_price,
                                        ZonePriceCall = tempAgreementZone.Call_price
                                    };

                                    foreach (var i in appliedAgreements)
                                    {
                                        if (i.CVR == temInvoice.CVR)
                                        {
                                            invoiceLine.ZoneLines = new List<ZoneLinesModel>();
                                            invoiceLine.ZoneLines.Add(temZone);
                                            i.LineCollections.Add(invoiceLine);
                                            //appliedAgreements.Add(i);
                                        }
                                    }
                                    // appliedAgreements.Select(x => { x.LineCollections.Add(invoiceLine); return x; }).ToList();                                       
                                }
                                else
                                {
                                    #region apply date
                                    DateTime StartDate;
                                    DateTime EndDate;

                                    if (DateTime.Compare(rangeExisted.StartDate, Convert.ToDateTime(record.Time)) < 0) //is earlier than
                                    {
                                        StartDate = rangeExisted.StartDate;
                                    }
                                    else
                                    {
                                        StartDate = Convert.ToDateTime(record.Time);
                                    }

                                    if (DateTime.Compare(rangeExisted.EndDate, Convert.ToDateTime(record.Time)) > 0)// is later than
                                    {
                                        EndDate = rangeExisted.EndDate;
                                    }
                                    else
                                    {
                                        EndDate = Convert.ToDateTime(record.Time);
                                    }
                                    #endregion
                                    //appliedAgreements.Select(x => { x.LineCollections.Where(w => w.Agreement_Description == rangeExisted.Agreement_Description).Select(a => { a.StartDate = invoiceLine.StartDate; a.EndDate = invoiceLine.EndDate; a.ZoneLines.Add(temZone); return a; }).ToList(); return x; }).ToList();

                                    var tempAgreementZone = tempAgreement.ZoneRecords.Single(x => x.Name == record.ZoneName);
                                    ZoneLinesModel temZone = new ZoneLinesModel()
                                    {
                                        ZoneName = tempAgreementZone.Name,
                                        ZoneCalls = 1,
                                        ZoneCallNo = "10036",
                                        ZoneSeconds = Convert.ToInt32(record.Volume_time_secs),
                                        ZoneMinuteNo = "10037",
                                        ZonePriceMinute = tempAgreementZone.Minute_price,
                                        ZonePriceCall = tempAgreementZone.Call_price
                                    };
                                    foreach (var i in appliedAgreements)
                                    {
                                        if (i.CVR == temInvoice.CVR)
                                        {
                                            foreach (var ii in i.LineCollections)
                                            {
                                                if (Convert.ToInt64(ii.Subscriber_Range_Start) <= Convert.ToInt64(record.Subscriber)
                                                  && Convert.ToInt64(ii.Subscriber_Range_End) >= Convert.ToInt64(record.Subscriber))
                                                {
                                                    ii.StartDate = StartDate;
                                                    ii.EndDate = EndDate;
                                                    ii.ZoneLines.Add(temZone);
                                                }
                                            }
                                        }
                                    }
                                    //appliedAgreements.Select(x => { x.LineCollections.Where(w => w.Agreement_Description == rangeExisted.Agreement_Description).Select(a => { a.StartDate= invoiceLine.StartDate; a.EndDate= invoiceLine.EndDate; a.ZoneLines.Add(temZone); return a; }).ToList(); return x; }).ToList();

                                }

                            }
                        }
                        catch
                        {
                            throw;
                        }

                    }
                    else
                    {
                        notExistedZoneList.Add(record);
                    }

                }
                else
                {
                    if(notVerifiedList.Any(x=> x.Key == record))
                    {
                        InvoiceRecords key = notVerifiedList.FirstOrDefault(x => x.Key == record).Key;
                        string value = notVerifiedList.FirstOrDefault(x => x.Key == record).Value;
                        value += " , " + rowNumber;
                        notVerifiedList[key] = value;
                    }
                    else
                    {
                        notVerifiedList.Add(record, rowNumber.ToString());
                    }
                }
            }
            return appliedAgreements;
        }

        
        public Dictionary<bool, string> CheckSupplierPrice(ZoneRecords zone, InvoiceRecords record, int rowNumber)
        {
            bool status = false;
            string errorMsg = string.Empty;

            double expectedPrice = 0.00;
            double recordPrice = double.Parse(record.Price.Replace(',','.'), CultureInfo.InvariantCulture);
            if (record.Invoice_group.Equals("CompanyCall"))
            {
                if (recordPrice == expectedPrice)
                {
                    status = true;
                }
            }
            else
            {
                decimal value = (zone.Minute_price / 60) * Convert.ToDecimal(record.Volume_time_secs) + zone.Call_price;
                expectedPrice = Convert.ToDouble(String.Format("{0:0.00}", value));

                if (recordPrice == expectedPrice || recordPrice < expectedPrice)
                {
                    status = true;
                }
                else
                {
                    errorMsg = "lineNumber: " + rowNumber + " recordPrice "+ recordPrice + " not equal with expectedPrice " + expectedPrice ;
                }
            }
            return new Dictionary<bool, string> { { status, errorMsg} };
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
                        if (line.Accumulated.Any(x => x.ZoneName == record.ZoneName && Convert.ToDecimal(x.Subscriber) >= Convert.ToDecimal(line.Subscriber_Range_Start)
                                                                                        && Convert.ToDecimal(x.Subscriber) <= Convert.ToDecimal(line.Subscriber_Range_End)))
                        {                            
                            line.Accumulated.Where(x => x.ZoneName == record.ZoneName)
                                .Select(x =>
                                {                                   
                                    x.styk += 1;
                                    x.Seconds += Convert.ToInt32(record.ZoneSeconds);
                                    x.Total = (x.Seconds / 60) * record.ZonePriceMinute +(x.styk * record.ZonePriceCall);
                                    return x;
                                }).ToList();
                        }
                        else
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
                                    Minute_price = record.ZonePriceMinute,
                                    styk = 1,
                                    Total = (record.ZoneSeconds / 60) * record.ZonePriceMinute + (record.ZonePriceCall)
                                });
                        }
                    }
                }
            }
            return billableList;
        }
               
        #region some code to test.
        //List<InvoiceLineCollectionModel> accumulatedList = new List<InvoiceLineCollectionModel>();
        //public List<InvoiceModel> ApplyPrice(List<InvoiceRecords> validatedList)
        //{
        //    //accumulatedList = new List<AccumulatedModel>();
        //    List<InvoiceModel> appliedPriceList = new List<InvoiceModel>();
        //    List<InvoiceRecords> notAppliedPriceList = new List<InvoiceRecords>();
        //    var agreementList = agreementRepository.GetAgreements().ToList();
        //    foreach (InvoiceRecords record in validatedList)
        //    {
        //        var tempAgreement = agreementList.Single(x => Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(record.Subscriber)
        //                                                   && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(record.Subscriber));
        //        if (tempAgreement != null)
        //        {
        //            string FieName = "Uni-tel udlandspriser SIP - Kopi.csv";
        //            var tempZone = fileRepository.GetFileByName(FieName).ZoneRecords.Single(x => x.Name == record.ZoneName);
        //           if(Convert.ToDecimal(record.Price) == tempZone.Minute_price)
        //           {
        //                InvoiceModel temInvoice = new InvoiceModel();
        //                temInvoice.CVR = tempAgreement.Customer_cvr;
        //                InvoiceLineCollectionModel invoiceLine = new InvoiceLineCollectionModel()
        //                {
        //                    //Id = record.Id,
        //                    StartDate = Convert.ToDateTime(record.Time),
        //                    EndDate = Convert.ToDateTime(record.Time),
        //                    Subscriber_Range_Start = tempAgreement.Subscriber_range_start,
        //                    Subscriber_Range_End = tempAgreement.Subscriber_range_end,
        //                    Agreement_Description = tempAgreement.Description
        //                };

        //                var tempAgreementZone = tempAgreement.ZoneRecords.Single(x => x.Name == record.ZoneName);
        //                ZoneLinesModel temZone = new ZoneLinesModel()
        //                {
        //                    ZoneName = tempAgreementZone.Name,
        //                    ZoneCalls = 1,
        //                    ZoneCallNo = "10036",
        //                    ZoneSeconds = Convert.ToDecimal(record.Volume_time_secs),
        //                    ZoneMinuteNo = "10037",
        //                    ZonePriceMinute = tempAgreementZone.Minute_price,
        //                    ZonePriceCall = tempAgreementZone.Call_price
        //                };
                        
        //                try
        //                {
        //                    temInvoice.LineCollections = new List<InvoiceLineCollectionModel>();
        //                    invoiceLine.ZoneLines = new List<ZoneLinesModel>();

        //                    if(appliedPriceList.Any(x=> x.CVR == temInvoice.CVR))// if invoice is existed
        //                    {
        //                        var existedInvoice = appliedPriceList.Single(x=> x.CVR == temInvoice.CVR);
        //                        if (existedInvoice.LineCollections.Any(x=> x.Id == invoiceLine.Id))//If invoiceLine is existed.
        //                       //if (existedInvoice.LineCollections.Any(x => x.ZoneLines.Any(z => z.ZoneName == record.ZoneName)))
        //                       {
        //                            //add the zone to the existed line in the invoice.
        //                            appliedPriceList.Select(x => { x.LineCollections.Select(a => { a.ZoneLines.Add(temZone); return a; }).ToList(); return x; }).ToList();
        //                        }
        //                        else // If invoiceLine not existe, create a new invoiceLine 
        //                        {
        //                            invoiceLine.ZoneLines.Add(temZone);
        //                            appliedPriceList.Select(x => { x.LineCollections.Add(invoiceLine); return x; }).ToList();
        //                        }
        //                    }
        //                    else // if invoice is not existe.
        //                    {
        //                        invoiceLine.ZoneLines.Add(temZone);
        //                        temInvoice.LineCollections.Add(invoiceLine);
        //                        appliedPriceList.Add(temInvoice);
        //                    }
                            
        //                }
        //                catch (Exception)
        //                {
        //                    throw;
        //                }
        //           }
        //           else
        //           {
        //                //add to not price validate with agreement list.
        //                notAppliedPriceList.Add(record);
        //           }
        //        }


        //    }

        //    return appliedPriceList;
        //}

        //public List<InvoiceModel> ApplyagreementPrice(List<InvoiceRecords> validatedList)
        //{
        //    //List<AccumulatedModel> accumulatedList = new List<AccumulatedModel>();
        //    var agreementList = agreementRepository.GetAgreements().ToList();
        //    List<InvoiceModel> billableList = new List<InvoiceModel>();            
        //    foreach (InvoiceRecords record in validatedList) {

        //        InvoiceModel temInvoice = new InvoiceModel();                
        //        InvoiceLineCollectionModel invoiceLine;                
        //        foreach (Agreement agreement in agreementList)
        //        {
        //            if (Convert.ToInt64(record.Subscriber) >= Convert.ToInt64(agreement.Subscriber_range_start)
        //                && Convert.ToInt64(record.Subscriber) <= Convert.ToInt64(agreement.Subscriber_range_end))
        //            {
        //                try
        //                {
        //                    temInvoice.CVR = agreement.Customer_cvr;
        //                    invoiceLine = new InvoiceLineCollectionModel() { StartDate = Convert.ToDateTime(record.Time), EndDate = Convert.ToDateTime(record.Time), Subscriber_Range_Start = agreement.Subscriber_range_start, Subscriber_Range_End = agreement.Subscriber_range_end, Agreement_Description = agreement.Description };
        //                    //invoiceLine.Accumulated = new List<AccumulatedModel>();
        //                    //invoiceLine.Accumulated.Subscriber = record.Subscriber;

        //                    ZoneRecords zone = agreement.ZoneRecords.Where(x => x.Name == record.ZoneName).FirstOrDefault<ZoneRecords>();
        //                    ZoneLinesModel temZone = new ZoneLinesModel()
        //                    {
        //                        ZoneName = zone.Name,
        //                        ZoneCalls = 1,
        //                        ZoneCallNo = "10036",
        //                        ZoneSeconds = Convert.ToDecimal(record.Volume_time_secs),
        //                        ZoneMinuteNo = "10037", ZonePriceMinute = zone.Minute_price,
        //                        ZonePriceCall = zone.Call_price
        //                    };

        //                    temInvoice.LineCollections = new List<InvoiceLineCollectionModel>();
        //                    invoiceLine.ZoneLines = new List<ZoneLinesModel>();
        //                    InvoiceModel ti = billableList.Where(x => x.CVR == temInvoice.CVR).FirstOrDefault();
        //                    if(ti == null)// if invoice does not contains the line, make a new invoice
        //                    {                                
        //                        invoiceLine.ZoneLines.Add(temZone);
        //                        temInvoice.LineCollections.Add(invoiceLine);
        //                        billableList.Add(temInvoice);                                
        //                    }
        //                    else
        //                    {
        //                        InvoiceLineCollectionModel tlc = ti.LineCollections.Where(x => x.Id == invoiceLine.Id).FirstOrDefault();
        //                        if (tlc == null) //if the line is not existed in the list, create a line and add zone to it.
        //                        {
        //                            invoiceLine.ZoneLines.Add(temZone);
        //                            billableList.Select(x => { x.LineCollections.Add(invoiceLine); return x; }).ToList();                                    
        //                        }
        //                        else
        //                        {
        //                            //add the zone to the existed line in the invoice.
        //                            billableList.Select(x => { x.LineCollections.Select(a => { a.ZoneLines.Add(temZone); return a; }).ToList(); return x; }).ToList();
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw ex;
        //                }

                        
        //            }
        //            else
        //            {

        //            }
        //        }

        //        //add accumulating model
        //    }

        //    return billableList;
        //}        

        //public List<InvoiceRecords>[] Verify (int fileId)
        //{
        //    List<InvoiceRecords> verifiedList = new List<InvoiceRecords>();
        //    List<InvoiceRecords> notVerifiedList = new List<InvoiceRecords>();

        //    var agrrementList = agreementRepository.GetAgreements();
        //    var recordList = fileRepository.GetFileInvoiceDetails(fileId).Where(x => x.RPBilled == "No").ToList();

        //    foreach (InvoiceRecords record in recordList)
        //    {                
        //        if(agrrementList.Any
        //            (s=> Convert.ToInt64(s.Subscriber_range_start) <= Convert.ToInt64(record.Subscriber) 
        //             && Convert.ToInt64(s.Subscriber_range_end) >= Convert.ToInt64(record.Subscriber)))
        //        {
        //            verifiedList.Add(record);
        //        }
        //        else
        //        {
        //            notVerifiedList.Add(record);
        //        }
        //    }
        //    return new List<InvoiceRecords>[] { verifiedList, notVerifiedList};
        //}

        //public List<AccumulatedModel> AccumulatePrice(List<InvoiceRecords> verifiedList)
        //{
        //    List<AccumulatedModel> accumulatedList = new List<AccumulatedModel>();
        //    foreach (InvoiceRecords record in verifiedList)
        //    {                
        //        if(accumulatedList.Any(x => x.ZoneName == record.ZoneName))
        //        {
        //            accumulatedList.Where(x => x.ZoneName == record.ZoneName)
        //                .Select(x => { x.styk += 1;
        //                    x.Seconds += Convert.ToDecimal(record.Volume_time_secs);
        //                    return x;
        //                }).ToList();
        //        }
        //        else
        //        {
        //            accumulatedList.Add(
        //                new AccumulatedModel() {
        //                    Subscriber = record.Subscriber,
        //                    ZoneName = record.ZoneName,
        //                    styk = 1,
        //                    Seconds = Convert.ToDecimal(record.Volume_time_secs),
        //                    Minute_price = Convert.ToDecimal(record.Price),
        //                });
        //        }                
        //    }
        //    return accumulatedList;
        //}
        #endregion


    }
}
