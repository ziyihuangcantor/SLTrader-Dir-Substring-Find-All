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
using SLTrader.Tools;
using SLTrader.Models;

namespace SLTrader.Areas.Compliance.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class CnsController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult Read_CnsExclusionList([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_CnsExclusionListProjection> cnsExclusionList = new List<SL_CnsExclusionListProjection>();

            try
            {
                if (!String.IsNullOrWhiteSpace(entityId))
                {
                    cnsExclusionList = DataCns.LoadCnsExclusionListByEntityId(entityId);
                }
            }
            catch
            {
                cnsExclusionList = new List<SL_CnsExclusionListProjection>();
            }

            return Extended.JsonMax(cnsExclusionList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_CnsBuyInAllocation([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string criteria)
        {
            List<SL_CnsBuyInAllocationProjection> cnsList = new List<SL_CnsBuyInAllocationProjection>();

            try
            {
                if ( !String.IsNullOrWhiteSpace( entityId ) )
                {
                    var issue = DataIssue.LoadIssue( criteria );

                    cnsList = DataCns.LoadCnsBuyAllocationByIssue( effectiveDate, entityId, issue.IssueId );
                }
            }
            catch
            {
                cnsList = new List<SL_CnsBuyInAllocationProjection>();
            }

            return Extended.JsonMax(cnsList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_CnsExposure([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_CnsExposureExtendedProjection> cnsList = new List<SL_CnsExposureExtendedProjection>();

            try
            {
                if (!String.IsNullOrWhiteSpace(entityId))
                {
                    cnsList = DataCns.LoadCnsExposure(effectiveDate, entityId);
                }
            }
            catch
            {
                cnsList = new List<SL_CnsExposureExtendedProjection>();
            }

            return Extended.JsonMax(cnsList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_CnsMonitor([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_CnsMonitorProjection> cnsList = new List<SL_CnsMonitorProjection>();

            try
            {
                if (!String.IsNullOrWhiteSpace(entityId))
                {
                    cnsList = DataCns.LoadCnsMonitor(effectiveDate, entityId);
                }
            }
            catch (Exception error)
            {
                string errorString = error.Message;

                cnsList = new List<SL_CnsMonitorProjection>();
            }

            return Extended.JsonMax(cnsList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_CnsBuyInAllocationIntradaySummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_CnsBuyInAllocationIntradaySummaryProjection> cnsList = new List<SL_CnsBuyInAllocationIntradaySummaryProjection>();

            try
            {
                if (!String.IsNullOrWhiteSpace(entityId))
                {
                    cnsList = DataCns.LoadCnsBuyAllocationIntradaySummary(effectiveDate, entityId);
                }
            }
            catch
            {
                cnsList = new List<SL_CnsBuyInAllocationIntradaySummaryProjection>();
            }

            return Extended.JsonMax(cnsList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_CnsProjection([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
       {
           List<SL_CnsExtendedProjection> cnsList = new List<SL_CnsExtendedProjection>();

           try
           {
               if (!String.IsNullOrWhiteSpace(entityId))
               {
                   cnsList = DataCns.LoadCnsProjection(effectiveDate, entityId);
               }
           }
           catch
           {
               cnsList = new List<SL_CnsExtendedProjection>();
           }

           return Extended.JsonMax(cnsList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
       }

       public ActionResult Read_TradeDetails([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
       {
           List<SL_TradeDetailExtendedProjection> tradeDetailList = new List<SL_TradeDetailExtendedProjection>();

           try
           {
               if (!String.IsNullOrWhiteSpace(entityId))
               {
                   tradeDetailList = DataTradeDetail.LoadTradeDetailByEffectiveDateAndEntity(effectiveDate, entityId);
               }
           }
           catch
           {
               tradeDetailList = new List<SL_TradeDetailExtendedProjection>();
           }

           return Extended.JsonMax(tradeDetailList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
       }

       public ActionResult Read_CnsAccount([DataSourceRequest] DataSourceRequest request, string entityId)
       {
           List<SL_CnsAccount> list;

           try
           {
               list = DataCnsAccount.LoadCnsAccountByEntityId(entityId);
           }
           catch (Exception)
           {
               list = new List<SL_CnsAccount>();
           }

           return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
       }

       public ActionResult Create_CnsAccount(SL_CnsAccount cnsAccount)
       {
           try
           {
               DataCnsAccount.AddCnsAccount(cnsAccount);
           }
           catch (Exception error)
           {
               return ThrowJsonError(error);
           }

           return Extended.JsonMax(new[] { cnsAccount }, JsonRequestBehavior.AllowGet);
       }

       public ActionResult Update_CnsAccount(SL_CnsAccount cnsAccount)
       {
           try
           {
               SL_CnsAccount tempCnsAccount = DataCnsAccount.LoadCnsAccountByPk(cnsAccount.SLCnsAccount);

               tempCnsAccount.AccountSubCategoryId = cnsAccount.AccountSubCategoryId;

               DataCnsAccount.UpdateCnsAccount(tempCnsAccount);
           }
           catch (Exception error)
           {
               return ThrowJsonError(error);
           }

           return Extended.JsonMax(new[] { cnsAccount }, JsonRequestBehavior.AllowGet);
       }

        public ActionResult Update_CnsMonitor(SL_CnsMonitorProjection cnsMonitor)
        {
            try
            {
                SL_IssueComment comment = new SL_IssueComment()
                {
                    EntityId = cnsMonitor.EntityId,
                    Comment = cnsMonitor.LastComment,
                    IssueId = cnsMonitor.IssueId,
                    UserId = SessionService.SecurityContext.UserId
                };

                DataIssue.AddIssueCOmment(comment);

                cnsMonitor.LastComment = comment.Comment;
                cnsMonitor.LastUserName = SessionService.SecurityContext.UserName;
            }
            catch (Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax(new[] { cnsMonitor }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_CnsAccount(SL_CnsAccount cnsAccount)
       {
           try
           {
               SL_CnsAccount tempCnsAccount = DataCnsAccount.LoadCnsAccountByPk(cnsAccount.SLCnsAccount);


               DataCnsAccount.DeleteCnsAccount(tempCnsAccount);
           }
           catch (Exception error)
           {
               return ThrowJsonError(error);
           }

           return Extended.JsonMax(new[] { cnsAccount }, JsonRequestBehavior.AllowGet);
       }

        public JsonResult Remove_CnsExclusionList(SL_CnsExclusionList item)
        {
            var _cnsExclusionList = new SL_CnsExclusionList();

            try
            {
                _cnsExclusionList = DataCns.LoadCnsExclusionListByPK(item.SLCnsExclusionList);

                _cnsExclusionList.StopDate = DateTime.Today;
                _cnsExclusionList.Comment = "Removed by " + SessionService.User.UserName;

                DataCns.UpdateCnsExclusionList(_cnsExclusionList);
            }
            catch
            {

            }

            return Extended.JsonMax(new[] { _cnsExclusionList }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Add_CnsExclusionList(string entityId, string criteria)
        {
            var _cnsExclusionList = new SL_CnsExclusionList();

            try
            {
                var issue = DataIssue.LoadIssue(criteria);

                if (issue == null)
                {
                    throw new Exception("Invalid criteria [" + criteria + "]");
                }

                _cnsExclusionList.EntityId = entityId;
                _cnsExclusionList.IssueId = issue.IssueId;
                _cnsExclusionList.StartDate = DateTime.Today;
                _cnsExclusionList.StopDate = null;
                _cnsExclusionList.Comment = "Added by " + SessionService.User.UserName;

                DataCns.AddCnsExclusionList(_cnsExclusionList);
            }
            catch(Exception e)
            {
                ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { _cnsExclusionList }, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadCnsExclusionListAdd(string entityId)
        {
            return PartialView("~/Areas/Compliance/Views/CnsAccount/_CnsExclusionListAdd.cshtml", entityId);
        }
    }
}