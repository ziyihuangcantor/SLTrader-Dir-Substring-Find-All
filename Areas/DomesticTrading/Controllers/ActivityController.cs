using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ActivityController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetActivity([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var activityList = DataServerStatus.LoadActivity(effectiveDate);

            return Extended.JsonMax(activityList.OrderBy(x => x.SLActivity).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetActivityAlert()
        {
            var activityList = DataServerStatus.LoadAlert(DateTime.Now.Date, SL_ActivityType.Alert);

            return Extended.JsonMax(activityList.OrderBy(x =>x.SLActivity), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetActivityByEntityAndCriteria([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string criteria)
        {
            var activityList = DataServerStatus.LoadActivityByEntityAndSecurity(effectiveDate, entityId, criteria);

            return Extended.JsonMax(activityList.OrderBy(x => x.DateTimeId).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult GetActivityPartial()
        {
            return PartialView("~/Areas/DomesticTrading/Views/Activity/Index.cshtml");
        }

        public PartialViewResult LoadActivityByContract(SL_ContractExtendedProjection item)
        {
           var activityList = new List<SL_ActivityProjection>();

           try
           {
               activityList = DataContracts.LoadActivityByContract( item );
           }
           catch ( Exception e )
           {
               return PartialView( "~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message );
           }

           return PartialView("~/Areas/DomesticTrading/Views/Activity/_ActivityByTypeId.cshtml", activityList.OrderBy(x => x.SLActivity));
        }

    }
}
