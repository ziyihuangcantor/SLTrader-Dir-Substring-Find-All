using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Tools;
using SLTrader.Custom;

namespace SLTrader.Areas.RebateBilling.Controllers
{
 
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class RebateBillingController : BaseController
    {      
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult LoadRebateBillingDefaultOptionsPartial(string entityId)
        {
            return PartialView("~/Areas/RebateBilling/Views/RebateBilling/_RebateBillingDefaultOptions.cshtml", entityId);
        }

        public JsonResult ProcessDefaultsUpdateRebateBilling(string entityId, float creditMarkup, float debitMarkup, string emailAddress, SL_RebateType rebateType, SL_RebateBillingType rebateBillingType)
        {
            try
            {
                DataSystemValues.UpdateValueByName("RebateBillingCreditMarkup_" + entityId, creditMarkup.ToString());
                DataSystemValues.UpdateValueByName("RebateBillingDebitMarkup_" + entityId, debitMarkup.ToString());
                DataSystemValues.UpdateValueByName("RebateBillingRebateType_" + entityId, rebateType.ToString());
                DataSystemValues.UpdateValueByName("RebateBillingType_" + entityId, rebateBillingType.ToString());
                DataSystemValues.UpdateValueByName("RebateBillingDefaultEmailAddress_" + entityId, emailAddress);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_RebateBillingSummary([DataSourceRequest] DataSourceRequest request, DateTime startDate, DateTime endDate, string entityId)
        {
            List<SL_RebateBillingSummaryProjection> summaryList = new List<SL_RebateBillingSummaryProjection>();

            try
            {
                summaryList = DataRebateBilling.LoadRebateBillingSummary(startDate, endDate, entityId);          
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(summaryList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_RebateBillingByGroupId([DataSourceRequest] DataSourceRequest request, DateTime startDate, DateTime endDate, string entityId, string groupId, bool summaryEnabled)
        {
            List<SL_RebateBillingItemProjection> summaryList = new List<SL_RebateBillingItemProjection>();

            try
            {
                if (summaryEnabled)
                {
                    summaryList = DataRebateBilling.LoadRebateBilling(startDate, endDate, entityId, groupId);
                }
                else
                {
                    summaryList = DataRebateBilling.LoadRebateBillingByEntity(startDate, endDate, entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(summaryList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ContractExtendedSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            try
            {
                contractList = DataRebateBilling.LoadRebateBillingContracts(effectiveDate, entityId);

                foreach(SL_ContractExtendedProjection item in contractList)
                {
                    item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, item.RebateRate, item.Amount, item.CostOfFunds, item.CollateralFlag);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(contractList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_FailExtendedSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_RebateBillingFailProjection> failList = new List<SL_RebateBillingFailProjection>();

            try
            {
                failList = DataRebateBilling.LoadRebateBillingFails(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(failList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_StockRecordExtendedSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_RebateBillingStockRecordProjection> stockRecordList;

            try
            {
                stockRecordList = DataRebateBilling.LoadRebateBillingStockRecord(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(stockRecordList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_RebateBillingIssueOverride([DataSourceRequest] DataSourceRequest request, string entityId, bool showExpired)
        {
            List<SL_RebateBillingIssueOverrideProjection> rebateBillingIssueOverrideList = new List<SL_RebateBillingIssueOverrideProjection>();

            try
            {
                rebateBillingIssueOverrideList = DataRebateBilling.LoadRebateBillingIssueOverride(entityId, showExpired);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(rebateBillingIssueOverrideList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadRebateBillingBulkUpdatePartial(List<SL_RebateBillingGroupAccountProjection> items)
        {
            return PartialView("~/Areas/RebateBilling/Views/Groups/_UpdateAccounts.cshtml", items);
        }

        public PartialViewResult LoadRebateBillingItemsBulkUpdatePartial(List<SL_RebateBillingItemProjection> items)
        {
            return PartialView("~/Areas/RebateBilling/Views/RebateBilling/_UpdateRebateBilling.cshtml", items);
        }

        public PartialViewResult LoadRebateBillingIssueOverridePartial(string entityid)
        {
            return PartialView("~/Areas/RebateBilling/Views/Groups/_AddIssueOverride.cshtml", entityid);
        }


        public JsonResult Read_Issue(string entityid, string criteria)
        {
            var issue = DataIssue.LoadIssue(criteria);


            return Extended.JsonMax(issue, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessAddIssueOverride(string entityId, string accountNumber, double? creditRate, double? debitRate, DateTime? startDate, DateTime? expireDate, int issueId)
        {
            try
            {
                SL_RebateBillingIssueOverride issueOverride = new SL_RebateBillingIssueOverride();

                issueOverride.EntityId = entityId;
                issueOverride.AccountNumber = string.IsNullOrEmpty(accountNumber) ? "" : accountNumber;

                issueOverride.CREDITRATE = creditRate;
                issueOverride.DEBITRATE = debitRate;

                issueOverride.StartDate = startDate;
                issueOverride.ExpireDate = expireDate;
                issueOverride.IssueId = issueId;
                issueOverride.UserId = SessionService.User.UserId;
                issueOverride.IsEnabled = true;

                DataRebateBilling.AddRebateBillingIssueOverride(issueOverride);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Update_RebateBillingIssueOverride([DataSourceRequest] DataSourceRequest request, SL_RebateBillingIssueOverrideProjection rebateGroup)
        {
            SL_RebateBillingIssueOverride _issueOverride =  new SL_RebateBillingIssueOverride();

            try
            {
                _issueOverride = DataRebateBilling.LoadRebateBillingIssueOverrideByPK(rebateGroup.SLRebateBilingIssueOverride);

                _issueOverride.CREDITRATE = (double)rebateGroup.CreditRate;
                _issueOverride.DEBITRATE = (double)rebateGroup.DebitRate;
                _issueOverride.StartDate = rebateGroup.StartDate;
                _issueOverride.ExpireDate = rebateGroup.ExpireDate;
                _issueOverride.IsEnabled = rebateGroup.IsEnabled;
                _issueOverride.UserId = SessionService.User.UserId;
                _issueOverride.DateTimeId = DateTime.Now;
                _issueOverride.AccountNumber = string.IsNullOrEmpty(rebateGroup.AccountNumber) ? "" : rebateGroup.AccountNumber;
                               
                DataRebateBilling.UpdateRebateBillingIssueOverride(_issueOverride);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { rebateGroup }.ToDataSourceResult(request, ModelState));
        }



        public ActionResult Update_RebateBillingItem([DataSourceRequest] DataSourceRequest request, SL_RebateBillingItemProjection rebateGroup)
        {
            SL_RebateBilling _rebateBillingItem = new SL_RebateBilling();

            try
            {
                _rebateBillingItem = DataRebateBilling.LoadRebateBillingItemByPK(rebateGroup.SLRebateBillingItem);

                if (_rebateBillingItem.QUANTITYALLOCATED != rebateGroup.QuantityAllocated)
                {
                    _rebateBillingItem.REBATEAMOUNT = (rebateGroup.QuantityAllocated * rebateGroup.Price) * rebateGroup.BillingRate / 100 / 360;

                    _rebateBillingItem.MARKUPREBATEAMOUNT = (rebateGroup.QuantityAllocated * rebateGroup.Price) * rebateGroup.MarkupBillingRate / 100 / 360;

                    rebateGroup.RebateAmount = _rebateBillingItem.REBATEAMOUNT;
                    rebateGroup.MarkUpRebateAmount = _rebateBillingItem.MARKUPREBATEAMOUNT;
                }

                if (_rebateBillingItem.BILLINGRATE != (double)rebateGroup.BillingRate)
                {
                    _rebateBillingItem.REBATEAMOUNT = (rebateGroup.QuantityAllocated * rebateGroup.Price) * rebateGroup.BillingRate / 100 / 360;

                    _rebateBillingItem.MARKUPREBATEAMOUNT = (rebateGroup.QuantityAllocated * rebateGroup.Price) * rebateGroup.MarkupBillingRate / 100 / 360;

                    rebateGroup.RebateAmount = _rebateBillingItem.REBATEAMOUNT;
                    rebateGroup.MarkUpRebateAmount = _rebateBillingItem.MARKUPREBATEAMOUNT;
                }

                _rebateBillingItem.QUANTITYALLOCATED = rebateGroup.QuantityAllocated;

                DataRebateBilling.UpdateRebateBillingItem(_rebateBillingItem);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { rebateGroup }.ToDataSourceResult(request, ModelState));
        }

        

    }
}