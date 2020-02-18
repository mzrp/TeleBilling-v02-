using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleBilling_v02_.Models;
using TeleBilling_v02_.Repository;

namespace TeleBilling_v02_.Controllers
{
    public class UserController : Controller
    {
        IUserRepository userRepository;
        public UserController() { this.userRepository = new UserRepository(new DBModelsContainer()); }
        public UserController(IUserRepository userRepository) { this.userRepository = userRepository; }

        // GET: User
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("ViewInvoiceFiles", "File");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                var rst = userRepository.Authenticat(username, password).First();
                bool auth = rst.Key;
                string msg = rst.Value;

                if (auth)
                {
                    try
                    {
                        Session["UserName"] = username;
                        Session["UserId"] = userRepository.GetUser(username).Id;
                        return RedirectToAction("ViewInvoiceFiles", "File");                  

                    }
                    catch
                    {
                        ViewBag.Message = "DB: User does not exist";
                    }
                }
                else
                {
                    ViewBag.Message = "AD:Username or password is wrong";
                    //return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
                }

            }
            return View();
        }    
    }
}