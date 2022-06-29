using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Tools.Helpers;
using SLTrader.Custom;
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class AutoActionController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_AutoBorrow([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var borrowOrderList = new List<SL_BorrowOrderProjection>();
            var removeCancelled = bool.Parse(DataSystemValues.LoadSystemValue("RemoveCancelled", "false"));

            ModelState.Clear();

            if (!entityId.Equals(""))
            {
                try
                {
                    borrowOrderList = DataAutoAction.LoadAutoBorrows(effectiveDate, entityId);
                    
                    if (removeCancelled)
                    {
                        borrowOrderList.RemoveAll(x => x.BorrowStatus == StatusMain.Cancelled);
                    }
                }
                catch (Exception e)
                {
                    DataError.LogError("", "Read_AutoBorrow", e.Message);
                }
            }

            var jsonResult = Json(borrowOrderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

            jsonResult.MaxJsonLength = int.MaxValue;
            
            return jsonResult;
        }

        public ActionResult UpdateAutoBorrow(SL_BorrowOrderProjection item)
        {

            try
            {
                SL_BorrowOrder orderTemp = DataAutoAction.LoadBorrowOrder(item.SLBorrowOrder);

                orderTemp.AddToLoanetIndicator = String.IsNullOrWhiteSpace(item.AddToLoanetId) ? "" : item.AddToLoanetId;
                orderTemp.BatchCode = String.IsNullOrWhiteSpace(item.BatchCode) ? "" : item.BatchCode;
                orderTemp.BorrowOrderRequest = String.IsNullOrWhiteSpace(item.BorrowOrderRequest) ? "" : item.BorrowOrderRequest; 
                orderTemp.BorrowOrderResponse = String.IsNullOrWhiteSpace(item.BorrowOrderResponse) ? "" : item.BorrowOrderResponse;
                orderTemp.BorrowOrderStatus = item.BorrowStatus;
                orderTemp.BorrowOrderSystem = item.BorrowOrderSystem;
                orderTemp.CollateralFlag = item.CollateralFlag;
                orderTemp.Comments = String.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment;
                orderTemp.ContraEntity = String.IsNullOrWhiteSpace(item.ContraEntityId) ? "" : item.ContraEntityId;
                orderTemp.DateTimeId = item.DateTimeId;
                orderTemp.DividendRate = Convert.ToDouble(item.DividendRate);
                orderTemp.EffectiveDate = DateTime.Today;
                orderTemp.EntityId = item.EntityId.ToString();
                orderTemp.IncomeTracked = item.IncomeTracked;
                orderTemp.IssueId = item.IssueId;
                orderTemp.Mark = Convert.ToDouble(item.Mark);
                orderTemp.MarkParameterId = String.IsNullOrWhiteSpace(item.MarkParameterId) ? "" : item.MarkParameterId;
                orderTemp.MaxPrice = Convert.ToDouble(item.MaxPrice);
                orderTemp.MinPartialQuantity = item.MinQuantity;
                orderTemp.MinRebateRateId = String.IsNullOrWhiteSpace(item.MinRebateRateId) ? "" : item.MinRebateRateId;
                
                orderTemp.MinRebateRateId = orderTemp.MinRebateRate < 0 ? "N" : "S";

                orderTemp.MinRebateRate = Convert.ToDouble(item.MinRebateRate);
                orderTemp.ProfitId = String.IsNullOrWhiteSpace(item.ProfitId) ? "" : item.ProfitId;
                orderTemp.Quantity = item.Quantity;
                orderTemp.SLBorrowOrder = item.SLBorrowOrder;
                orderTemp.SubmissionType = String.IsNullOrWhiteSpace(item.SubmissionType) ? "" : item.SubmissionType;
                orderTemp.TimeOut = String.IsNullOrWhiteSpace(item.TimeOut) ? "" : item.TimeOut;
                orderTemp.DateTimeId = item.DateTimeId;

                DataAutoAction.UpdateBorrowOrder(orderTemp);
            }
            catch (Exception e)
            {
                DataError.LogError("", "UpdateAutoBorrow", e.Message);
            }

            return Json(item);
        }

        public ActionResult UpdateAutoBorrowHelper([DataSourceRequest] DataSourceRequest request, BorrowOrderSuggestionModel item)
        {
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult SaveLcorTrades(IEnumerable<TradeChangeModel> list)
        {
            if (list == null) return Json(list.Where(x => x.SubmissionType != StatusDetail.Approved));

            var tradeChangeModels = list as TradeChangeModel[] ?? list.ToArray();
            
            foreach (var item in tradeChangeModels.Where(item => item.Enabled))
            {
                try
                {
                    var order = new SL_BorrowOrder
                    {
                        EffectiveDate = DateTime.Today,
                        AddToLoanetIndicator = DataSystemValues.LoadSystemValue("AutoBorrowAddToLoanet", "Y"),
                        BatchCode = DataSystemValues.LoadSystemValue("AutoBorrowBatchCode", ""),
                        BorrowOrderRequest = "",
                        BorrowOrderResponse = "",
                        BorrowOrderSystem = 0,
                        BorrowOrderStatus = StatusMain.Pending,
                        CollateralFlag = SL_CollateralFlag.C,
                        Comments = "",
                        ContraEntity = String.IsNullOrWhiteSpace(item.ContraEntityId) ? "" : item.ContraEntityId,
                        DividendRate = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowDivRate", "100")),
                        EntityId = item.EntityId,
                        IncomeTracked =
                            Convert.ToBoolean(DataSystemValues.LoadSystemValue("AutoBorrowIncomeTracked", "False")),
                        IssueId = item.IssueId,
                        Mark = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowMark", "1.02")),
                        MarkParameterId = DataSystemValues.LoadSystemValue("AutoBorrowMarkId", "%"),
                        MaxPrice = Convert.ToDouble(item.Price),
                        Quantity = item.Quantity
                    };

                    order.MinPartialQuantity = bool.Parse(DataSystemValues.LoadSystemValue("AutoBorrowMatchMin", "false")) ? order.Quantity : Convert.ToDecimal(DataSystemValues.LoadSystemValue("AutoBorrowMinPartialQuantity", "100")); 
                            
                    order.MinRebateRate = Convert.ToDouble(item.RebateRate);
                    if (!String.IsNullOrWhiteSpace(item.ContraEntityId))
                    {

                        try
                        {
                            DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);

                            order.MinRebateRateId = item.RebateRate >= 0 ? DataSystemValues.LoadSystemValue("AutoBorrowRoutingTableRateId", "") : "N";
                        }
                        catch
                        {
                            order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                        }
                    }
                    else
                    {
                        order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                    }

                    order.ProfitId = DataSystemValues.LoadSystemValue("AutoBorrowProfitId", "");
                    order.SubmissionType = "";
                    order.TimeOut = DataSystemValues.LoadSystemValue("AutoBorrowTimeOut", "10");
                        
                    DataAutoAction.AddBorrowOrder(order);

                    item.SubmissionType = StatusDetail.Approved;
                }
                catch (Exception e)
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                }
            }

            return Json(tradeChangeModels.Where(x => x.SubmissionType != StatusDetail.Approved));
        }

        public JsonResult HelperSave(IEnumerable<BorrowOrderSuggestionModel> list)
        {
            if (list != null)
            {
                foreach (BorrowOrderSuggestionModel item in list)
                {
                    if (item.Enabled)
                    {
                        try
                        {
                            SL_BorrowOrder order = new SL_BorrowOrder();

                            order.EffectiveDate = DateTime.Today;
                            order.AddToLoanetIndicator = DataSystemValues.LoadSystemValue("AutoBorrowAddToLoanet", "Y");
                            order.BatchCode = DataSystemValues.LoadSystemValue("AutoBorrowBatchCode", "");
                            order.BorrowOrderRequest = "";
                            order.BorrowOrderResponse = "";
                            order.BorrowOrderSystem = 0;
                            order.BorrowOrderStatus = StatusMain.Pending;
                            order.CollateralFlag = SL_CollateralFlag.C;
                            order.Comments = "";
                            order.ContraEntity = String.IsNullOrWhiteSpace(item.ContraEntityId) ? "" : item.ContraEntityId;
                            order.DividendRate = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowDivRate", "100"));
                            order.EntityId = item.EntityId;
                            order.IncomeTracked = Convert.ToBoolean(DataSystemValues.LoadSystemValue("AutoBorrowIncomeTracked", "False"));
                            order.IssueId = item.IssueId;
                            order.Mark = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowMark", "1.02"));
                            order.MarkParameterId = DataSystemValues.LoadSystemValue("AutoBorrowMarkId", "%");
                            order.MaxPrice = Convert.ToDouble(item.MaxPrice);
                            order.Quantity = item.Quantity;

                            if (bool.Parse(DataSystemValues.LoadSystemValue("AutoBorrowMatchMin", "false")))
                            {
                                order.MinPartialQuantity = order.Quantity;
                            }
                            else
                            {
                                order.MinPartialQuantity = Convert.ToDecimal(DataSystemValues.LoadSystemValue("AutoBorrowMinPartialQuantity", "100"));
                            }

                            order.MinRebateRate = Convert.ToDouble(item.RebateRate);

                            if (!String.IsNullOrWhiteSpace(item.ContraEntityId))
                            {

                                try
                                {
                                    DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);

                                    if (item.RebateRate >= 0)
                                    {
                                        order.MinRebateRateId = "S";
                                    }
                                    else
                                    {
                                        order.MinRebateRateId = "N";
                                    }
                                }
                                catch
                                {
                                    if (item.RebateRate >= 0)
                                    {
                                        order.MinRebateRateId = DataSystemValues.LoadSystemValue("AutoBorrowRoutingTableRateId", "");
                                    }
                                    else
                                    {
                                        order.MinRebateRateId = "N";
                                    }
                                }
                            }

                            order.ProfitId = DataSystemValues.LoadSystemValue("AutoBorrowProfitId", "");
                            order.SubmissionType = "";
                            order.TimeOut = DataSystemValues.LoadSystemValue("AutoBorrowTimeOut", "10");

                            DataAutoAction.AddBorrowOrder(order);

                            item.SubmissionType = StatusDetail.Approved;
                        }
                        catch (Exception e)
                        {
                            item.MemoInfo = e.Message;
                            item.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
            }

            return Json(list.Where(x => x.SubmissionType != StatusDetail.Approved));
        }

        public JsonResult Send(IEnumerable<SL_BorrowOrderProjection> list)
        {
            if (list != null)
            {
                foreach (SL_BorrowOrderProjection item in list)
                {
                    if (item.BorrowStatus == StatusMain.Pending)
                    {
                        try
                        {
                            SL_AutoBorrowOrder order = DataAutoBorrow.LoadBorrowOrder(item.SLBorrowOrder);

                            DataTransformation.GenerateBorrowOrder( order );
                                                       
                            order.BorrowOrderStatus = StatusMain.Ready;

                            DataAutoBorrow.UpdateBorrowOrder( order );
                        }
                        catch (Exception e)
                        {
                            DataError.LogError("", "Send", e.Message);
                        }
                    }
                }
            }

            return Json(list);
        }

        public JsonResult Cancel(IEnumerable<SL_BorrowOrderProjection> list)
        {
            if (list != null)
            {
                foreach (SL_BorrowOrderProjection item in list)
                {
                    if (item.BorrowStatus == StatusMain.Pending)
                    {
                        try
                        {
                            SL_BorrowOrder order = DataAutoAction.LoadBorrowOrder(item.SLBorrowOrder);

                            order.BorrowOrderStatus = StatusMain.Cancelled;

                            DataAutoAction.UpdateBorrowOrder(order);
                        }
                        catch (Exception e)
                        {
                            DataError.LogError("", "Cancel", e.Message);
                        }
                    }
                }
            }

            return Json(list);
        }

        public JsonResult Mirror(IEnumerable<SL_BorrowOrderProjection> list)
        {
            if (list != null)
            {
                foreach (SL_BorrowOrderProjection item in list)
                {
                    try
                    {
                        SL_BorrowOrder orderTemp = new SL_BorrowOrder();

                        orderTemp.AddToLoanetIndicator = String.IsNullOrWhiteSpace(item.AddToLoanetId) ? "" : item.AddToLoanetId;
                        orderTemp.BatchCode = String.IsNullOrWhiteSpace(item.BatchCode) ? "" : item.BatchCode;
                        orderTemp.BorrowOrderRequest = "";
                        orderTemp.BorrowOrderResponse = "";
                        orderTemp.BorrowOrderStatus = StatusMain.Pending;
                        orderTemp.BorrowOrderSystem = item.BorrowOrderSystem;
                        orderTemp.CollateralFlag = item.CollateralFlag;
                        orderTemp.Comments = String.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment;
                        orderTemp.ContraEntity = "";
                        orderTemp.DateTimeId = item.DateTimeId;
                        orderTemp.DividendRate = Convert.ToDouble(item.DividendRate);
                        orderTemp.EffectiveDate = DateTime.Today;
                        orderTemp.EntityId = item.EntityId.ToString();
                        orderTemp.IncomeTracked = item.IncomeTracked;
                        orderTemp.IssueId = item.IssueId;
                        orderTemp.Mark = Convert.ToDouble(item.Mark);
                        orderTemp.MarkParameterId = String.IsNullOrWhiteSpace(item.MarkParameterId) ? "" : item.MarkParameterId;
                        orderTemp.MaxPrice = Convert.ToDouble(item.MaxPrice);
                        orderTemp.MinPartialQuantity = item.MinQuantity;
                        orderTemp.MinRebateRate = Convert.ToDouble(item.MinRebateRate);

                        if (orderTemp.MinRebateRate < 0)
                        {
                            orderTemp.MinRebateRateId = "N";
                        }
                        else
                        {
                            orderTemp.MinRebateRateId = "S";
                        }

                        orderTemp.ProfitId = String.IsNullOrWhiteSpace(item.ProfitId) ? "" : item.ProfitId;
                        orderTemp.Quantity = item.Quantity;
                        orderTemp.SubmissionType = String.IsNullOrWhiteSpace(item.SubmissionType) ? "" : item.SubmissionType;
                        orderTemp.TimeOut = String.IsNullOrWhiteSpace(item.TimeOut) ? "" : item.TimeOut;

                        DataAutoAction.AddBorrowOrder(orderTemp);
                    }
                    catch (Exception e)
                    {
                        DataError.LogError("", "Mirror", e.Message);
                    }
                }
            }

            return Json(list);
        }

        public JsonResult ProcessLcorOptions(IEnumerable<SL_BorrowOrderProjection> list,
                                string contraEntityId,
                                decimal? rebateRate,
                                string rebateRateId,      
                                decimal? mark,
                                string profitId,
                                string batchCode,
                                string timeOut)
        {
            if (list != null)
            {
                foreach (SL_BorrowOrderProjection item in list)
                {
                    if (item.BorrowStatus == StatusMain.Pending)
                    {
                        try
                        {
                            SL_BorrowOrder orderTemp = DataAutoAction.LoadBorrowOrder(item.SLBorrowOrder);

                            orderTemp.ContraEntity = String.IsNullOrWhiteSpace(contraEntityId) ? orderTemp.ContraEntity : contraEntityId;
                            orderTemp.ProfitId = String.IsNullOrWhiteSpace(profitId) ? orderTemp.ProfitId : profitId;
                            orderTemp.BatchCode = String.IsNullOrWhiteSpace(batchCode) ? orderTemp.BatchCode : batchCode;
                            orderTemp.MinRebateRate = (rebateRate == null) ? orderTemp.MinRebateRate : Convert.ToDouble(rebateRate);
                            orderTemp.MinRebateRateId = (rebateRate == null) ? orderTemp.MinRebateRateId : rebateRateId;
                            orderTemp.Mark = (mark == null) ? orderTemp.Mark : Convert.ToDouble(mark);
                            orderTemp.TimeOut = String.IsNullOrWhiteSpace(timeOut) ? orderTemp.TimeOut : timeOut;

                            DataAutoAction.UpdateBorrowOrder(orderTemp);
                        }
                        catch (Exception e)
                        {
                            DataError.LogError("", "ProcessLcorOptions", e.Message);
                        }
                    }
                }
            }

            return Json(list);
        }


        public JsonResult ProcessHelperLcorOptions(IEnumerable<BorrowOrderSuggestionModel> list,
                                string contraEntityId,
                                decimal? rebateRate,
                                decimal? mark)
        {
            if (list != null)
            {
                foreach (BorrowOrderSuggestionModel item in list)
                {
                    if (item.Enabled)
                    {
                        try
                        {                          
                            item.ContraEntityId = String.IsNullOrWhiteSpace(contraEntityId) ? "" : contraEntityId;
                            item.RebateRate = (rebateRate == null) ? item.RebateRate : Convert.ToDecimal(rebateRate);
                            item.Mark = (mark == null) ? item.Mark : Convert.ToDecimal(mark);

                            item.SubmissionType = StatusDetail.Pending;
                        }
                        catch (Exception e)
                        {
                            item.MemoInfo = e.Message;
                            item.SubmissionType = StatusDetail.Rejected;
                            DataError.LogError("", "ProcessLcorOptions", e.Message);
                        }
                    }
                }
            }

            return Json(list);
        }

        public JsonResult ProcessHelperSelectAll(IEnumerable<BorrowOrderSuggestionModel> list)
        {
            foreach(BorrowOrderSuggestionModel item in list)
            {
                item.Enabled = true;
            }

            return Json(list);
        }

        public JsonResult ProcessHelperSaveBorrowOrder(IEnumerable<BorrowOrderSuggestionModel> list)
        {
            string defaultAddToLoanetIndicator = DataSystemValues.LoadSystemValue("AutoBorrowAddToLoanet", "Y");
            string defaultBatchCode = DataSystemValues.LoadSystemValue("AutoBorrowBatchCode", "");
            double defaultDividendRate = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowDivRate", "100"));
            bool defaultIncomeTracked = Convert.ToBoolean(DataSystemValues.LoadSystemValue("AutoBorrowIncomeTracked", "False"));
            string defaultMarkId = DataSystemValues.LoadSystemValue("AutoBorrowMarkId", "%");
            bool matchQuantity = bool.Parse(DataSystemValues.LoadSystemValue("AutoBorrowMatchMin", "false"));
            string defaultProfitId = DataSystemValues.LoadSystemValue("AutoBorrowProfitId", "");
            string defaultTimeOut = DataSystemValues.LoadSystemValue("AutoBorrowTimeOut", "10");
            decimal defaultMinPartialQuantity = Convert.ToDecimal(DataSystemValues.LoadSystemValue("AutoBorrowMinPartialQuantity", "100"));
            string defaultMinRebateTableId =  DataSystemValues.LoadSystemValue("AutoBorrowRoutingTableRateId", "");

            foreach (BorrowOrderSuggestionModel item in list)
            {
                if (item.Enabled)
                {
                    try
                    {
                        SL_BorrowOrder order = new SL_BorrowOrder();

                        order.EffectiveDate = DateTime.Today;
                        order.AddToLoanetIndicator = defaultAddToLoanetIndicator;
                        order.BatchCode = defaultBatchCode;
                        order.BorrowOrderRequest = "";
                        order.BorrowOrderResponse = "";
                        order.BorrowOrderSystem = 0;
                        order.BorrowOrderStatus = StatusMain.Pending;
                        order.CollateralFlag = SL_CollateralFlag.C;
                        order.Comments = "";
                        order.ContraEntity = String.IsNullOrWhiteSpace(item.ContraEntityId) ? "" : item.ContraEntityId;
                        order.DividendRate = defaultDividendRate;
                        order.EntityId = item.EntityId;
                        order.IncomeTracked = defaultIncomeTracked;
                        order.IssueId = item.IssueId;
                        order.Mark = Convert.ToDouble(item.Mark);
                        order.MarkParameterId = defaultMarkId;
                        order.MaxPrice = Convert.ToDouble(item.MaxPrice); //Convert.ToDouble(BondFire.Calculators.TradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(DataIssue.LoadIssuePrice(item.IssueId).CurrentCashPrice), null));
                        order.Quantity = item.Quantity;
                        
                        if (matchQuantity)
                        {
                            order.MinPartialQuantity = order.Quantity;
                        }
                        else
                        {
                            order.MinPartialQuantity = defaultMinPartialQuantity;
                        }


                        if (!String.IsNullOrWhiteSpace(item.ContraEntityId))
                        {

                            try
                            {
                                DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);

                                if (item.RebateRate >= 0)
                                {
                                    order.MinRebateRateId = defaultMinRebateTableId;
                                }
                                else
                                {
                                    order.MinRebateRateId = "N";
                                }
                            }
                            catch
                            {
                                if (item.RebateRate >= 0)
                                {
                                    order.MinRebateRateId = "S";
                                }
                                else
                                {
                                    order.MinRebateRateId = "N";
                                }
                            }
                        }
                        else
                        {
                            if (item.RebateRate >= 0)
                            {
                                order.MinRebateRateId = "S";
                            }
                            else
                            {
                                order.MinRebateRateId = "N";
                            }
                        }

                        order.MinRebateRate = Convert.ToDouble(item.RebateRate);
                        order.ProfitId = defaultProfitId;
                        order.SubmissionType = "";
                        order.TimeOut = defaultTimeOut;

                        DataAutoAction.AddBorrowOrder(order);

                        item.SubmissionType = StatusDetail.Approved;
                    }
                    catch (Exception e)
                    {
                        item.MemoInfo = e.Message;
                        item.SubmissionType = StatusDetail.Rejected;
                    }
                }
            }

            return Json(list.Where(x => x.SubmissionType != StatusDetail.Approved));
        }

        public JsonResult LoadSingleBorrowOrder(SL_BoxCalculationExtendedProjection item, decimal modelId)
        {
            BorrowOrderSuggestionModel order = new BorrowOrderSuggestionModel();

            if (item.ExcessPositionSettled != 0)
            {                
                order.ContraEntityId = "";
                order.Enabled = true;
                order.EntityId = item.EntityId;
                order.IssueId = (int)item.IssueId;
                order.Mark = 1.02m;
                order.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice), null);
                order.ModelId = modelId;
                order.Quantity = Math.Abs(item.SuggestionBorrowSettled);
                order.RebateRate = 0;
                order.SecurityNumber = item.SecurityNumber;
                order.Ticker = item.Ticker;
                order.MemoInfo = "";
                order.SubmissionType = StatusDetail.Pending;

                order = BorrowOrderValidationService.SetDefaults(order);
            }

            return Json(order, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadBulkBorrowOrder(IEnumerable<SL_BoxCalculationExtendedProjection> items)
        {
            List<BorrowOrderSuggestionModel> list = new List<BorrowOrderSuggestionModel>();
            long modelCount = 0;

            if (items != null)
            {
                foreach (SL_BoxCalculationExtendedProjection item in items)
                {
                    if (item.ExcessPositionSettled != 0)
                    {
                        BorrowOrderSuggestionModel order = new BorrowOrderSuggestionModel();
                        order.ContraEntityId = "";
                        order.Enabled = true;
                        order.EntityId = item.EntityId;
                        order.IssueId = (int)item.IssueId;
                        order.Mark = 1.02m;
                        order.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice), null);
                        order.ModelId = modelCount;
                        order.Quantity = Math.Abs(item.SuggestionBorrowSettled);
                        order.RebateRate = 0;
                        order.SecurityNumber = item.SecurityNumber;
                        order.Ticker = item.Ticker;
                        order.MemoInfo = "";
                        order.SubmissionType = StatusDetail.Pending;

                        order = BorrowOrderValidationService.SetDefaults(order);

                        list.Add(order);
                        modelCount++;
                    }
                }
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadAutoBorrowBoxProjectionPartial(IEnumerable<BorrowOrderSuggestionModel> items)
        {
            List<BorrowOrderSuggestionModel> list = new List<BorrowOrderSuggestionModel>();

            bool useIssueMarkup = bool.Parse(DataSystemValues.LoadSystemValue("UseIssueMarginBoxMgmt", "false"));

            long modelCount = 0;

            if (items != null)
            {
                foreach (BorrowOrderSuggestionModel item in items)
                {
                    if (item.Quantity != 0)
                    {
                        item.ContraEntityId = "";
                        item.Enabled = true;
                        item.EntityId = item.EntityId;
                        item.IssueId = (int)item.IssueId;
                        item.Mark = 1.02m;
                        item.ModelId = modelCount;

                        if (!useIssueMarkup)
                        {
                            item.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, item.MaxPrice, null);
                        }

                        item.RebateRate = 0;
                        item.SecurityNumber = item.SecurityNumber;
                        item.Ticker = item.Ticker;
                        item.MemoInfo = "";
                        item.SubmissionType = StatusDetail.Pending;

                        BorrowOrderSuggestionModel tempItem =  BorrowOrderValidationService.SetDefaults(item);
                        list.Add(tempItem);
                        modelCount++;
                    }
                }
            }

            if (list.Count == 0)
            {
                return PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml");
            }
            
            return PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowHelper.cshtml", list);
        }

        public PartialViewResult LoadAutoLoanApprovedContraEntityProjectionPartial(string entityId, string contraEntity)
        {
            List<AutoBorrowOrderSuggestionModel> list = new List<AutoBorrowOrderSuggestionModel>();
            List<SL_AutoLoanOrder> autoLoanOrderList = new List<SL_AutoLoanOrder>();

            bool useIssueMarkup = bool.Parse(DataSystemValues.LoadSystemValue("UseIssueMarginBoxMgmt", "false"));

            long modelCount = 0;

            var items = DataAutoLoan.LoadAutoLoanByEntityAndContraEntity(DateTime.Today, entityId, contraEntity);

            if (items != null)
            {
                foreach (SL_AutoLoanOrderProjection item in items.Where(x => !x.IsExportedAutoBorrow))
                {
                    if (item.ApprovedAmount > 0 && item.LoanStatusFlag == SL_AutoLoanOrderStatusFlag.Approved)
                    {
                        SL_AutoLoanOrder _tempOrder = new SL_AutoLoanOrder();

                        try
                        {
                            _tempOrder = DataAutoLoan.LoadAutoLoanByPk(item.SLAutoLoanOrder);
                            _tempOrder.IsExportedAutoBorrow = true;
                            autoLoanOrderList.Add(_tempOrder);
                        }
                        catch
                        {

                        }

                        AutoBorrowOrderSuggestionModel order = new AutoBorrowOrderSuggestionModel();
                        order.ContraEntityId = "";
                        order.Enabled = true;
                        order.ListName = DateTime.Now.ToString("HHmmss") + "-" + SessionService.SecurityContext.UserName;
                        order.EntityId = item.EntityId;
                        order.IssueId = (int)item.IssueId;
                        order.Mark = 1.02m;
                        order.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice), null);
                        order.ModelId = modelCount;
                        order.Quantity = Math.Abs((decimal)item.ApprovedQuantity);
                        order.RebateRate = 0.0;
                        order.SecurityNumber = item.SecurityNumber;
                        order.Ticker = item.Ticker;
                        order.MemoInfo = item.SLAutoLoanOrder.ToString();
                        order.SubmissionType = StatusDetail.Pending;

                        order = AutoBorrowOrderValidationService.SetDefaults(order);

                        list.Add(order);
                        modelCount++;
                    }
                }

                try
                {
                    DataAutoLoan.UpdateLoanOrderRange(autoLoanOrderList, SessionService.SecurityContext);
                }
                catch
                {

                }
            }

            return list.Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowHelper.cshtml", list);
        }


        public PartialViewResult LoadAutoLoanApprovedProjectionPartial(IEnumerable<SL_AutoLoanOrderProjection> items)
        {
            List<AutoBorrowOrderSuggestionModel> list = new List<AutoBorrowOrderSuggestionModel>();
            List<SL_AutoLoanOrder> autoLoanOrderList = new List<SL_AutoLoanOrder>();

            bool useIssueMarkup = bool.Parse(DataSystemValues.LoadSystemValue("UseIssueMarginBoxMgmt", "false"));

            long modelCount = 0;

            if (items != null)
            {
                foreach (SL_AutoLoanOrderProjection item in items.Where(x => !x.IsExportedAutoBorrow))
                {
                    if (item.ApprovedAmount >  0 && item.LoanStatusFlag == SL_AutoLoanOrderStatusFlag.Approved)
                    {
                        SL_AutoLoanOrder _tempOrder = new SL_AutoLoanOrder();

                        try
                        {
                            _tempOrder = DataAutoLoan.LoadAutoLoanByPk(item.SLAutoLoanOrder);
                            _tempOrder.IsExportedAutoBorrow = true;
                            autoLoanOrderList.Add(_tempOrder);
                        }
                        catch
                        {

                        }

                        AutoBorrowOrderSuggestionModel order = new AutoBorrowOrderSuggestionModel();
                        order.ContraEntityId = "";
                        order.Enabled = true;
                        order.ListName = DateTime.Now.ToString("HHmmss") + "-" + SessionService.SecurityContext.UserName;
                        order.EntityId = item.EntityId;
                        order.IssueId = (int)item.IssueId;
                        order.Mark = 1.02m;
                        order.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice), null);
                        order.ModelId = modelCount;
                        order.Quantity = Math.Abs((decimal)item.ApprovedQuantity);
                        order.RebateRate = 0.0;
                        order.SecurityNumber = item.SecurityNumber;
                        order.Ticker = item.Ticker;
                        order.MemoInfo = item.SLAutoLoanOrder.ToString();                        
                        order.SubmissionType = StatusDetail.Pending;

                        order = AutoBorrowOrderValidationService.SetDefaults(order);

                        list.Add(order);
                        modelCount++;
                    }
                }
                
                try
                {
                    DataAutoLoan.UpdateLoanOrderRange(autoLoanOrderList, SessionService.SecurityContext);
                }
                catch
                {

                }
            }

            return list.Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowHelper.cshtml", list);
        }


        public PartialViewResult LoadAutoBorrowContractProjectionPartial(IEnumerable<SL_ContractExtendedProjection> items)
        {
            List<BorrowOrderSuggestionModel> list = new List<BorrowOrderSuggestionModel>();
            long modelCount = 0;

            if (items != null)
            {
                foreach (SL_ContractExtendedProjection item in items)
                {
                    if (item.Quantity != 0)
                    {
                        BorrowOrderSuggestionModel order = new BorrowOrderSuggestionModel();
                        order.ContraEntityId = "";
                        order.Enabled = true;
                        order.EntityId = item.EntityId;
                        order.IssueId = (int)item.IssueId;
                        order.Mark = 1.02m;
                        order.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice), null);
                        order.ModelId = modelCount;
                        order.Quantity = Math.Abs(item.Quantity);
                        order.RebateRate = item.RebateRate;
                        order.SecurityNumber = item.SecurityNumber;
                        order.Ticker = item.Ticker;
                        order.MemoInfo = "";
                        order.SubmissionType = StatusDetail.Pending;

                        order = BorrowOrderValidationService.SetDefaults(order);

                        list.Add(order);
                        modelCount++;
                    }
                }
            }


            if (list.Count == 0)
            {
                return PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml");
            }

            return PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowHelper.cshtml", list);
        }

        public PartialViewResult LoadAutoBorrowRecallProjectionPartial(IEnumerable<SL_RecallExtendedProjection> items)
        {
            var list = new List<BorrowOrderSuggestionModel>();
            long modelCount = 0;

            if (items == null)
                return list.Count == 0
                    ? PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml")
                    : PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowHelper.cshtml", list);
            foreach (SL_RecallExtendedProjection item in items)
            {
                if (item.QuantityRecalled != 0)
                {
                    if (item.IssueId != null)
                    {
                        var order = new BorrowOrderSuggestionModel
                        {
                            ContraEntityId = "",
                            Enabled = true,
                            EntityId = item.EntityId,
                            IssueId = (int) item.IssueId,
                            Mark = 1.02m,
                            MaxPrice = item.Price,
                            ModelId = modelCount,
                            Quantity = Math.Abs(item.QuantityRecalled),
                            RebateRate = item.RebateRate,
                            SecurityNumber = item.SecurityNumber,
                            Ticker = item.Ticker,
                            MemoInfo = "",
                            SubmissionType = StatusDetail.Pending
                        };

                        order = BorrowOrderValidationService.SetDefaults(order);

                        list.Add(order);
                    }
                    modelCount++;
                }
            }

            return list.Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowHelper.cshtml", list);
        }

        public PartialViewResult LoadAutoBorrowDefaultOptionsPartial()
        {
            return PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowDefaultOptions.cshtml");
        }

        public PartialViewResult LoadAutoBorrowsHsitorySummaryPartial(DateTime effectiveDate, string entityId)
        {
            var model = DataAutoBorrow.LoadBorrowOrderHistory(effectiveDate, entityId);

            return PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowApprovalHistory.cshtml", model);
        }

        public JsonResult ProcessDefaultsUpdateBorrowOrder(
            string batchCode,
            decimal divRate,
            bool incomeTracked,
            bool matchMin,
            string profitId,
            decimal mark,
            string markId,           
            decimal minQuantity,            
            string timeOut,
            string addToLoanet,
            string routeTableRateId)
        {
            var success = true;

            try
            {                
                DataSystemValues.UpdateValueByName("AutoBorrowBatchCode", batchCode);

                DataSystemValues.UpdateValueByName("AutoBorrowDivRate", divRate.ToString());

                DataSystemValues.UpdateValueByName("AutoBorrowIncomeTracked", incomeTracked.ToString());

                DataSystemValues.UpdateValueByName("AutoBorrowProfitId", profitId);

                DataSystemValues.UpdateValueByName("AutoBorrowMark", mark.ToString());

                DataSystemValues.UpdateValueByName("AutoBorrowMarkId", markId);

                DataSystemValues.UpdateValueByName("AutoBorrowMinPartialQuantity", minQuantity.ToString());

                DataSystemValues.UpdateValueByName("AutoBorrowTimeOut", timeOut);

                DataSystemValues.UpdateValueByName("AutoBorrowAddToLoanet", addToLoanet);

                DataSystemValues.UpdateValueByName("AutoBorrowMatchMin", matchMin.ToString());

                DataSystemValues.UpdateValueByName("AutoBorrowRoutingTableRateId", routeTableRateId.ToString());
            }
            catch
            {
                success = false;
            }

            return Json(success);
        }
    }
}
