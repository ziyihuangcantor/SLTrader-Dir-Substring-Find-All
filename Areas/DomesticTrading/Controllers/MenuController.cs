using System.Web.Mvc;
using System.Web.SessionState;
using Kendo.Mvc;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class MenuController : BaseController
    {
        [PopulateSiteMap(SiteMapName = "DomesticTradingWebSiteMap", ViewDataKey = "DomesticTradingWebSiteMap")]
        public ActionResult Index()
        {
            
            return View();
        }
    }
}
