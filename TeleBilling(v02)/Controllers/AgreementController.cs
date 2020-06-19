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

namespace TeleBilling_v02_.Controllers
{
    public class AgreementController : Controller
    {
        IAgreementRepository agreementRepository;
        IFileRepository fileRepository;
        ICustomerInfo2Repository customerInfo2Repository;
        public AgreementController()
        {
            this.agreementRepository = new AgreementRepository(new DBModelsContainer());
            this.fileRepository = new FileRepository(new DBModelsContainer());

            CustomerInfo2_Service service = new CustomerInfo2_Service();
            service.Credentials= new NetworkCredential("rpnavapi", "Telefon1", "Gowingu");
            this.customerInfo2Repository = new CustomerInfo2Repository(service);
        }
        public AgreementController(IAgreementRepository agreementRepository, IFileRepository fileRepository, ICustomerInfo2Repository customerInfo2Repository)
        {
            this.agreementRepository = agreementRepository;
            this.fileRepository = fileRepository;
            this.customerInfo2Repository = customerInfo2Repository;
        }

        // GET: Agreement
        public ActionResult ViewAllAgreements()
        {
            var agreements = agreementRepository.GetAgreements();
            if(agreements == null)
            {
                return HttpNotFound();
            }

            return View(agreements);
        }

        public ActionResult ViewAgreementDetails(int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);

            return View(agreement);
        }

        public ActionResult ViewAgreementZones(int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);
            var agreementsZones = agreementRepository.GetAgreementZones(agreementId).ToList();

