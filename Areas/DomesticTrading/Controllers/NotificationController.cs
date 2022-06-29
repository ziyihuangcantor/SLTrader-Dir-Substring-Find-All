using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Models;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class NotificationController : BaseController
    {
        // GET: DomesticTrading/Notification
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_Notification([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_ClientEmailProjection> clientEmails = new List<SL_ClientEmailProjection>();

            try
            {
                if (entityId != null)
                    clientEmails = DataNotification.LoadClientEmailActions(entityId);
            }
            catch
            {
                clientEmails = new List<SL_ClientEmailProjection>();
            }

            return Extended.JsonMax(clientEmails.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}