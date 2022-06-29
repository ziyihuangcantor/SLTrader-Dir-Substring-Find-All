using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ActivityActionMarkerController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_ActivityActionMarker([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_ActivityActionMarker> list = new List<SL_ActivityActionMarker>();

            try
            {
                list = DataActivityActionMarker.LoadSLActivityActionMarkerByEntityId(entityId);
            }
            catch (Exception)
            {
                list = new List<SL_ActivityActionMarker>();
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateActivityActionMarker(decimal ActivityActionId, decimal ActivityActionMarkerId)
        {
            SL_ActivityActionMarker tempActivityAction = DataActivityActionMarker.LoadSLActivityActionMarkerByPK(ActivityActionId);

            tempActivityAction.SLActivityActionMarker = (int)ActivityActionMarkerId;

            DataActivityActionMarker.UpdateActivityActionMarker(tempActivityAction);

            return Extended.JsonMax(new[] { tempActivityAction }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ActivityActionMarkerDropDown(string entityId)
        {
            List<SL_ActivityActionMarker> list;

            try
            {
                list = DataActivityActionMarker.LoadSLActivityActionMarkerByEntityId(entityId);
            }
            catch (Exception)
            {
                list = new List<SL_ActivityActionMarker>();
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_ActivityActionMarker(SL_ActivityActionMarker ActivityActionMarker)
        {
            ActivityActionMarker.UserId = SessionService.User.UserId;

            DataActivityActionMarker.AddActivityActionMarker(ActivityActionMarker);

            return Extended.JsonMax(new[] { ActivityActionMarker }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ActivityActionMarker(SL_ActivityActionMarker ActivityActionMarker)
        {
            SL_ActivityActionMarker tempActivityActionMarker = DataActivityActionMarker.LoadSLActivityActionMarkerByPK(ActivityActionMarker.SLActivityActionMarker);

            tempActivityActionMarker.EntityType = ActivityActionMarker.EntityType;
            tempActivityActionMarker.Field = ActivityActionMarker.Field;
            tempActivityActionMarker.OperatorId = ActivityActionMarker.OperatorId;
            tempActivityActionMarker.Value = ActivityActionMarker.Value;
            tempActivityActionMarker.Description = ActivityActionMarker.Description;
            tempActivityActionMarker.Marker = ActivityActionMarker.Marker;
            tempActivityActionMarker.TradeType = ActivityActionMarker.TradeType;

            DataActivityActionMarker.UpdateActivityActionMarker(tempActivityActionMarker);

            return Extended.JsonMax(new[] { tempActivityActionMarker }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_ActivityActionMarker(SL_ActivityActionMarker ActivityActionMarker)
        {
            SL_ActivityActionMarker tempActivityActionMarker = DataActivityActionMarker.LoadSLActivityActionMarkerByPK(ActivityActionMarker.SLActivityActionMarker);


            DataActivityActionMarker.DeleteActivityActionMarker(tempActivityActionMarker);

            return Extended.JsonMax(new[] { tempActivityActionMarker }, JsonRequestBehavior.AllowGet);
        }    
    }
}