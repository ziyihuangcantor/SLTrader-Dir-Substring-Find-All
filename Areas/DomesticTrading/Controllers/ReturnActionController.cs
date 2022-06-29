using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Sql;
using System.Web.Mvc;
using System.Web.SessionState;
using SLTrader.Custom;
using SLTrader.Models;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ReturnActionController : BaseController
    {
        // GET: DomesticTrading/ReturnAction
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Destroy_ReturnBorrowCallback([DataSourceRequest] DataSourceRequest request, SL_CallBackExtendedProjection callBackItem)
        {           
            try
            {
                SL_Callback sLCallback = DataCallback.LoadCallbackByPK(callBackItem.SLCallback); ;

                sLCallback.ReturnStatusId = StatusMain.Cancelled;
                callBackItem.ReturnStatusId = StatusMain.Cancelled;

                DataCallback.UpdateCallBack(sLCallback);
            }
            catch (Exception exception)
            {
            }

            return Extended.JsonMax(new[] { callBackItem }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_BorrowReturnActionCallbackMultiSelect([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_CallBackExtendedProjection> callbackList = new List<SL_CallBackExtendedProjection>();
            try
            {
                foreach (string _Entityid in entityId)
                {
                    List<SL_CallBackExtendedProjection> borrowCallbackList = DataCallback.LoadCallbackExtended(effectiveDate, _Entityid);
                    if (!borrowCallbackList.Any<SL_CallBackExtendedProjection>((SL_CallBackExtendedProjection x) => x.TradeType == TradeType.StockBorrow))
                    {
                        continue;
                    }
                    callbackList.AddRange(
                        from x in borrowCallbackList
                        where x.TradeType == TradeType.StockBorrow
                        select x);
                }
            }
            catch (Exception exception)
            {
                callbackList = new List<SL_CallBackExtendedProjection>();
            }
            return Extended.JsonMax(QueryableExtensions.ToDataSourceResult(callbackList, request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_LoanReturnActionCallbackMultiSelect([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_CallBackExtendedProjection> callbackList = new List<SL_CallBackExtendedProjection>();
            try
            {
                foreach (string _Entityid in entityId)
                {
                    List<SL_CallBackExtendedProjection> loanCallbackList = DataCallback.LoadCallbackExtended(effectiveDate, _Entityid);
                    if (!loanCallbackList.Any<SL_CallBackExtendedProjection>((SL_CallBackExtendedProjection x) => x.TradeType == TradeType.StockLoan))
                    {
                        continue;
                    }
                    callbackList.AddRange(
                        from x in loanCallbackList
                        where x.TradeType == TradeType.StockLoan
                        select x);
                }
            }
            catch (Exception exception)
            {
                callbackList = new List<SL_CallBackExtendedProjection>();
            }
            return Extended.JsonMax(QueryableExtensions.ToDataSourceResult(callbackList, request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ReturnActionCallbacSummarykMultiSelect([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_CallBackSummaryExtendedProjection> callbackSummaryList = new List<SL_CallBackSummaryExtendedProjection>();
            try
            {
                foreach (string _Entityid in entityId)
                {
                    callbackSummaryList.AddRange(DataCallback.LoadCallbackSummaryExtended(effectiveDate, _Entityid));
                }
            }
            catch (Exception exception)
            {
                callbackSummaryList = new List<SL_CallBackSummaryExtendedProjection>();
            }
            return Extended.JsonMax(QueryableExtensions.ToDataSourceResult(callbackSummaryList, request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ReturnActionMultiSelect([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_ReturnActionExtendedProjection> returnActionList = new List<SL_ReturnActionExtendedProjection>();

            try
            {
                foreach (var _EntityId in entityId)
                {
                    returnActionList.AddRange(DataReturnAction.LoadReturnAction(effectiveDate, _EntityId));
                }
            }
            catch (Exception error)
            {
                returnActionList = new List<SL_ReturnActionExtendedProjection>();
            }

            return Extended.JsonMax(returnActionList.OrderByDescending(x => x.SecurityNumber).ThenByDescending(x => x.TradeType).ToList().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ReturnAction( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            List<SL_ReturnActionExtendedProjection> returnActionList;

            try
            {
                returnActionList = DataReturnAction.LoadReturnAction( effectiveDate, entityId );
            }
            catch
            {
                returnActionList = new List<SL_ReturnActionExtendedProjection>();
            }

            return Extended.JsonMax( returnActionList.OrderByDescending( x => x.SecurityNumber ).ThenByDescending( x => x.TradeType ).ToList().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ReturnActionCallbackMultiSelect([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_CallBackExtendedProjection> callbackList = new List<SL_CallBackExtendedProjection>();

            try
            {
                foreach (var _Entityid in entityId)
                {
                    callbackList.AddRange(DataCallback.LoadCallbackExtended(effectiveDate, _Entityid));
                }
            }
            catch (Exception error)
            {
                ThrowJsonError(error);
            }

            return Extended.JsonMax(callbackList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ReturnActionCallback( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            List<SL_CallBackExtendedProjection> callbackList = new List<SL_CallBackExtendedProjection>();

            try
            {
                callbackList = DataCallback.LoadCallbackExtended(effectiveDate, entityId);
            }
            catch (Exception error)
            {
                ThrowJsonError(error);
            }

            return Extended.JsonMax(callbackList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ReturnActionByCriteria( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string criteria )
        {
            List<SL_ReturnActionExtendedProjection> returnActionList;

            try
            {
                var issue = DataIssue.LoadIssue( criteria );
                returnActionList = DataReturnAction.LoadReturnActionByIssue( effectiveDate, entityId, issue.IssueId );
            }
            catch
            {
                returnActionList = new List<SL_ReturnActionExtendedProjection>();
            }

            return Extended.JsonMax( returnActionList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ReturnActionByIssue( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId, string contractNumber, string contraEntity )
        {
            List<SL_ReturnActionExtendedProjection> returnActionList = new List<SL_ReturnActionExtendedProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        returnActionList.AddRange(DataReturnAction.LoadReturnActionByIssue(effectiveDate, item, issueId));
                    }
                }
                else
                {
                    returnActionList = DataReturnAction.LoadReturnActionByIssue(effectiveDate, entityId, issueId);
                }
            }
            catch
            {
                returnActionList = new List<SL_ReturnActionExtendedProjection>();
            }

            return Extended.JsonMax( returnActionList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
    }
}