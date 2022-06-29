using System.Web.Mvc;
using System.Web.SessionState;
using Kendo.Mvc;

namespace SLTrader.Areas.RebateBilling.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class MenuController : Controller
    {
        [PopulateSiteMap(SiteMapName = "RebateBillingWebSiteMap", ViewDataKey = "RebateBillingWebSiteMap")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
