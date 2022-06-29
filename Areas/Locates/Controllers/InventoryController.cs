using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Models;
using SLTrader.Custom;
using SLTrader.Tools;

namespace SLTrader.Areas.Locates.Controllers
{   
    public class InventoryController : BaseController
    {
        //
        // GET: /Locates/Inventory/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult LoadLocateInventoryByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entity, string criteria)
        {
            var invList = new List<SL_LocateInventorySummaryProjection>();

            if (criteria.Equals(""))
                return Extended.JsonMax(invList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            var issue = DataIssue.LoadIssue(criteria);

            invList = DataInventory.LoadLocateInventoryByIssue(effectiveDate, entity, issue.IssueId,SessionService.SecurityContext).OrderByDescending(x => x.EffectiveDate).ToList();

            invList = invList.Where(x => x.EffectiveDate == effectiveDate).ToList();

            return Extended.JsonMax(invList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadInventoryByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entity, string criteria)
        {
            var invList = new List<SL_InventoryProjection>();

            if (criteria.Equals(""))
                return Extended.JsonMax(invList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            var issue = DataIssue.LoadIssue(criteria);

            invList = DataInventory.LoadInventoryByIssue(effectiveDate, entity, issue.IssueId).OrderByDescending(x => x.EffectiveDate).ToList();

            invList = invList.Where(x => x.EffectiveDate == effectiveDate).ToList();

            return Extended.JsonMax(invList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadInventoryByLocate([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entity, string criteria)
        {
            var invList = new List<SL_InventoryProjection>();

            if (criteria.Equals(""))
                return Extended.JsonMax(invList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            var issue = DataIssue.LoadIssue(criteria);

            invList = DataInventory.LoadInventoryByIssue(effectiveDate, entity, issue.IssueId).OrderByDescending(x => x.EffectiveDate).ToList();

            invList = invList.Where(x => x.EffectiveDate == effectiveDate).ToList();

            return Extended.JsonMax(invList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadInventoryByIssueChart(string entity, string criteria)
        {
            var invList = new List<SL_InventoryProjection>();

            if (criteria.Equals("")) return Extended.JsonMax(invList, JsonRequestBehavior.AllowGet);
            var issue = DataIssue.LoadIssue(criteria);

            invList = DataInventory.LoadInventoryByIssue(DateTime.Today, entity, issue.IssueId);

            return Extended.JsonMax(invList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadClientInventoryFiles([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entity)
        {
            var clientList = new List<SL_InventorySummaryFileProjection>();

            if (!entity.Equals(""))
            {
                clientList = DataInventory.LoadClientFilesByEntity(effectiveDate, entity);
            }

            return Extended.JsonMax(clientList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadClientInventoryFileBySource([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entity, string source, decimal clientEmailAction)
        {
            var clientList = new List<SL_InventoryProjection>();

            if (!entity.Equals(""))
            {
                clientList = DataInventory.LoadInventoryBySource(effectiveDate, entity, source).Where(x => x.SLClientEmailActionId == clientEmailAction).ToList();
            }

            return Extended.JsonMax(clientList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

         public PartialViewResult ReadInventorySnapshotPartial([DataSourceRequest] DataSourceRequest request, string entityId, string issueId)
        {
            var model = new List<SL_InventoryProjection>();
            string path = "";


            try
            {
                if (!entityId.Equals(""))
                {
                    var issue = DataIssue.LoadIssue(issueId);
                    model = DataInventory.LoadInventoryByIssue(DateTime.Today, entityId, issue.IssueId).OrderByDescending(x => x.EffectiveDate).ToList();
                }
            }
            catch
            {
                model = new List<SL_InventoryProjection>();
            }

            if (SessionService.UserPreference.SLSecurityLayoutTypeId == BondFire.Entities.SL_SecurityLayoutType.TOP)
            {
                path = "~/Areas/Locates/Views/Inventory/_InventorySnapshotHorizontal.cshtml";
            }
            else
            {
                path = "~/Areas/Locates/Views/Inventory/_InventorySnapshot.cshtml";
            }

            return PartialView(path, model);
        }

        public JsonResult Read_InventorySourceDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<string>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                response = DataNotification.LoadClientEmail(entityId).Where(x => !x.Source.Trim().Equals("")).Select(x => x.Source).Distinct().ToList();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult ClientAddInventoryMarkerPartial(string entityid, decimal SLCLientEmailActionId)
        {
            var dataItem = DataInventory.LoadClientFilesByEntity(DateTime.Today, entityid).Where(x => x.SLClientEmailActionId == SLCLientEmailActionId).First();

            return PartialView("~/Areas/Locates/Views/Inventory/_ClientEmailActionMarkerApplyPartial.cshtml", dataItem);
        }

        public PartialViewResult ClientAddInventoryExportPartial(IEnumerable<SL_InventorySummaryFileProjection> list)
        {
            var clientList = new List<SL_InventoryProjection>();

            if (list.Any())
            {
                foreach (var item in list)
                {
                    clientList.AddRange(DataInventory.LoadInventoryBySource(item.EffectiveDate, item.EntityId, item.Source).Where(x => x.SLClientEmailActionId == item.SLClientEmailActionId).ToList());
                }
            }

            InventoryListExportModel model = new InventoryListExportModel();

            model.fileProjectionList = list.ToList();
            model.inventoryProjectionList = clientList.ToList();

            return PartialView("~/Areas/Locates/Views/Inventory/_ClientEmailActionExportPartial.cshtml", model);
        }


        public PartialViewResult Delete_InventorySourcePartial(string entityId,string source, decimal SLCLientEmailActionId)
        {
            var response = new List<SL_LocateInventory>();

            var message = "";

            if (entityId.Equals(""))
            {
                return PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowErrorHelper.cshtml", "Missing EntityId!");
            }

            try
            {
                decimal inventoryCount = 0;

                if (SLCLientEmailActionId == -1)
                {
                    inventoryCount = DataInventory.LoadInventoryBySource(DateTime.Today, entityId, source).Count();
                }
                else
                {
                    inventoryCount = DataInventory.LoadInventoryBySource(DateTime.Today, entityId, source).Where(x => x.SLClientEmailActionId == SLCLientEmailActionId).Count();
                }
                

                var allocationCount = DataLocate.LoadLocateAllocation(entityId, source).Count();
                
                message = string.Format("Process will delete {0} inventory items and alter  {1} locate(s).", inventoryCount, allocationCount);
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowErrorHelper.cshtml", e.Message);
            }

            return PartialView("~/Areas/Locates/Views/Shared/Templates/_InventoryDelete.cshtml", new DeleteInventoryModel{ Message = message, EntityId = entityId, Source = source, SLCLientEmailActionId = SLCLientEmailActionId });
        }


        public JsonResult Delete_InventorySource(string entityId, string source, decimal SLCLientEmailActionId)
        {
            var response = new List<SL_LocateInventory>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                if (SLCLientEmailActionId == -1)
                {
                    response = DataLocate.DeleteLocateInventory(DateTime.Today, entityId, source);
                }
                else
                {
                    response = DataLocate.DeleteLocateInventory(DateTime.Today, entityId, source, SLCLientEmailActionId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult ReadInventoryAddPartial(string entityId)
        {
            return PartialView("~/Areas/Locates/Views/Locates/_InventoryList.cshtml", entityId);
        }
    }
}
