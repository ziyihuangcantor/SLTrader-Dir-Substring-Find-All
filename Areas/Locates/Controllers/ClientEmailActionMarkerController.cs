using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.Locates.Controllers
{
    public class ClientEmailActionMarkerController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_ClientEmailActionMarker([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_ClientEmailActionMarker> list;

            try
            {
                list = DataClientEmailActionMarker.LoadSLCLientEmailActionMarkerByEntityId(entityId);
            }
            catch (Exception)
            {
                list = new List<SL_ClientEmailActionMarker>();
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateClientEmailActionMarker(decimal clientEmailActionId, decimal clientEmailActionMarkerId)
        {
            SL_ClientEmailAction tempClientEmailAction = DataClientFile.LoadClientEmailActionByPK(clientEmailActionId);

            tempClientEmailAction.SLClientEmailActionMarkerId = (int)clientEmailActionMarkerId;

            DataClientFile.UpdateClientEmailAction(tempClientEmailAction);

            return Extended.JsonMax(new[] { tempClientEmailAction }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ClientEmailActionMarkerDropDown(string entityId)
        {
            List<SL_ClientEmailActionMarker> list;

            try
            {
                list = DataClientEmailActionMarker.LoadSLCLientEmailActionMarkerByEntityId(entityId);
            }
            catch (Exception)
            {
                list = new List<SL_ClientEmailActionMarker>();
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_ClientEmailActionMarker(SL_ClientEmailActionMarker clientEmailActionMarker)
        {
            DataClientEmailActionMarker.AddClientEmailActionMarker(clientEmailActionMarker);

            return Extended.JsonMax(new[] { clientEmailActionMarker }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ClientEmailActionMarker(SL_ClientEmailActionMarker clientEmailActionMarker)
        {
            SL_ClientEmailActionMarker tempClientEmailActionMarker = DataClientEmailActionMarker.LoadSLCLientEmailActionMarkerByPK(clientEmailActionMarker.SLClientEmailActionMarker);

            tempClientEmailActionMarker.Description = clientEmailActionMarker.Description;
            tempClientEmailActionMarker.Marker = clientEmailActionMarker.Marker;

            DataClientEmailActionMarker.UpdateClientEmailActionMarker(tempClientEmailActionMarker);

            return Extended.JsonMax(new[] { tempClientEmailActionMarker }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_ClientEmailActionMarker(SL_ClientEmailActionMarker clientEmailActionMarker)
        {
            SL_ClientEmailActionMarker tempClientEmailActionMarker = DataClientEmailActionMarker.LoadSLCLientEmailActionMarkerByPK(clientEmailActionMarker.SLClientEmailActionMarker);


            DataClientEmailActionMarker.DeleteClientEmailActionMarker(tempClientEmailActionMarker);

            return Extended.JsonMax(new[] { tempClientEmailActionMarker }, JsonRequestBehavior.AllowGet);
        }
    }
}