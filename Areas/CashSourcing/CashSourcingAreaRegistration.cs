using System.Web.Mvc;

namespace SLTrader.Areas.CashSourcing
{
    public class CashSourcingAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "CashSourcing";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "CashSourcing_default",
                "CashSourcing/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
