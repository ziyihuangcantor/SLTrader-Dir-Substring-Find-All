using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Web.Mvc;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Helpers.SessionHelper;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;
using Newtonsoft.Json;
using SLTrader.Custom;

namespace SLTrader.Areas.Header.Controllers
{
    public class HeaderController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadValues([DataSourceRequest] DataSourceRequest request)
        {
            return Extended.JsonMax(DataSystemValues.LoadSystemValues().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetScratchPadLayout()
        {
            return Extended.JsonMax( SessionService.UserPreference.SLTradeLayoutTypeId.ToString(), JsonRequestBehavior.AllowGet );
        }

        public ActionResult UpdateValue([DataSourceRequest] DataSourceRequest request, SL_SystemValue valueItem)
        {
            var tempValue = DataSystemValues.LoadSystemValueByPK(valueItem.SLSystemValue);

            tempValue.Value = valueItem.Value;

            DataSystemValues.UpdateValue(tempValue);

            return Json(tempValue);
        }

        public PartialViewResult PartialViewContent(string url)
        {
            return PartialView(url);
        }

        public JsonResult UpdatePassword(string oldPassword, string newPassword)
        {
            var success = false;

            try
            {
                DataUser.UpdateUserPassword(oldPassword, newPassword);
                success = true;
            }
            catch
            {
                success = false;
            }

            return Json(success);
        }

        public JsonResult KeepAlive()
        {
            return Json(true);
        }

        public JsonResult LoadSystemValue(string name)
        {
            var value = DataSystemValues.LoadSystemValue(name, "");

            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ClearUser()
        {
            SessionDictionaryService.Clear();
            SessionSecurityService.Clear();
            SessionUserFirmService.Clear();
            SessionUserService.Clear();
            SessionUserPreferenceService.Clear();

            return Json(true);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public RedirectResult LoadApplication(SL_Application? application)
        {
            var mApp = new SL_Application();
            var url = "";

            if (application == null)
            {
                mApp = SessionService.UserPreference.Application;

                if ((Convert.ToInt32(mApp) == 0) || (mApp == SL_Application.Invalid))
                {
                    mApp = (SL_Application) Convert.ToInt32(SessionSecurityService.LoadApplications().First().Value);
                }
            }
            else
            {
                mApp = (SL_Application) application;
            }


            switch (mApp)
            {
                case SL_Application.Administration:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLAdminApp))
                    {
                        url = Url.Action("Index", "Admin", new {area = "Admin"});
                    }
                    break;

                case SL_Application.DomesticTrading:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLDomesticTradingApp))
                    {
                        url = Url.Action("Index", "DomesticTrading", new {area = "DomesticTrading"});
                    }
                    break;

                case SL_Application.DepoMonitor:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLDepoMonitorApp))
                    {
                        url = Url.Action("Index", "Monitor", new {area = "Monitor"});
                    }
                    break;

