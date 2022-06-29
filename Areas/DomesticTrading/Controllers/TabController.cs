using System.Web.Mvc;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class TabController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

         
        public PartialViewResult RetrievePartial(string url)
        {
            return PartialView(url); 
        }
    }
}
