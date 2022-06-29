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
    public class DepoManagementController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_DepoManagement([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, decimal? portfolioId)
        {
            List<SL_DepoManagemntExtendedProjection> boxList = new List<SL_DepoManagemntExtendedProjection>();

            ModelState.Clear();

            if (entityId.Equals(""))
                return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            try
            {
                boxList = portfolioId == null ?
                DataDepoManagement.LoadDepoManagement(effectiveDate, entityId) : DataDepoManagement.LoadPortfolioBox(effectiveDate, entityId, (decimal)portfolioId);

            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(boxList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Load_DepoManagementPartial([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            return PartialView("~/Areas/DomesticTrading/Views/DepoManagement/_DepoManagement.cshtml", entityId);
        }


        public ActionResult Create_DepoManagement([DataSourceRequest] DataSourceRequest request, SL_DepoManagement  depoManagement)
        {

            try
            {
                depoManagement.BoxLocation = string.IsNullOrWhiteSpace(depoManagement.BoxLocation) ? "" : depoManagement.BoxLocation;
                depoManagement.FirmId = string.IsNullOrWhiteSpace(depoManagement.FirmId) ? "" : depoManagement.FirmId;

                DataDepoManagement.AddDepoManagement(depoManagement);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { depoManagement }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Load_DepoManagement([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_DepoManagement> depoMangement = new List<SL_DepoManagement>();

            try
            {
                depoMangement = DataDepoManagement.LoadDepoManagement(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(depoMangement.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);            
        }

        public ActionResult Update_DepoManagement([DataSourceRequest] DataSourceRequest request, SL_DepoManagement depoManagement)
        {
            try
            {
                var depoManagementList = DataDepoManagement.LoadDepoManagement(depoManagement.EntityId);

                var depoManagementItem = depoManagementList.Where(x => x.SLDepoManagement == depoManagement.SLDepoManagement).First();

                depoManagementItem.BoxLocation = string.IsNullOrWhiteSpace(depoManagement.BoxLocation) ? "" : depoManagement.BoxLocation;
                depoManagementItem.FirmId = string.IsNullOrWhiteSpace(depoManagement.FirmId) ? "" : depoManagement.FirmId;
                depoManagementItem.IsBuyInMarket = depoManagement.IsBuyInMarket;
                depoManagement.Name = string.IsNullOrWhiteSpace(depoManagement.Name) ? "" : depoManagement.Name;

                DataDepoManagement.UpdateDepoManagement(depoManagementItem);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { depoManagement }.ToDataSourceResult(request, ModelState));
        }
    }
}