                case SL_Application.Dashboard:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLDashboardApp))
                    {
                        url = Url.Action("Index", "Dashboard", new {area = "Dashboard"});
                    }
                    break;

                case SL_Application.Locates:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLLocateApp))
                    {
                        url = Url.Action("Index", "Locates", new {area = "Locates"});
                    }
                    break;

                case SL_Application.RebateBilling:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLRebateBillingApp))
                    {
                        url = Url.Action("Index", "RebateBilling", new {area = "RebateBilling"});
                    }
                    break;

                case SL_Application.Compliance:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLComplianceApp))
                    {
                        url = Url.Action("Index", "Compliance", new { area = "Compliance" });
                    }
                    break;

                case SL_Application.CashSourcing:
                    if ( SessionSecurityService.IsEditable( ManagerTask.ViewSLCashSourcingApp ) )
                    {
                        url = Url.Action( "Index", "CashSourcing", new { area = "CashSourcing" } );
                    }
                    break;

                case SL_Application.FailMaster:
                    if (SessionSecurityService.IsEditable(ManagerTask.ViewSLDomesticTradingApp))
                    {
                        url = Url.Action("Index", "FailMaster", new { area = "FailMaster" });
                    }
                    break;

                default:
                    url = Url.Action("Index", "_Default", new {area = "_Default"});
                    break;
            }


            return new RedirectResult(url, true);
        }

        public PartialViewResult Read_Alert()
        {
            int count;

            try
            {
                var list = DataServerStatus.LoadAlert(DateTime.Today, SL_ActivityType.Alert);
                count = list.Count();
            }
            catch
            {
                count = 0;
            }

            return PartialView("~/Areas/Header/Views/Header/_Alerts.cshtml", count);
        }

        public PartialViewResult Read_Status()
        {
            var result = false;

            try
            {
                result = DataServerStatus.LoadServerStatus();
            }
            catch
            {
                result = false;
            }

            return PartialView("~/Areas/Header/Views/Header/_Status.cshtml", result);
        }

        public ActionResult GetProfileImage()
        {
            var user = SessionService.UserPreference;

            var image = user.Thumbnail;

            return File(image, "image/jpg");
        }

        public bool UpdateProfileImage(HttpPostedFileBase profileImage)
        {
            var success = false;

            try
            {
                var userPreference = SessionService.UserPreference;

               

                DataUser.AddUserPreference(userPreference);

                success = true;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        [HttpPost]
        public JsonResult UpdateProfile(SL_UserPreference userPreference)
        {
            bool success = false;

            try
            {
                if (userPreference.SLUserPreference == -1)
                {
                    userPreference.Thumbnail = new byte[0];
                    DataUser.AddUserPreference(userPreference);
                }
                else 
                {
                    SL_UserPreference _userPref = DataUser.LoadUserPreference(userPreference.UserId);
                    var previousSLSecurityLayoutTypeId = _userPref.SLSecurityLayoutTypeId;

                    _userPref.Thumbnail = new byte[0];
                    _userPref.Application = userPreference.Application;
                    _userPref.DefaultFirm = userPreference.DefaultFirm;
                    _userPref.ShowActivity = userPreference.ShowActivity;
                    _userPref.ShowOnStartUp = userPreference.ShowOnStartUp;
                    _userPref.ShowSecMaster = userPreference.ShowSecMaster;
                    _userPref.Customcss = userPreference.Customcss;
                    _userPref.FontFamily = userPreference.FontFamily;
                    _userPref.FontSize = userPreference.FontSize;
                    _userPref.SLTradeLayoutTypeId = userPreference.SLTradeLayoutTypeId;
                    _userPref.UserTypeId = userPreference.UserTypeId;
                    _userPref.ShowInventoryMarker = userPreference.ShowInventoryMarker;
                    _userPref.SLSecurityLayoutTypeId = userPreference.SLSecurityLayoutTypeId;
                    _userPref.ShowMatchingIssues = userPreference.ShowMatchingIssues;
                    _userPref.DefaultFirmMultiple = userPreference.DefaultFirmMultiple;
                    _userPref.RollupEntity = userPreference.RollupEntity;
                    _userPref.ReportingCurrency = userPreference.ReportingCurrency;

                    DataUser.UpdateUserPreference(_userPref);

                    if (userPreference.UserId == SessionService.UserPreference.UserId)
                    {
                        SessionUserPreferenceService.LoadUserPreferenceFromDB(SessionService.SecurityContext);

                        SessionService.UserPreference.SLSecurityLayoutTypeId = previousSLSecurityLayoutTypeId;
                    }
                }

                success = true;
            }
            catch(Exception e)
            {
                success = false;
            }

            return Json(success, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GetUserPreferencePartial()
        {
            var userPreference = SessionService.UserPreference;

            return PartialView("~/Areas/Header/Views/Header/_UserPreferences.cshtml", userPreference);
        }
        public PartialViewResult UpdateUserPasswordByUserIdPartial(int userId)
        {
            return PartialView("~/Areas/Header/Views/Header/_UserPasswordChange.cshtml", userId);
        }
        public JsonResult GetUserPreference()
        {
            var userPreference = SessionService.UserPreference;

            return Json(userPreference);
        }

        public JsonResult GetUserPreferenceMultiple()
        {
            return Json(SessionService.UserDefaultFrimMultiple);
        }

        [HttpPost]
        public bool Save(HttpPostedFileBase profileImage)
        {
            if (profileImage != null)
            {
                SL_UserPreference userPreference = SessionService.UserPreference;
                userPreference.Thumbnail = ConvertToByte(profileImage);

                DataUser.AddUserPreference(userPreference);
            }

            return true;
        }

        private static byte[] ConvertToByte(HttpPostedFileBase source)
        {
            var target = new MemoryStream();
            source.InputStream.CopyTo(target);
            return target.ToArray();
        }

        public JsonResult LoadUserLayout(string gridName)
        {
            SL_UserLayout userLayout = new SL_UserLayout();
            string layout;

            try
            {
                userLayout = DataUserLayout.LoadUserLayoutByGridNameAndUserId(gridName);
                layout = userLayout.Layout;
            }
            catch (Exception)
            {
                layout = "";
            }

            return Extended.JsonMax(layout, JsonRequestBehavior.AllowGet);    
        }

        public void UpdateUserLayout(string gridName, string options)
        {             
            try
            {
                var userLayout = DataUserLayout.LoadUserLayoutByGridNameAndUserId(gridName);

                userLayout.Layout = options;

                DataUserLayout.UpdateUserLayout(userLayout);
            }
            catch
            {
                var userLayout = new SL_UserLayout()
                {
                    GridName = gridName,
                    Layout = options,
                    UserId = SessionService.SecurityContext.UserId
                };

                DataUserLayout.AddUserLayout(userLayout);
            }
           
        }

        public void ResetUserLayout(string gridName)
        {
            try
            {
                SL_UserLayout _layOut =  DataUserLayout.LoadUserLayoutByGridNameAndUserId(gridName);

                DataUserLayout.DeleteUserLayout(_layOut);
            }
            catch (Exception)
            {
                
            }
        }
    }
}
