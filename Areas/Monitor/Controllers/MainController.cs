using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BondFire.Entities;
using SLTrader.Tools;

namespace SLTrader.Areas.Monitor.Controllers
{
    public class MainController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult Read_Alert()
        {
            int count;

            try
            {

                var mDataObject = DataServerStatus.LoadActivity(DateTime.Today);
                mDataObject = mDataObject.Where(x => x.ActivityType == SL_ActivityType.Alert).ToList();
                
                count = mDataObject.Count();
            }
            catch
            {
                count = 0;
            }

            return PartialView("~/Views/Shared/Header/_Alerts.cshtml", count);
        }

        public PartialViewResult Read_Status()
        {
            bool result;

            try
            {
                result = DataServerStatus.LoadServerStatus();               
            }
            catch
            {
                result = false;
            }

            return PartialView("~/Views/Shared/Header/_Status.cshtml", result);
        }

        public ActionResult GetProfileImage()
        {
            SL_UserPreference user = SessionService.UserPreference;

            byte[] image = user.Thumbnail;

            return File(image, "image/jpg");
        }

        public ActionResult UpdateProfileImage(HttpPostedFileBase profileImage)
        {
           /* using (var userService = AppHostBase.ResolveService<UserPreferenceService>(System.Web.HttpContext.Current))
            {
                SL_UserPreference user = (SL_UserPreference)Session["UserPreference"];
                SecurityContext securityContext = SessionService.SecurityContext;

                profileImage.InputStream.Read(user.Thumbnail, 0, profileImage.ContentLength);

                Requests.UserPreferenceAdd request = new Requests.UserPreferenceAdd();
                request.UserPreference = user;
                request.SecurityContext = securityContext;

                bool test = userService.Post(request).Result;
            }*/

            return View();
        }

        [HttpPost]
        public bool UpdateProfile(SL_UserPreference userPreference)
        {
            bool success;

            try
            {
                DataUser.AddUserPreference(userPreference);

                success = true;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public PartialViewResult GetUserPreferencePartial()
        {
            SL_UserPreference userPreference = SessionService.UserPreference;

            return PartialView("~/Views/Shared/Header/_UserPreferences.cshtml", userPreference);
        }

        [HttpPost]
        public ActionResult Save(HttpPostedFileBase profileImage)
        {
            if (profileImage != null)
            {

                SL_UserPreference user = SessionService.UserPreference;

                user.Thumbnail = ConvertToByte(profileImage);

                DataUser.AddUserPreference(user);
            }

            return Content("");
        }

        private static byte[] ConvertToByte(HttpPostedFileBase source)
        {
            var target = new MemoryStream();
            source.InputStream.CopyTo(target);
            return target.ToArray();
        }
    }
}
