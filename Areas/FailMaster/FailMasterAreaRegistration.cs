using System.Web.Mvc;

namespace SLTrader.Areas.FailMaster
{
    public class FailMasterAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "FailMaster";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "FailMaster_default",
                "FailMaster/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
