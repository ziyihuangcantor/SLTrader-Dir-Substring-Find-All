using System.Web.Mvc;

namespace SLTrader.Areas.Login.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public void LogClientError(string screen, string message)
        {
            DataError.LogError(Request.Browser.Browser, screen, message);
        }
    }
}
