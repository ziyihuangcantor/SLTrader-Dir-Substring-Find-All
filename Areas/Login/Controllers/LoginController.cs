using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BondFire.Entities;
using SLTrader.Helpers.SessionHelper;
using SLTrader.Models;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;
using SLTrader.Custom;

namespace SLTrader.Areas.Login.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Login(string userName, string password)
        {

            try
            {
                if (userName.ToString().Equals("") || password.ToString().Equals("")) { throw new Exception(); }

                DataUser.UserLogin(userName, password, HttpContext.Request.UserHostName, HttpContext.Request.UserHostAddress);

                DataUser.LoadUserPreference();

                MvcApplication._needTimer.Enabled = true;


            }
            catch (Exception error)
            {
                return ThrowJSONError(error);
            }

            return Json(SessionService.SecurityContext, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadUser()
        {
            try
            {
                SessionUserService.LoadUserFromDb(SessionService.SecurityContext);
            }
            catch
            {
                SessionService.User = new User();
            }

            return Json(SessionService.User, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadUserPreference()
        {
            try
            {
                SessionUserPreferenceService.LoadUserPreferenceFromDB(SessionService.SecurityContext);
            }
            catch (Exception error)
            {
                SessionService.UserPreference = new SL_UserPreference();                
            }

            return Json(SessionService.UserPreference, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadUserFirms()
        {
            try
            {
                SessionUserFirmService.LoadUserFirmFromDb(SessionService.SecurityContext);
            }
            catch(Exception e)
            {
                SessionService.UserFirms = new List<Company>();
            }

            return Json(SessionService.UserFirms, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDictionary()
        {
            try
            {
                SessionDictionaryService.LoadDictionaryFromDb(SessionService.SecurityContext);
            }
            catch
            {
                SessionService.UserDictionary = new List<SL_Dictionary>();
            }

            return Json(SessionService.UserDictionary, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadUserSecurityProfile()
        {
            try
            {
                SessionSecurityService.LoadSecurityFromDb(SessionService.SecurityContext);
            }
            catch (Exception e)
            {
                SessionService.UserSecurityProfile = new List<ManagerModel>();
            }

            return Json(SessionService.UserSecurityProfile, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public PartialViewResult LoadInformation()
        {
            return PartialView("~/Areas/Login/Views/Login/_LoginLoad.cshtml", SessionService.SecurityContext);
        }

        private JsonResult ThrowJSONError(Exception e)
        {
            Response.StatusCode = (int)HttpStatusCode.Conflict;

            return Json(new { Message = e.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}
