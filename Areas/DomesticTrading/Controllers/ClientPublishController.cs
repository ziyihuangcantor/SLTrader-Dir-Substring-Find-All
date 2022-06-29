using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ClientPublishController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_ClientPublish([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_ClientPublish> list;

            try
            {
                list = DataClientPublish.LoadClientPubishByEntityId(entityId);
            }
            catch (Exception)
            {
                list = new List<SL_ClientPublish>();
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_ClientPublish(SL_ClientPublish ClientPublish)
        {
            DataClientPublish.AddClientPublish(ClientPublish);

            return Extended.JsonMax(new[] { ClientPublish }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ClientPublish(SL_ClientPublish ClientPublish)
        {
            SL_ClientPublish tempClientPublish =  DataClientPublish.LoadClientPublish(ClientPublish.SLClientPublish);

            tempClientPublish.ReportName = ClientPublish.ReportName;
            tempClientPublish.QuantityHaircut = ClientPublish.QuantityHaircut;
            tempClientPublish.QuantityMin = ClientPublish.QuantityMin;
            tempClientPublish.IsEnabled = ClientPublish.IsEnabled;
            tempClientPublish.Destination = ClientPublish.Destination;
            tempClientPublish.SecurityCriteria = ClientPublish.SecurityCriteria;
            tempClientPublish.OncePerDay = ClientPublish.OncePerDay;
            tempClientPublish.FileFormat = ClientPublish.FileFormat;
            tempClientPublish.Priority = ClientPublish.Priority;
            tempClientPublish.DaysToRun = ClientPublish.DaysToRun;
            tempClientPublish.RunOnHoliday = ClientPublish.RunOnHoliday;
            tempClientPublish.LastProcessDate = ClientPublish.LastProcessDate;

            DataClientPublish.UpdateClientPublish(tempClientPublish);

            return Extended.JsonMax(new[] { ClientPublish }, JsonRequestBehavior.AllowGet);
        }
    }
}