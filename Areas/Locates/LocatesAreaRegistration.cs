using System.Web.Mvc;

namespace SLTrader.Areas.Locates
{
    public class LocatesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Locates";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Locates_default",
                "Locates/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
