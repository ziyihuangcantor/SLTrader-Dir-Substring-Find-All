using System.Web.Mvc;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class StaticController : Controller
    {
        //
        // GET: /DomesticTrading/Static/

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult GetLoadingPartial()
        {
            return PartialView("~/Areas/DomesticTrading/Views/Shared/_LoadingTemplate.cshtml");
        }

    }
}