            return View(agreementsZones);
        }

        /*
        public ActionResult ViewDetails(int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);

            try
            {
                System.Web.HttpContext.Current.Session["sesAgreementId"] = agreementId.ToString();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            var priceFile_ZoneRecords = agreement.CSVFile.ZoneRecords.ToList();
            var agreements_ZoneRecords = agreement.ZoneRecords.ToList();
            AgreementDisplay display = new AgreementDisplay
            {
                Customer_cvr = agreement.Customer_cvr,
                Customer_name = agreement.Customer_name,
                Description = agreement.Description,
                Status = agreement.Status,
                Date = agreement.Date,
                Subscriber_range_start = agreement.Subscriber_range_start,
                Subscriber_range_end = agreement.Subscriber_range_end
            };

            display.ZoneRecords = new List<AgreementZoneRecords>();
            foreach (ZoneRecords zone in priceFile_ZoneRecords)
            {
                if (agreements_ZoneRecords.Any(x => x.Name == zone.Name))
                {
                    ZoneRecords az = agreements_ZoneRecords.Find(x => x.Name == zone.Name);

                    AgreementZoneRecords d = new AgreementZoneRecords();
                    d.Id = zone.Id;
                    d.Country_code = zone.Country_code;
                    d.Name = zone.Name;
                    d.Minute_price_Supplier = zone.Minute_price;
                    d.Call_price_Supplier = zone.Call_price;
                    d.Minute_price_RP = az.Minute_price;
                    d.Call_price_RP = az.Call_price;

                    display.ZoneRecords.Add(d);
                }
            }
           
            return View(display);
        }
        */

        [HttpPost]
        public ActionResult ViewZoneDetails()
        {
            string sBulkValue = "-1";

            if (Request.Form["NewBulkCallPriceRP"] != null)
            {
                sBulkValue = Request.Form["NewBulkCallPriceRP"].ToString();

                // db edit now
                System.Web.HttpContext.Current.Session["sesBulkCallRPValue"] = sBulkValue;
            }
            else
            {
                System.Web.HttpContext.Current.Session["sesBulkCallRPValue"] = "n/a";
                sBulkValue = "-1";
            }

            int agreementId = -1;
            if (System.Web.HttpContext.Current.Session["sesAgreementId"] != null)
            {
                agreementId = Convert.ToInt32(System.Web.HttpContext.Current.Session["sesAgreementId"]);
            }

            if (agreementId != -1)
            {
                if (sBulkValue != "-1")
                {
                    decimal dBulkValue = -1;

                    try
                    {
                        dBulkValue = Convert.ToDecimal(sBulkValue);
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        dBulkValue = -1;
                    }

                    if (dBulkValue != -1)
                    {
                        using (var db = new DBModelsContainer())
                        {
                            var ZoneRecordsSetResult = db.ZoneRecordsSet.Where(x => x.AgreementId == agreementId);
                            foreach (var ZoneRecordsSet in ZoneRecordsSetResult)
                            {
                                ZoneRecordsSet.Call_price = dBulkValue;
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }

            return RedirectToAction("ViewDetails", "Agreement", new { itemid = -1, agreementId = agreementId });
        }

        public ActionResult ViewDetails(int itemid, int agreementId)
        {
            Agreement agreement = agreementRepository.GetAgreement(agreementId);
            int iItemId = itemid;

            if (itemid == -1)
            {
                // do something
            }

            try
            {
                System.Web.HttpContext.Current.Session["sesAgreementId"] = agreementId.ToString();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            var priceFile_ZoneRecords = agreement.CSVFile.ZoneRecords.ToList();
            var agreements_ZoneRecords = agreement.ZoneRecords.ToList();
            AgreementDisplay display = new AgreementDisplay
            {
                Customer_cvr = agreement.Customer_cvr,
                Customer_name = agreement.Customer_name,
                Description = agreement.Description,
                Status = agreement.Status,
                Date = agreement.Date,
                Subscriber_range_start = agreement.Subscriber_range_start,
                Subscriber_range_end = agreement.Subscriber_range_end
            };

            display.ZoneRecords = new List<AgreementZoneRecords>();
            foreach (ZoneRecords zone in priceFile_ZoneRecords)
            {
                if (agreements_ZoneRecords.Any(x => x.Name == zone.Name))
                {
                    ZoneRecords az = agreements_ZoneRecords.Find(x => x.Name == zone.Name);

                    AgreementZoneRecords d = new AgreementZoneRecords();
                    d.Id = zone.Id;
                    d.Country_code = zone.Country_code;
                    d.Name = zone.Name;
                    d.Minute_price_Supplier = zone.Minute_price;
                    d.Call_price_Supplier = zone.Call_price;
                    d.Minute_price_RP = az.Minute_price;
                    d.Call_price_RP = az.Call_price;

                    display.ZoneRecords.Add(d);
                }
            }

            return View(display);
        }

        public ActionResult CreateAgreement()
        {
            Agreement agreement = new Agreement();
            agreement.Date = DateTime.Now;
            //agreement.AgreementZoneRecords = fileRepository.GetFileByName(fileId).AgreementZoneRecords;
            //List<Supplier> list = agreementRepository.GetSuppliers().ToList();
            //ViewBag.SupplierList = new SelectList(list, "Id", "Name");

            int typeId = fileRepository.GetType("PriceFile").Id;
            List<CSVFile> fileList = fileRepository.GetCsvFileByTypeId(typeId).ToList();
            ViewBag.FileList = new SelectList(fileList, "Id", "Name");
            List<CustomerInfo2> customerList = customerInfo2Repository.GetCustomers();
            ViewBag.CustomerList = new SelectList(customerList, "No", "Name");

            string username = Session["UserName"].ToString();

            if (username == "")
            {
                agreement.UserId = 5;
            }
            else
            {
                var user = fileRepository.GetUser(username);
                if (user != null)
                {
                    agreement.UserId = user.Id;
                }
                else
                {
                    return Json(new { success = false, message = "User not found!!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            return View(agreement);
        }

        ICollection<ZoneRecords> zoneReords = new List<ZoneRecords>();
        
        [HttpPost]
        public ActionResult Create(Agreement model)
        {
            
            model.Customer_name = customerInfo2Repository.GetCustomer(model.Customer_cvr).Name;
            zoneReords= fileRepository.GetFileZoneDetails(model.CSVFileId).ToList();

            bool existed = agreementRepository.GetAgreements().ToList().Any(x=> Convert.ToInt64(x.Subscriber_range_start) <= Convert.ToInt64(model.Subscriber_range_start)
                                                                             && Convert.ToInt64(x.Subscriber_range_end) >= Convert.ToInt64(model.Subscriber_range_end));
            
            if (zoneReords.Count >= 0) {

                if (zoneReords.Count > 0)
                {
                    foreach (ZoneRecords line in zoneReords)
                    {
                        ZoneRecords temp = new ZoneRecords
                        {
                            Name = line.Name,
                            Minute_price = line.Minute_price + ((line.Minute_price * 25) / 100),
                            Call_price = line.Call_price + ((line.Call_price * 25) / 100)
                        };

                        model.ZoneRecords.Add(temp);
                    }
                }

                using (var db = new DBModelsContainer())
                {
                    try
                    {
                        if (existed)
                        {
                            return Json(new { success = false, message = "the agreements is already in the database" }, JsonRequestBehavior.AllowGet);
                        }
                        db.AgreementSet.Add(model);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        //return Json(new { success = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }

                return RedirectToAction("ViewAllAgreements");
            }
            return View("Error");
        }

        public ActionResult DeActivateAgreement(int agreementId)
        {
            return View();
        }
    }
}