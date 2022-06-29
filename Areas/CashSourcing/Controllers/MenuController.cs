using System.Web.Mvc;
using System.Web.SessionState;
using Kendo.Mvc;
using SLTrader.Custom;


namespace SLTrader.Areas.CashSourcing.Controllers
{
    public class MenuController : BaseController
    {
        [PopulateSiteMap( SiteMapName = "CashSourcingWebSiteMap", ViewDataKey = "CashSourcingWebSiteMap" )]
        public ActionResult Index()
        {
            return View();
        }
    }
}
