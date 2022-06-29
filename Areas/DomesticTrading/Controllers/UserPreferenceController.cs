using System.IO;
using System.Web;
using System.Web.Mvc;
using BondFire.Entities;
using SLTrader.Tools;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class UserPreferenceController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetProfileImage()
        {
            SL_UserPreference user = SessionService.UserPreference;
        
            byte [] image = user.Thumbnail;

            return File(image, "image/jpg");
        }

        public ActionResult UpdateProfileImage(HttpPostedFileBase profileImage)
       {

           SL_UserPreference user = SessionService.UserPreference;

           user.Thumbnail = ConvertToByte(profileImage);

           DataUser.AddUserPreference(user);

           return View();
       }

        [HttpPost]
        public bool UpdateProfile(SL_UserPreference userPreference)
        {
            bool success = false;

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

        private byte[] ConvertToByte(HttpPostedFileBase source)
        {
            MemoryStream target = new MemoryStream();
            source.InputStream.CopyTo(target);
            return target.ToArray();
        }
    }
}
