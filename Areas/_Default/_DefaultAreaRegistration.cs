using System.Web.Mvc;

namespace SLTrader.Areas._Default
{
    public class _DefaultAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "_Default";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "_Default_default",
                "_Default/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
