using System.Web.Mvc;

namespace SLTrader.Areas.DomesticTrading
{
    public class DomesticTradingAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DomesticTrading";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DomesticTrading_default",
                "DomesticTrading/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
