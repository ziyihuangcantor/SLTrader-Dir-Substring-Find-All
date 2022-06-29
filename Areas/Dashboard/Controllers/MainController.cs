using System.Web.Mvc;

namespace SLTrader.Areas.Dashboard.Controllers
{
    public class MainController : Controller
    {
        public ActionResult Index()
        {
            return View("_Dashboard", null);
        }

        public PartialViewResult LoadContentPartial(string contentName)
        {
            return PartialView(contentName);
        }

        public PartialViewResult RetrievePartial(string url)
        {
            return PartialView(url);
        }
    }
}
