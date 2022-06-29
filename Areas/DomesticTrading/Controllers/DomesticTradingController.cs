using System.Web.Mvc;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{

    public class DomesticTradingController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
