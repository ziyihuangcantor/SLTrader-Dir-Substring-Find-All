using System.Web.Mvc;

namespace SLTrader.Areas.Dashboard.Controllers
{
    public class RiskController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult GetActivityPartial()
        {
            return PartialView("~/Areas/Dashboard/Views/Risk/_AlertActivityGrid.cshtml");
        }

    }
}
