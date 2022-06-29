using System.Web.Mvc;

namespace SLTrader.Areas.RebateBilling
{
    public class RebateBillingAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "RebateBilling";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "RebateBilling_default",
                "RebateBilling/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}