using System.Web.Mvc;

namespace SLTrader.Areas.Header
{
    public class HeaderAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Header";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Header_default",
                "Header/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
