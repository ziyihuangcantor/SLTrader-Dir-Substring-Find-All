using System.Web.Mvc;
using System.Web.SessionState;
using SLTrader.Models;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ReturnController : BaseController
    {
        //
        // GET: /Return/

        public ActionResult Index()
        {
            return View();
        }
    }
}
