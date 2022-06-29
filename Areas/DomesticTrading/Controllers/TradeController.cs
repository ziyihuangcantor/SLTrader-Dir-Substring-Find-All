using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;

using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using BondFire.Core.Dates;
using BondFire.FileImportProcesses.Utility;
using BFLogic.ParseLogic;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using SLTrader.Enums;
using SLTrader.Tools;
using SLTrader.Custom;
using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Tools.Helpers;
using SLTrader.Rules;
using SLTrader.Exceptions;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class TradeController : BaseController
    {
        public static HttpPostedFileBase TradeFile;

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ExceedCreditLimitList(IEnumerable<TradeChangeModel> items)
        {
            bool success = false;

            if (items != null)
            {
                if (items.Select(x => x.EntityId).Distinct().Count() == 1)
                {
                    if (items.Select(x => new { x.ContraEntityId, x.CurrencyCode}).Distinct().Count() == 1)
                    {
                        string entityId = items.Select(x => x.EntityId).Distinct().First();
                        string contraEntityId = items.Select(x => x.ContraEntityId).Distinct().First();
                        Currency currency = items.Select(x => x.CurrencyCode).Distinct().First();

                        decimal amount = items.Select(x => x.Amount).Sum();

                        success = DataContraEntity.ExceededCreditLimit(entityId, contraEntityId, amount, currency);
                    }
                }
            }

            return Extended.JsonMax(success, JsonRequestBehavior.AllowGet);
        }




       public JsonResult ExceedPrepayCreditLimitTest(SingleTradeModel item)
        {
            PrepayCreditLimitModel model = new PrepayCreditLimitModel()
            {
                Message = "",
                RestrictTrade = false
            };

            var prepayTrade1 = false;
            var creditTrade1 = false;
            var prepayTrade2 = false;
            var creditTrade2 = false;


            string message = "";

            if (!string.IsNullOrWhiteSpace(item.Trade1.ContraEntityId))
            {
                if (item.Trade1.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE)
                {
                    prepayTrade1 = DataContraEntity.ExceededPrepayLimit(item.Trade1.EntityId, item.Trade1.ContraEntityId, item.Trade1.Amount, (Currency)item.Trade1.CurrencyCode, (DateTime)item.Trade1.CashSettleDate);
                }

                creditTrade1 = DataContraEntity.ExceededCreditLimit(item.Trade1.EntityId, item.Trade1.ContraEntityId, item.Trade1.Amount, (Currency)item.Trade1.CurrencyCode);
            }

            if (!string.IsNullOrWhiteSpace(item.Trade2.ContraEntityId) && item.IsMirror)
            {
                if (item.Trade1.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE)
                {

                    prepayTrade2 = DataContraEntity.ExceededPrepayLimit(item.Trade1.EntityId, item.Trade2.ContraEntityId, item.Trade2.Amount, (Currency)item.Trade2.CurrencyCode, (DateTime)item.Trade2.CashSettleDate);
                }

                creditTrade2 = DataContraEntity.ExceededCreditLimit(item.Trade2.EntityId, item.Trade2.ContraEntityId, item.Trade2.Amount, (Currency)item.Trade2.CurrencyCode);
            }

            if (prepayTrade1)
            {
                model.Message += "Prepay limit exceeded (Trade 1)\r\n";                
            }

            if (creditTrade1)
            {
                model.Message += "Credit limit exceeded (Trade 1)\r\n";
                model.RestrictTrade = true;
            }

            if (prepayTrade2)
            {
                model.Message += "Prepay limit exceeded (Tradde 2)\r\n";
            }

            if (creditTrade2)
            {
                model.Message += "Credit limit exceeded (Trade 2)\r\n";
                model.RestrictTrade = true;
            }


            return Extended.JsonMax(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ExceedPrepayLimitList(IEnumerable<TradeChangeModel> items)
        {
            bool success = false;

            if (items != null)
            {
                if (items.Select(x => x.EntityId).Distinct().Count() == 1)
                {
                    if ((items.Select(x => x.ExecutingType).Distinct().Count() == 1) && (items.Select(x => x.ExecutingType).Distinct().First() == SL_ExecutionSystemType.GLOBALONE))
                    {
                        string entityId = items.Select(x => x.EntityId).Distinct().First();
                        string contraEntityId = items.Select(x => x.ContraEntityId).Distinct().First();
                        Currency currency = items.Select(x => x.CurrencyCode).Distinct().First();
                        decimal amount = items.Select(x => x.Amount).Sum();
                        DateTime cashSettleDate = items.Select(x => x.CashSettleDate).First();
                        
                        success = DataContraEntity.ExceededPrepayLimit(entityId, contraEntityId, amount, currency, cashSettleDate);
                    }
                }
            }

            return Extended.JsonMax(success, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ExceedCreditLimit(SingleTradeModel item)
        {
            bool success = false;

            if (!string.IsNullOrWhiteSpace(item.Trade1.ContraEntityId))
            {
                success = DataContraEntity.ExceededCreditLimit(item.Trade1.EntityId, item.Trade1.ContraEntityId, item.Trade1.Amount, (Currency)item.Trade1.CurrencyCode);
            }

            if (!string.IsNullOrWhiteSpace(item.Trade2.ContraEntityId) && item.IsMirror)
            {
                success = DataContraEntity.ExceededCreditLimit(item.Trade2.EntityId, item.Trade2.ContraEntityId, item.Trade2.Amount, (Currency)item.Trade2.CurrencyCode);
            }

            return Extended.JsonMax(success, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ExceedPrepayLimit(SingleTradeModel item)
        {
            bool success = false;

            if (!string.IsNullOrWhiteSpace(item.Trade1.ContraEntityId) && item.Trade1.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE)
            {
                success = DataContraEntity.ExceededPrepayLimit(item.Trade1.EntityId, item.Trade1.ContraEntityId, item.Trade1.Amount, (Currency)item.Trade1.CurrencyCode, (DateTime)item.Trade1.CashSettleDate);
            }

            if (!string.IsNullOrWhiteSpace(item.Trade2.ContraEntityId) && item.IsMirror && item.Trade2.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE)
            {
                success = DataContraEntity.ExceededPrepayLimit(item.Trade2.EntityId, item.Trade2.ContraEntityId, item.Trade2.Amount, (Currency)item.Trade2.CurrencyCode,(DateTime)item.Trade1.CashSettleDate);
            }

            return Extended.JsonMax(success, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SendContract(IEnumerable<SL_TradeExtendedProjection> items)
        {
            var slTradeExtendedProjections = items as SL_TradeExtendedProjection[] ?? items.ToArray();
            
            if (!slTradeExtendedProjections.Any()) return Json(items);

            foreach (var trade in slTradeExtendedProjections.ToList())
            {
                try
                {
                    if ((trade.TradeStatus == StatusDetail.Approved) ||
                        (trade.TradeStatus == StatusDetail.Transmitted)) continue;

                    var status = DataExternalOperations.AddTradeToContract(trade, false);

                    trade.TradeStatus = status ? StatusDetail.Approved : StatusDetail.Rejected;
                }
                catch(Exception e)
                {
                    trade.TradeStatus = StatusDetail.Rejected;        
                    return ThrowJsonError(e);
                }
            }

            return Json(items);
        }

        public ActionResult SendBreaKOutContract(IEnumerable<SL_TradeBreakOutExtendedProjection> items)
        {
            var slTradeExtendedProjections = items as SL_TradeBreakOutExtendedProjection[] ?? items.ToArray();

            if (!slTradeExtendedProjections.Any()) return Json(items);

            foreach (var trade in slTradeExtendedProjections.ToList())
            {
                try
                {
                    if ((trade.TradeStatus == StatusDetail.Approved) ||
                        (trade.TradeStatus == StatusDetail.Transmitted)) continue;

                    var status = DataExternalOperations.AddTradeBreakOutToContract(trade, false);

                    trade.TradeStatus = status ? StatusDetail.Approved : StatusDetail.Rejected;
                }
                catch (Exception e)
                {
                    trade.TradeStatus = StatusDetail.Rejected;
                    return ThrowJsonError(e);
                }
            }

            return Json(items);
        }

        public ActionResult CancelTrade(IEnumerable<SL_TradeExtendedProjection> items)
        {
            foreach (SL_TradeExtendedProjection trade in items.ToList())
            {
                try
                {
                    if ((trade.TradeStatus != StatusDetail.Approved) && (trade.TradeStatus != StatusDetail.Transmitted))
                    {
                        bool status = DataTrades.DeleteTradeExtended(trade);

                        if (status)
                        {
                            trade.TradeStatus = StatusDetail.Cancelled;
                        }
                    }
                }
                catch
                {
                    trade.TradeStatus = StatusDetail.Rejected;
                }
            }

            return Json(items);
        }

        public ActionResult CancelTradeBreakOut(IEnumerable<SL_TradeBreakOutExtendedProjection> items)
        {
            foreach (SL_TradeBreakOutExtendedProjection trade in items.ToList())
            {
                try
                {
                    if ((trade.TradeStatus != StatusDetail.Approved) && (trade.TradeStatus != StatusDetail.Transmitted))
                    {
                        bool status = DataTrades.DeleteTradeExtendedBreakOut(trade);

                        if (status)
                        {
                            trade.TradeStatus = StatusDetail.Cancelled;
                        }
                    }
                }
                catch
                {
                    trade.TradeStatus = StatusDetail.Rejected;
                }
            }

            return Json(items);
        }

        public JsonResult TradeTypeGet()
        {
            return Extended.JsonMax(EnumExtensions.GetEnumSelectList<TradeType>().Where(x => x.Text.Contains("Stock")).ToList(), JsonRequestBehavior.AllowGet);
        }

        public string PCMatrixLookup(SL_TradeExtendedProjection item)
        {
            string pcMatrix = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
            
            return pcMatrix;
        }

        public string Trade1PCMatrixLookup(SingleTradeModel item)
        {
            string pcMatrix = PcMatrixService.SetDefaults(item.Trade1, DataPCMatrix.LoadPCMatrix(item.Trade1.EntityId));
           

            return pcMatrix;
        }

        public string Trade2PCMatrixLookup(SingleTradeModel item)
        {
            string pcMatrix = PcMatrixService.SetDefaults(item.Trade2, DataPCMatrix.LoadPCMatrix(item.Trade2.EntityId));
          

            return pcMatrix;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateTrade([DataSourceRequest] DataSourceRequest request, string entityId, SL_TradeExtendedProjection item)
        {
            item.EntityId = entityId;

            Issue issue = null;
            SL_ContraEntity contraEntity = null;

            if (!item.SecurityNumber.Equals(""))
            {
                issue = DataIssue.LoadIssue(item.SecurityNumber);
            }

            if (!String.IsNullOrWhiteSpace(item.ContraEntityId))
            {
                contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);
            }

            if (string.IsNullOrWhiteSpace(item.ProfitId))
            {
                item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
            }

            item = SLTradeCalculator.GenerateDefaults(item, issue, contraEntity, true,  bool.Parse(DataSystemValues.LoadSystemValue("Round" + item.ExecutingSystem, true.ToString())));
            item.TradeStatus = StatusDetail.Pending;
            
            try
            {
                DataTrades.AddTrade(item);              
            }
            catch (Exception e)
            {
                item.Comment = e.Message;
            }

            var items = new List<SL_TradeExtendedProjection> {item};

            return Json(items.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateTradeBreakOut([DataSourceRequest] DataSourceRequest request, string entityId, SL_TradeBreakOutExtendedProjection item)
        {
            item.EntityId = entityId;

            Issue issue = null;
            SL_ContraEntity contraEntity = null;

            if (!item.SecurityNumber.Equals(""))
            {
                issue = DataIssue.LoadIssue(item.SecurityNumber);
            }

            if (!String.IsNullOrWhiteSpace(item.ContraEntityId))
            {
                contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);
            }

            if (string.IsNullOrWhiteSpace(item.ProfitId))
            {
                item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
            }

            item = SLTradeCalculator.GenerateDefaults(item, issue, contraEntity, true,bool.Parse(DataSystemValues.LoadSystemValue("Round" + item.ExecutingSystem, true.ToString())));
            item.TradeStatus = StatusDetail.Pending;

            try
            {
                DataTrades.AddTrade(item);
            }
            catch (Exception e)
            {
                item.Comment = e.Message;
            }

            var items = new List<SL_TradeBreakOutExtendedProjection> { item };

            return Json(items.ToDataSourceResult(request, ModelState));
        }

        public ActionResult UpdateTrade([DataSourceRequest] DataSourceRequest request, SL_TradeExtendedProjection item)
        {
            try
            {
                if ((item.TradeStatus != StatusDetail.Approved) && (item.TradeStatus != StatusDetail.Transmitted))
                {
                    if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
                    {
                        if (item.IssueId != null)
                        {
                            var orginalPrice = Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice);
                            var amountPrice = Math.Ceiling(Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice) * Convert.ToDouble(item.Mark));


                            if (((item.Price / orginalPrice) <= 1.20) && ((item.Price / orginalPrice) >= 1))
                            {
                                item.Price = amountPrice;
                            }
                            else
                            {
                                item.Price = item.Price;
                            }
                        }
                    }

                    try
                    {
                        var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)item.EffectiveDate, item.EntityId, item.FeeType);

                        item.RebateRate = Convert.ToDouble(fundingRate.Fund + (decimal)item.FeeOffset);
                    }
                    catch
                    {
                        
                    }

                    if (!string.IsNullOrWhiteSpace(item.ContraEntityId))
                    {
                        var serverCopy = DataTrades.LoadTradeExtendedByPk(decimal.Parse(item.TradeNumber));

                        if (!(string.IsNullOrWhiteSpace(serverCopy.ContraEntityId) ? "" : serverCopy.ContraEntityId).Equals(item.ContraEntityId))
                        {
                            var contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);
                            var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)),contraEntity);
                        
                            item.Mark = (item.TradeType.Equals(TradeType.StockBorrow))? (double.Parse(contraEntity.BorrowColl) / 100f) :(double.Parse(contraEntity.LoanColl) / 100f);
                            item.Price = Convert.ToDouble(price);
                            item.Amount = Convert.ToDecimal(item.Price) * item.Quantity;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
                    }
                                      
                    item.Amount = SLTradeCalculator.CalculateMoney(
                        Convert.ToDouble(item.Price),
                        item.Quantity,
                        Convert.ToDouble(item.Mark));

                    if (item.SecurityLoc == "US" || item.SecurityLoc == "")
                    {
                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                            item.TradeType,
                            SLTradeCalculator.Locale.Domestic,
                            Convert.ToDecimal(item.RebateRate),
                            Convert.ToDecimal(item.Amount),
                            Convert.ToDecimal(item.CostOfFunds),
                            item.CollateralFlag);
                    }
                    else
                    {
                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                          item.TradeType,
                          SLTradeCalculator.Locale.International,
                          Convert.ToDecimal(item.RebateRate),
                          Convert.ToDecimal(item.Amount),
                          Convert.ToDecimal(item.CostOfFunds),
                          item.CollateralFlag);
                    }


                    DataTrades.AddTrade(item);

                    if (item.EffectiveDate != null)
                        item = DataTrades.LoadTradeExtendedByPk(decimal.Parse(item.TradeNumber));
                }
            }
            catch(Exception e)
            {
                item.Comment = e.Message;
                item.TradeFlag = e.Message;
                item.TradeStatus = StatusDetail.Rejected;
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTradeBreakOut([DataSourceRequest] DataSourceRequest request, SL_TradeBreakOutExtendedProjection item)
        {
            try
            {
                if ((item.TradeStatus != StatusDetail.Approved) && (item.TradeStatus != StatusDetail.Transmitted))
                {
                    if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
                    {
                        if (item.IssueId != null)
                        {
                            var orginalPrice = Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice);
                            var amountPrice = Math.Ceiling(Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice) * Convert.ToDouble(item.Mark));


                            if (((item.Price / orginalPrice) <= 1.20) && ((item.Price / orginalPrice) >= 1))
                            {
                                item.Price = amountPrice;
                            }
                            else
                            {
                                item.Price = item.Price;
                            }
                        }
                    }


                    try
                    {
                        var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)item.EffectiveDate,item.EntityId, item.FeeType);

                        item.RebateRate = Convert.ToDouble(fundingRate.Fund + (decimal)item.FeeOffset);
                    }
                    catch { }

                    if (!string.IsNullOrWhiteSpace(item.ContraEntityId))
                    {
                        var serverCopy = DataTrades.LoadTradeExtendedByPk(decimal.Parse(item.TradeNumber));

                        if (!(string.IsNullOrWhiteSpace(serverCopy.ContraEntityId) ? "" : serverCopy.ContraEntityId).Equals(item.ContraEntityId))
                        {
                            var contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);
                            var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                            item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (double.Parse(contraEntity.BorrowColl) / 100f) : (double.Parse(contraEntity.LoanColl) / 100f);
                            item.Price = Convert.ToDouble(price);

                            if (item.TradeType == TradeType.StockBorrow)
                            {
                                item.BorrowAmount = Convert.ToDecimal(item.Price) * item.BorrowQuantity;
                            }
                            else
                            {
                                item.LoanAmount = Convert.ToDecimal(item.Price) * item.LoanQuantity;
                            }
                        }
                    }

                    List<SL_CostOfFund> costOfFundsList = new List<SL_CostOfFund>();

                    costOfFundsList = DataFunding.LoadCostOfFund(item.EntityId);

                    item.CostOfFunds = (double)SLTradeCalculator.IncludeCostOfFunds(costOfFundsList, item.EntityId, item.TradeType, item.ProfitId, (Currency)item.CurrencyCode);

                    if (!string.IsNullOrWhiteSpace(item.FeeType))
                    {
                        try
                        {
                            var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)item.EffectiveDate, item.EntityId, item.FeeType);

                            item.RebateRate = (double)(fundingRate.Fund + (decimal)item.FeeOffset);
                        }
                        catch
                        {
                            item.RebateRate = (double)item.FeeOffset;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
                    }

                    if (item.TradeType == TradeType.StockBorrow)
                    {
                        item.BorrowAmount = SLTradeCalculator.CalculateMoney(
                            Convert.ToDouble(item.Price),
                            item.BorrowQuantity,
                            Convert.ToDouble(item.Mark));

                        if (item.SecurityLoc == "US" || item.SecurityLoc == "")
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.BorrowAmount),
                                Convert.ToDecimal(item.CostOfFunds),
                                item.CollateralFlag);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.BorrowAmount),
                              Convert.ToDecimal(item.CostOfFunds),
                              item.CollateralFlag);
                        }
                    }
                    else
                    {
                        item.LoanAmount = SLTradeCalculator.CalculateMoney(
                                                 Convert.ToDouble(item.Price),
                                                 item.LoanQuantity,
                                                 Convert.ToDouble(item.Mark));


                        if (item.SecurityLoc == "US" || item.SecurityLoc == "")
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.LoanAmount),
                                Convert.ToDecimal(item.CostOfFunds),
                                item.CollateralFlag);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.LoanAmount),
                              Convert.ToDecimal(item.CostOfFunds),
                              item.CollateralFlag);
                        }
                    }

                    DataTrades.AddTrade(item);

                    if (item.EffectiveDate != null)
                        item = DataTrades.LoadTradeExtendedBreakOutByPk(decimal.Parse(item.TradeNumber));
                }
            }
            catch (Exception e)
            {
                item.Comment = e.Message;
                item.TradeFlag = e.Message;
                item.TradeStatus = StatusDetail.Rejected;
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateBulkTrade([DataSourceRequest] DataSourceRequest request, SL_TradeExtendedProjection item, bool contraEntityChanged)
        {
            try
            {
                if ((item.TradeStatus != StatusDetail.Approved) && (item.TradeStatus != StatusDetail.Transmitted))
                {
                    if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
                    {
                        if (item.IssueId != null)
                        {
                            var orginalPrice = Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice);
                            var amountPrice = Math.Ceiling(Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice) * Convert.ToDouble(item.Mark));


                            if (((item.Price / orginalPrice) <= 1.20) && ((item.Price / orginalPrice) >= 1))
                            {
                                item.Price = amountPrice;
                            }
                            else
                            {
                                item.Price = item.Price;
                            }
                        }
                    }

                    if ((contraEntityChanged) && (!string.IsNullOrWhiteSpace(item.ContraEntityId)))
                    {
                        var contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);

                        var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                        item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (double.Parse(contraEntity.BorrowColl) / 100f) : (double.Parse(contraEntity.LoanColl) / 100f);
                        item.Price = Convert.ToDouble(price);
                        item.Amount = Convert.ToDecimal(item.Price) * item.Quantity;
                    }


                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
                    }

                    item.Amount = SLTradeCalculator.CalculateMoney(
                        Convert.ToDouble(item.Price),
                        item.Quantity,
                        Convert.ToDouble(item.Mark));


                    if (item.SecurityLoc == "US" || item.SecurityLoc == "")
                    {
                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                            item.TradeType,
                            SLTradeCalculator.Locale.Domestic,
                            Convert.ToDecimal(item.RebateRate),
                            Convert.ToDecimal(item.Amount),
                            Convert.ToDecimal(item.CostOfFunds),
                            item.CollateralFlag);
                    }
                    else
                    {
                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                          item.TradeType,
                          SLTradeCalculator.Locale.International,
                          Convert.ToDecimal(item.RebateRate),
                          Convert.ToDecimal(item.Amount),
                          Convert.ToDecimal(item.CostOfFunds),
                          item.CollateralFlag);
                    }
                  

                    item.TradeStatus = StatusDetail.HeldLocal;
                }
            }
            catch
            {
                item.TradeStatus = StatusDetail.Rejected;
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateBulkTradeBreakOut([DataSourceRequest] DataSourceRequest request, SL_TradeBreakOutExtendedProjection item)
        {
            try
            {
                bool contraEntityChanged = false;

                try
                {
                    var _tradeItem = DataTrades.LoadTradeByPk(decimal.Parse(item.TradeNumber));

                    if (_tradeItem.ContraEntity != item.ContraEntityId)
                    {
                        contraEntityChanged = true;
                    }
                }
                catch
                {
                    contraEntityChanged = false;
                }

                if ((item.TradeStatus != StatusDetail.Approved) && (item.TradeStatus != StatusDetail.Transmitted))
                {
                    if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
                    {
                        if (item.IssueId != null)
                        {
                            var orginalPrice = Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice);
                            var amountPrice = Math.Ceiling(Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice) * Convert.ToDouble(item.Mark));


                            if (((item.Price / orginalPrice) <= 1.20) && ((item.Price / orginalPrice) >= 1))
                            {
                                item.Price = amountPrice;
                            }
                            else
                            {
                                item.Price = item.Price;
                            }
                        }
                    }

                    if ((contraEntityChanged) && (!string.IsNullOrWhiteSpace(item.ContraEntityId)))
                    {
                        var contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);

                        var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                        item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (double.Parse(contraEntity.BorrowColl) / 100f) : (double.Parse(contraEntity.LoanColl) / 100f);
                        item.Price = Convert.ToDouble(price);

                        if (item.TradeType == TradeType.StockBorrow)
                        {
                            item.BorrowAmount = Convert.ToDecimal(item.Price) * item.BorrowQuantity;
                        }
                        else
                        {
                            item.LoanAmount = Convert.ToDecimal(item.Price) * item.LoanQuantity;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(item.FeeType))
                    {
                        try
                        {
                            var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)item.EffectiveDate, item.EntityId, item.FeeType);
                            
                            item.RebateRate = (double)(fundingRate.Fund + item.FeeOffset);
                        }
                        catch
                        {
                            item.RebateRate = (double)(item.FeeOffset);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
                    }

                    if (item.TradeType == TradeType.StockBorrow)
                    {
                        item.BorrowAmount = SLTradeCalculator.CalculateMoney(
                        Convert.ToDouble(item.Price),
                        item.BorrowQuantity,
                        Convert.ToDouble(item.Mark));

                        if (item.SecurityLoc == "US" || item.SecurityLoc == "")
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.BorrowAmount),
                                Convert.ToDecimal(item.CostOfFunds),
                                item.CollateralFlag);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.BorrowAmount),
                              Convert.ToDecimal(item.CostOfFunds),
                              item.CollateralFlag);
                        }
                    }
                    else
                    {
                        item.LoanAmount = SLTradeCalculator.CalculateMoney(
                      Convert.ToDouble(item.Price),
                      item.LoanQuantity,
                      Convert.ToDouble(item.Mark));


                        if (item.SecurityLoc == "US" || item.SecurityLoc == "")
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.LoanAmount),
                                Convert.ToDecimal(item.CostOfFunds),
                                item.CollateralFlag);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.LoanAmount),
                              Convert.ToDecimal(item.CostOfFunds),
                              item.CollateralFlag);
                        }
                    }
                  
                   

                    item.TradeStatus = StatusDetail.HeldLocal;
                }
            }
            catch
            {
                item.TradeStatus = StatusDetail.Rejected;
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult MirrorTrade(IEnumerable<SL_TradeExtendedProjection> items)
        {
            foreach (var item in items)
            {
                try
                {
                    item.TradeNumber = null;
                    item.ContraEntityId = null;
                    item.RebateRate = 0;
                    item.RebateRateId = "";

                    item.TradeType = MirrorTradeType(item.ExecutingSystem, item.TradeType);
                    item.DeliveryCode = TradeTypeDefaultDeliveryCode(item.TradeType);

                    DataTrades.AddTrade(item);
                }
                catch
                {
                    item.TradeStatus = StatusDetail.Rejected;
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }


        public ActionResult MirrorTradeBreakOut(IEnumerable<SL_TradeBreakOutExtendedProjection> items)
        {
            foreach (var item in items)
            {
                try
                {
                    var quantity = (item.TradeType == TradeType.StockBorrow) ? item.LoanQuantity : item.BorrowQuantity;
                    var amount = (item.TradeType == TradeType.StockBorrow) ? item.LoanAmount : item.BorrowAmount;

                    item.TradeNumber = null;
                    item.ContraEntityId = null;
                    item.RebateRate = 0;
                    item.RebateRateId = "";

                    item.TradeType = MirrorTradeType(item.ExecutingSystem, item.TradeType);
                    item.DeliveryCode = TradeTypeDefaultDeliveryCode(item.TradeType);

                    if (item.TradeType == TradeType.StockBorrow)
                    {
                        item.BorrowQuantity = quantity;
                        item.BorrowAmount = amount;
                        item.LoanQuantity = 0;
                        item.LoanAmount = 0;
                    }
                    else
                    {
                        item.LoanQuantity = quantity;
                        item.LoanAmount = amount;
                        item.BorrowQuantity = 0;
                        item.BorrowAmount = 0;
                    }

                    DataTrades.AddTrade(item);
                }
                catch
                {
                    item.TradeStatus = StatusDetail.Rejected;
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadBulkUpdatePartial(IEnumerable<SL_TradeExtendedProjection> items)
        {
            var newList = new List<SL_TradeExtendedProjection>();
            var index = 0;

            foreach (var item in items)
            {
                try
                {
                    item.TradeNumber = "*" + ((char)index).ToString(CultureInfo.InvariantCulture);
                    item.ContraEntityId = null;
                    item.RebateRate = 0;
                    item.RebateRateId = "";

                    item.TradeType = MirrorTradeType(item.ExecutingSystem, item.TradeType);

                    item.DeliveryCode = TradeTypeDefaultDeliveryCode(item.TradeType);


                    var newItem = item;
                    newList.Add(newItem);
                }
                catch
                {
                    item.TradeStatus = StatusDetail.Rejected;
                }
                index++;
            }

            return newList.Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/Trade/_UpdateTrades.cshtml", newList);
        }

        public ActionResult LoadBulkUpdateBreakOutPartial(IEnumerable<SL_TradeBreakOutExtendedProjection> items)
        {
            var newList = new List<SL_TradeBreakOutExtendedProjection>();
            var index = 0;

            foreach (var item in items)
            {
                try
                {
                    var quantity = (item.TradeType == TradeType.StockBorrow) ? item.BorrowQuantity : item.LoanQuantity;
                    var amount = (item.TradeType == TradeType.StockBorrow) ? item.BorrowAmount : item.LoanAmount;

                    item.TradeNumber = "*" + ((char)index).ToString(CultureInfo.InvariantCulture);                    
                    item.ContraEntityId = null;
                    item.RebateRate = 0;
                    item.RebateRateId = "";
                    item.TradeStatus = StatusDetail.Pending;
                    
                    item.TradeType = MirrorTradeType(item.ExecutingSystem, item.TradeType);

                    item.DeliveryCode = TradeTypeDefaultDeliveryCode(item.TradeType);

                    if (item.TradeType == TradeType.StockBorrow)
                    {
                        item.BorrowQuantity = quantity;
                        item.BorrowAmount = amount;
                        item.LoanQuantity = 0;
                        item.LoanAmount = 0;
                    }
                    else
                    {
                        item.LoanQuantity = quantity;
                        item.LoanAmount = amount;
                        item.BorrowQuantity = 0;
                        item.BorrowAmount = 0;
                    }


                    var newItem = item;
                    newList.Add(newItem);
                }
                catch
                {
                    item.TradeStatus = StatusDetail.Rejected;
                }
                index++;
            }

            return newList.Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/Trade/_UpdateTradesBreakOut.cshtml", newList);
        }

        public PartialViewResult BulkUpdateTrades(IEnumerable<SL_TradeExtendedProjection> items)
        {
            return items.ToList().Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/Trade/_UpdateTrades.cshtml", items);
        }

        public PartialViewResult BulkUpdateTradesBreakOut(IEnumerable<SL_TradeBreakOutExtendedProjection> items)
        {            
            return items.ToList().Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/Trade/_UpdateTradesBreakOut.cshtml", items.Where(x => x.TradeStatus == StatusDetail.Pending).ToList());
        }

        public ActionResult ReadTradeSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            var tradeList = new List<SL_TradeBreakOutExtendedProjection>();
            var removeCancelled = bool.Parse(DataSystemValues.LoadSystemValue("RemoveCancelled", "true"));

            try
            {
                foreach (var _entityId in entityId)
                {
                    tradeList.AddRange(DataTrades.LoadTradesExtendedBreakOut(effectiveDate, _entityId));
                }

                if (removeCancelled)
                {
                    tradeList.RemoveAll(x => x.TradeStatus == StatusDetail.Cancelled);
                }
            }
            catch
            {
                tradeList = new List<SL_TradeBreakOutExtendedProjection>();
            }

            return Extended.JsonMax(tradeList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadScratchPadSummary([DataSourceRequest] DataSourceRequest request)
        {
            List<TradeChangeModel> tradeList = new List<TradeChangeModel>();

            return Json(tradeList.ToDataSourceResult(request));
        }

       public PartialViewResult LoadTradeTicket(string entityId, int issueId)
        {
            var item = new SingleTradeModel();          

            var executingTypes = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeType();
            try
            {
                var company = DataEntity.LoadEntities().Where(x => x.CompanyId.ToString() == entityId).First();
                var issueItem = DataIssue.LoadIssueById(issueId);

                item.Company = company;
                item.Trade1 = new SL_TradeExtendedProjection();
                item.Trade2 = new SL_TradeExtendedProjection();

                try
                {
                    item.AllocationList = DataStockRecord.LoadStockRecordByIssue(DateTime.Today, entityId, issueId.ToString()).Where(x => x.IsOptedFPL).Select(q => new FPLSpecificAccountAllocationModel()
                    {
                        IsEnabled = true,
                        ContraEntity = q.ContraEntityFPL,
                        AccountNumber = q.AccountNumber,
                        AccountType = q.AccountType,
                        OriginalQuantity = q.SettledQuantity,
                        Quantity = q.SettledQuantity
                    }).ToList();
                }
                catch
                {
                    item.AllocationList = new List<FPLSpecificAccountAllocationModel>();
                }

                double price;

                try
                {
                    var issuePrice = DataIssue.LoadIssuePrice(company.CompanyId.ToString(), issueItem.IssueId);

                    price = Convert.ToDouble(SLTradeCalculator.CalculatePrice(company.CompanyId.ToString(), Convert.ToDecimal(issuePrice.CurrentCashPrice), null));

                }
                catch
                {
                    price = 1;
                }         

                item.Trade1.ExecutingSystem = company.DefaultExecutionSystem;
                item.Trade1.EntityId = entityId;
                item.Trade1.IssueId = issueItem.IssueId;
                item.Trade1.SecurityNumber = string.IsNullOrWhiteSpace(issueItem.Cusip) ? "" : issueItem.Cusip;
                item.Trade1.Ticker = (company.Country == Country.UnitedStates) ? issueItem.Ticker : issueItem.SecNumber;
                item.Trade1.Sedol = string.IsNullOrWhiteSpace(issueItem.SEDOL) ? "" : issueItem.SEDOL;
                item.Trade1.Isin = string.IsNullOrWhiteSpace(issueItem.ISIN) ? "" : issueItem.ISIN;
                item.Trade1.Quick = string.IsNullOrWhiteSpace(issueItem.Quick) ? "" : issueItem.Quick;
                item.Trade1.Price = price;
                item.Trade1.Mark = 1.02;
                item.Trade1.TradeDate = DateCalculator.Default.Today;
                item.Trade1.MarkParameterId = "%";
                item.Trade1.PriceCalcType = SL_TradePriceAmountCalcType.CASH;

                item.Trade2.ExecutingSystem = company.DefaultExecutionSystem;
                item.Trade2.EntityId = entityId;
                item.Trade2.IssueId = issueItem.IssueId;
                item.Trade2.SecurityNumber = string.IsNullOrWhiteSpace(issueItem.Cusip) ? "" : issueItem.Cusip;
                item.Trade2.Ticker = (company.Country == Country.UnitedStates) ? issueItem.Ticker : issueItem.SecNumber;
                item.Trade2.Sedol = string.IsNullOrWhiteSpace(issueItem.SEDOL) ? "" : issueItem.SEDOL;
                item.Trade2.Isin = string.IsNullOrWhiteSpace(issueItem.ISIN) ? "" : issueItem.ISIN;
                item.Trade2.Quick = string.IsNullOrWhiteSpace(issueItem.Quick) ? "" : issueItem.Quick;
                item.Trade2.Price = price;
                item.Trade2.Mark = 1.02;
                item.Trade2.TradeDate = DateCalculator.Default.Today;
                item.Trade2.MarkParameterId = "%";
                item.Trade2.PriceCalcType = SL_TradePriceAmountCalcType.CASH;


                if (item.Trade1.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE)
                {
                    item.Trade1.TradeType = TradeType.BorrowVsRate;
                    item.Trade1.DeliveryCode = SL_DeliveryCode.NONE;

                    item.Trade2.TradeType = TradeType.LoanVsRate;
                    item.Trade2.DeliveryCode = SL_DeliveryCode.NONE;
                }
                else
                {
                    item.Trade1.TradeType = TradeType.StockBorrow;
                    item.Trade1.DeliveryCode = SL_DeliveryCode.PTS;

                    item.Trade2.TradeType = TradeType.StockLoan;
                    item.Trade2.DeliveryCode = SL_DeliveryCode.CCF;
                }

                CountryMapping countryCodeTrade1 = DataIssue.LoadCountryMapping(executingTypes.Where(x => x.TradeType == item.Trade1.TradeType && x.ExecutionSystemType == item.Trade1.ExecutingSystem).Select(x => x.DefaultSecuritySettleLocation).First());
                CountryMapping countryCodeTrade2 = DataIssue.LoadCountryMapping(executingTypes.Where(x => x.TradeType == item.Trade2.TradeType && x.ExecutionSystemType == item.Trade2.ExecutingSystem).Select(x => x.DefaultSecuritySettleLocation).First());

                string countryCodeTrade1Override = executingTypes.Where(x => x.TradeType == item.Trade1.TradeType && x.ExecutionSystemType == item.Trade1.ExecutingSystem).Select(x => x.DefaultSecuritySettleLocationOverride).First();
                string countryCodeTrade2Override = executingTypes.Where(x => x.TradeType == item.Trade1.TradeType && x.ExecutionSystemType == item.Trade1.ExecutingSystem).Select(x => x.DefaultSecuritySettleLocationOverride).First();

                item.Trade1.Amount = SLTradeCalculator.CalculateMoney(price, item.Trade1.Quantity, Convert.ToDouble(item.Trade1.Mark));
                item.Trade1.TradeNumber = "";
                item.Trade1.RebateRateId = "";
                item.Trade1.CashSettleDate = (item.Trade1.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE) ? DateCalculator.Default.NextBusinessDay : DateCalculator.Default.Today;
                item.Trade1.SecuritySettleDate = (item.Trade1.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE) ? DateCalculator.Default.GetBusinessDayByBusinessDays(DateCalculator.Default.Today,2) : DateCalculator.Default.Today;
                item.Trade1.CollateralFlag = SL_CollateralFlag.C;
                item.Trade1.CurrencyCode = executingTypes.Where(x => x.TradeType == item.Trade1.TradeType && x.ExecutionSystemType == item.Trade1.ExecutingSystem).Select(x => x.DefaultCurrency).First();
                item.Trade1.SecurityLoc = (string.IsNullOrWhiteSpace(countryCodeTrade1Override)) ? countryCodeTrade1.CountryCode.ToString() : countryCodeTrade1Override;
                item.Trade1.CashLoc = (string.IsNullOrWhiteSpace(countryCodeTrade1Override)) ? countryCodeTrade1.CountryCode.ToString() : countryCodeTrade1Override;
                item.Trade1.IsStp = true;
             
                item.Trade1 = SLTradeCalculator.GenerateDefaults(item.Trade1, issueItem, null, false, bool.Parse(DataSystemValues.LoadSystemValue("Round" + item.Trade1.ExecutingSystem, true.ToString())));

                item.Trade2.Amount = SLTradeCalculator.CalculateMoney(price, item.Trade2.Quantity, Convert.ToDouble(item.Trade2.Mark));
                item.Trade2.TradeNumber = "";
                item.Trade2.RebateRateId = "";
                item.Trade2.CashSettleDate = (item.Trade2.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE) ? DateCalculator.Default.NextBusinessDay : DateCalculator.Default.Today;
                item.Trade2.SecuritySettleDate = (item.Trade2.ExecutingSystem == SL_ExecutionSystemType.GLOBALONE) ? DateCalculator.Default.GetBusinessDayByBusinessDays(DateCalculator.Default.Today, 2) : DateCalculator.Default.Today;
                item.Trade2.CollateralFlag = SL_CollateralFlag.C;
                item.Trade2.CurrencyCode = executingTypes.Where(x => x.TradeType == item.Trade2.TradeType && x.ExecutionSystemType == item.Trade2.ExecutingSystem).Select(x => x.DefaultCurrency).First();
                item.Trade2.SecurityLoc = (string.IsNullOrWhiteSpace(countryCodeTrade2Override)) ? countryCodeTrade2.CountryCode.ToString() : countryCodeTrade2Override;
                item.Trade2.CashLoc = (string.IsNullOrWhiteSpace(countryCodeTrade2Override)) ? countryCodeTrade2.CountryCode.ToString() : countryCodeTrade2Override;
                item.Trade2.IsStp = true;

                item.Trade2 = SLTradeCalculator.GenerateDefaults(item.Trade2, issueItem, null, false, bool.Parse(DataSystemValues.LoadSystemValue("Round" + item.Trade2.ExecutingSystem, true.ToString())));

                item.IsMirror = false;
            }
            catch (Exception e)
            {
                ThrowJsonError(e);
            }

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_BoxCalculationTrade.cshtml", item);
        }

        public PartialViewResult EditTrade(SL_TradeExtendedProjection item)
        {
            //item = TradeValidationService.GenerateDefaults(item, false);
            
            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_BoxCalculationTrade.cshtml", item);
        }

        public PartialViewResult LoadScratchpad()
        {          
            return PartialView("~/Areas/DomesticTrading/Views/Trade/_Scratchpad.cshtml", new List<TradeChangeModel>());
        }

        public bool AddTrade(SL_TradeExtendedProjection trade)
        {
            bool success;

            try
            {
                ModelState.Clear();


                Issue issue = null;
                SL_ContraEntity contraEntity;

                if (!trade.SecurityNumber.Equals(""))
                {
                    issue = DataIssue.LoadIssue(trade.SecurityNumber);
                }

                if (!trade.EntityId.Equals("") && !trade.ContraEntityId.Equals(""))
                {
                    contraEntity = DataContraEntity.LoadContraEntity(trade.EntityId, trade.ContraEntityId);
                }
                else
                {
                    contraEntity = new SL_ContraEntity
                    {
                        BorrowColl = "102",
                        BorrowMarkCode = "%",
                        LoanColl =  "102",
                        LoanMarkCode = "%",
                        MarkRndHse = 0.9,
                        MarkRndInst = "U",
                        MarkValHse = 0.9,
                        MarkValInst = "U"
                    };
                }

               
                trade = SLTradeCalculator.GenerateDefaults(trade,issue, contraEntity, false, bool.Parse(DataSystemValues.LoadSystemValue("Round" + trade.ExecutingSystem, true.ToString())));
                trade.RebateRateId = "";

                success = DataTrades.AddTrade(trade);
            }
            catch (Exception error)
            {
                ThrowJsonError(error);

                success = false;
            }

            return success;
        }

        public bool AddTradeConfirm(SL_TradeExtendedProjection trade)
        {
            bool success;

            try
            {
                ModelState.Clear();


                Issue issue = null;
                SL_ContraEntity contraEntity;

                if (!trade.SecurityNumber.Equals(""))
                {
                    issue = DataIssue.LoadIssue(trade.SecurityNumber);
                }

                if (!trade.EntityId.Equals("") && !trade.ContraEntityId.Equals(""))
                {
                    contraEntity = DataContraEntity.LoadContraEntity(trade.EntityId, trade.ContraEntityId);
                }
                else
                {
                    contraEntity = new SL_ContraEntity
                    {
                        BorrowColl = "102",
                        BorrowMarkCode = "%",
                        LoanColl = "102",
                        LoanMarkCode = "%",
                        MarkRndHse = 0.9,
                        MarkRndInst = "U",
                        MarkValHse = 0.9,
                        MarkValInst = "U"
                    };
                }

                trade.ProfitId = PcMatrixService.SetDefaults(trade, DataPCMatrix.LoadPCMatrix(trade.EntityId));
                trade = SLTradeCalculator.GenerateDefaults(trade, issue, contraEntity, false, bool.Parse(DataSystemValues.LoadSystemValue("Round" + trade.ExecutingSystem, true.ToString())));
                trade.RebateRateId = "";

                success = DataTrades.AddTrade(trade);

                if (!String.IsNullOrWhiteSpace(trade.TradeNumber))
                {
                    DataExternalOperations.AddTradeToContract(DataTrades.LoadTradeExtendedByPk(decimal.Parse(trade.TradeNumber)), true);
                }
            }
            catch (Exception error)
            {
                ThrowJsonError(error);

                success = false;
            }

            return success;
        }

        public bool AddSingleTradeModel(SingleTradeModel model)
        {
            bool success;

            try
            {
                ModelState.Clear();

                /** Trade 1 **/
                Issue issue = null;
                SL_ContraEntity contraEntity;

                if (!string.IsNullOrWhiteSpace(model.Trade1.SecurityNumber))
                {
                    issue = DataIssue.LoadIssue(model.Trade1.SecurityNumber);
                }
                else
                {
                    issue = DataIssue.LoadIssueById((int)model.Trade1.IssueId);
                }

                if (!model.Trade1.EntityId.Equals("") && !model.Trade1.ContraEntityId.Equals(""))
                {
                    contraEntity = DataContraEntity.LoadContraEntity(model.Trade1.EntityId, model.Trade1.ContraEntityId);
                }
                else
                {
                    contraEntity = new SL_ContraEntity
                    {
                        BorrowColl = "102",
                        BorrowMarkCode = "%",
                        LoanColl = "102",
                        LoanMarkCode = "%",
                        MarkRndHse = 0.9,
                        MarkRndInst = "U",
                        MarkValHse = 0.9,
                        MarkValInst = "U"
                    };
                }

                if ( string.IsNullOrWhiteSpace( model.Trade1.ProfitId ) )
                {
                    model.Trade1.ProfitId = PcMatrixService.SetDefaults( model.Trade1, DataPCMatrix.LoadPCMatrix( model.Trade1.EntityId ) );
                }


                model.Trade1.FeeType = string.IsNullOrWhiteSpace(model.Trade1.FeeType) ? "" : model.Trade1.FeeType;
                model.Trade1.FeeOffset = (model.Trade1.FeeOffset == null) ? 0 : model.Trade1.FeeOffset;


                if (!string.IsNullOrWhiteSpace(model.Trade1.FeeType))
                {
                    try
                    {
                        var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)model.Trade1.EffectiveDate, model.Trade1.EntityId, model.Trade1.FeeType);

                        model.Trade1.RebateRate = Convert.ToDouble(fundingRate.Fund + (decimal)model.Trade1.FeeOffset);
                    }
                    catch (Exception)
                    {
                        model.Trade1.RebateRate = Convert.ToDouble(model.Trade1.FeeOffset);
                    }
                }

                if ((model.Trade1.Quantity % 100) != 0)
                {
                    model.Trade1 = SLTradeCalculator.GenerateDefaults(model.Trade1, issue, contraEntity, true, false);
                }
                else
                {
                    model.Trade1 = SLTradeCalculator.GenerateDefaults(model.Trade1, issue, contraEntity, true, bool.Parse(DataSystemValues.LoadSystemValue("Round" + model.Trade1.ExecutingSystem, true.ToString())));
                }

                model.Trade1.RebateRateId = "";

                success = DataTrades.AddTrade(model.Trade1);

                /** Trade 2 **/
                if (model.IsMirror)
                {
                    if (!model.Trade2.SecurityNumber.Equals(""))
                    {
                        issue = DataIssue.LoadIssue(model.Trade2.SecurityNumber);
                    }

                    if (!model.Trade2.EntityId.Equals("") && !model.Trade2.ContraEntityId.Equals(""))
                    {
                        contraEntity = DataContraEntity.LoadContraEntity(model.Trade2.EntityId, model.Trade2.ContraEntityId);
                    }
                    else
                    {
                        contraEntity = new SL_ContraEntity
                        {
                            BorrowColl = "102",
                            BorrowMarkCode = "%",
                            LoanColl = "102",
                            LoanMarkCode = "%",
                            MarkRndHse = 0.9,
                            MarkRndInst = "U",
                            MarkValHse = 0.9,
                            MarkValInst = "U"
                        };
                    }

                    if ( string.IsNullOrWhiteSpace( model.Trade2.ProfitId ) )
                    {
                        model.Trade2.ProfitId = PcMatrixService.SetDefaults( model.Trade2, DataPCMatrix.LoadPCMatrix( model.Trade2.EntityId ) );
                    }

                    model.Trade2.FeeType = string.IsNullOrWhiteSpace(model.Trade2.FeeType) ? "" : model.Trade2.FeeType;
                    model.Trade2.FeeOffset = (model.Trade2.FeeOffset == null) ? 0 : model.Trade2.FeeOffset;

                    if (!string.IsNullOrWhiteSpace(model.Trade2.FeeType))
                    {
                        try
                        {
                            var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)model.Trade2.EffectiveDate, model.Trade2.EntityId, model.Trade2.FeeType);

                            model.Trade2.RebateRate = Convert.ToDouble(fundingRate.Fund + (decimal)model.Trade2.FeeOffset);
                        }
                        catch (Exception )
                        {
                            model.Trade2.RebateRate = Convert.ToDouble(model.Trade2.FeeOffset);
                        }
                    }

                    if ((model.Trade2.Quantity % 100) != 0)
                    {
                        model.Trade2 = SLTradeCalculator.GenerateDefaults(model.Trade2, issue, contraEntity, true, false);
                    }
                    else
                    {
                        model.Trade2 = SLTradeCalculator.GenerateDefaults(model.Trade2, issue, contraEntity, true, bool.Parse(DataSystemValues.LoadSystemValue("Round" + model.Trade1.ExecutingSystem, true.ToString())));
                    }

                    model.Trade2.RebateRateId = "";

                    success = DataTrades.AddTrade(model.Trade2);
                }
            }
            catch (Exception error)
            {
                ThrowJsonError(error);

                success = false;
            }

            return success;
        }

        public bool AddSingleTradeModelConfirm(SingleTradeModel model, List<FPLSpecificAccountAllocationModel> specificList)
        {
            bool success;

            try
            {
                ModelState.Clear();

                /** Trade 1 **/
                Issue issue = null;
                SL_ContraEntity contraEntity;

                if (!string.IsNullOrWhiteSpace(model.Trade1.SecurityNumber))
                {
                    issue = DataIssue.LoadIssue(model.Trade1.SecurityNumber);
                }
                else
                {
                    issue = DataIssue.LoadIssueById((int)model.Trade1.IssueId);
                }

                if (!model.Trade1.EntityId.Equals("") && !model.Trade1.ContraEntityId.Equals(""))
                {
                    contraEntity = DataContraEntity.LoadContraEntity(model.Trade1.EntityId, model.Trade1.ContraEntityId);
                }
                else
                {
                    contraEntity = new SL_ContraEntity
                    {
                        BorrowColl = "102",
                        BorrowMarkCode = "%",
                        LoanColl = "102",
                        LoanMarkCode = "%",
                        MarkRndHse = 0.9,
                        MarkRndInst = "U",
                        MarkValHse = 0.9,
                        MarkValInst = "U"
                    };
                }



                model.Trade1.FeeType = string.IsNullOrWhiteSpace(model.Trade1.FeeType) ? "" : model.Trade1.FeeType;
                model.Trade1.FeeOffset = (model.Trade1.FeeOffset == null) ? 0 : model.Trade1.FeeOffset;


                if (!string.IsNullOrWhiteSpace(model.Trade1.FeeType))
                {
                    try
                    {
                        var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)model.Trade1.EffectiveDate, model.Trade1.EntityId, model.Trade1.FeeType);

                        model.Trade1.RebateRate = Convert.ToDouble(fundingRate.Fund + (decimal)model.Trade1.FeeOffset);
                    }
                    catch (Exception)
                    {
                        model.Trade1.RebateRate = Convert.ToDouble(model.Trade1.FeeOffset);
                    }
                }


                model.Trade1.ProfitId = PcMatrixService.SetDefaults(model.Trade1, DataPCMatrix.LoadPCMatrix(model.Trade1.EntityId));

                if ((model.Trade1.Quantity % 100) != 0)
                {
                    model.Trade1 = SLTradeCalculator.GenerateDefaults(model.Trade1, issue, contraEntity, false, false);
                }
                else
                {
                    model.Trade1 = SLTradeCalculator.GenerateDefaults(model.Trade1, issue, contraEntity, false, bool.Parse(DataSystemValues.LoadSystemValue("Round" + model.Trade1.ExecutingSystem, true.ToString())));
                }

                model.Trade1.RebateRateId = "";

                success = DataTrades.AddTrade(model.Trade1);

                try
                {
                    if (model.Trade1.TradeType == TradeType.FullyPaidBorrow)
                    {
                        var company = DataEntity.LoadEntity(model.Trade1.EntityId);

                        foreach (var _item in specificList)
                        {
                            var item = new SL_FPLSpecificAccountAllocation();
                            item.AccountNumber = _item.AccountNumber;
                            item.AccountType = _item.AccountType.ToString();
                            item.ContractNumber = "";
                            item.ContraEntity = model.Trade1.ContraEntityId;
                            item.EntityId = model.Trade1.EntityId;
                            item.FirmId = company.ShortName;
                            item.IssueId = (int)model.Trade1.IssueId;
                            item.Quantity = _item.Quantity;
                            item.TradeNumber = int.Parse(model.Trade1.TradeNumber);
                            item.TradeType = model.Trade1.TradeType;

                            DataFPLending.AddFPLSpecificAccountAllocationModel(item);
                        }
                    }
                }
                catch(Exception e)
                {

                }

                if (!String.IsNullOrWhiteSpace(model.Trade1.TradeNumber))
                {
                    DataExternalOperations.AddTradeToContract(DataTrades.LoadTradeExtendedByPk(decimal.Parse(model.Trade1.TradeNumber)), true);
                }

                /** Trade 2 **/
                if (model.IsMirror)
                {
                    if (!string.IsNullOrWhiteSpace(model.Trade2.SecurityNumber))
                    {
                        issue = DataIssue.LoadIssue(model.Trade2.SecurityNumber);
                    }
                    else
                    {
                        issue = DataIssue.LoadIssueById((int)model.Trade2.IssueId);
                    }

                    if (!model.Trade2.EntityId.Equals("") && !model.Trade2.ContraEntityId.Equals(""))
                    {
                        contraEntity = DataContraEntity.LoadContraEntity(model.Trade2.EntityId, model.Trade2.ContraEntityId);
                    }
                    else
                    {
                        contraEntity = new SL_ContraEntity
                        {
                            BorrowColl = "102",
                            BorrowMarkCode = "%",
                            LoanColl = "102",
                            LoanMarkCode = "%",
                            MarkRndHse = 0.9,
                            MarkRndInst = "U",
                            MarkValHse = 0.9,
                            MarkValInst = "U"
                        };
                    }

                    model.Trade2.FeeType = string.IsNullOrWhiteSpace(model.Trade2.FeeType) ? "" : model.Trade2.FeeType;
                    model.Trade2.FeeOffset = (model.Trade2.FeeOffset == null) ? 0 : model.Trade2.FeeOffset;

                    if (!string.IsNullOrWhiteSpace(model.Trade2.FeeType))
                    {

                        try
                        {
                            var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)model.Trade2.EffectiveDate, model.Trade2.EntityId, model.Trade2.FeeType);

                            model.Trade2.RebateRate = Convert.ToDouble(fundingRate.Fund + (decimal)model.Trade2.FeeOffset);
                        }
                        catch (Exception)
                        {
                            model.Trade2.RebateRate = Convert.ToDouble(model.Trade2.FeeOffset);
                        }
                    }


                    model.Trade2.ProfitId = PcMatrixService.SetDefaults(model.Trade2, DataPCMatrix.LoadPCMatrix(model.Trade2.EntityId));

                    if ((model.Trade2.Quantity % 100) != 0)
                    {
                        model.Trade2 = SLTradeCalculator.GenerateDefaults(model.Trade2, issue, contraEntity, false, false);
                    }
                    else
                    {
                        model.Trade2 = SLTradeCalculator.GenerateDefaults(model.Trade2, issue, contraEntity, false, bool.Parse(DataSystemValues.LoadSystemValue("Round" + model.Trade1.ExecutingSystem, true.ToString())));
                    }

                    model.Trade2.RebateRateId = "";

                    success = DataTrades.AddTrade(model.Trade2);

                    if (!String.IsNullOrWhiteSpace(model.Trade2.TradeNumber))
                    {
                        DataExternalOperations.AddTradeToContract(DataTrades.LoadTradeExtendedByPk(decimal.Parse(model.Trade2.TradeNumber)), true);
                    }
                }
            }
            catch (Exception error)
            {
                ThrowJsonError(error);

                success = false;
            }

            return success;
        }
        /*****ScratchPad Function******/
       
        public ActionResult TradeModel_Update([DataSourceRequest] DataSourceRequest request, TradeChangeModel item, bool contraEntityChanged)
        {
            List<SL_CostOfFund> costOfFundsList = new List<SL_CostOfFund>();

            costOfFundsList = DataFunding.LoadCostOfFund(item.EntityId);

            item.CostOfFunds = SLTradeCalculator.IncludeCostOfFunds(costOfFundsList, item.EntityId, item.TradeType, item.ProfitId, item.CurrencyCode);

            try
            {
                GenerateRules.Process(item.EntityId, item);

                item.RuleAlert = "";
            }
            catch (AlertException alertEx)
            {
                var _activity = GenerateRules.GenerateFailActivity(item, alertEx.Message);

                try
                {
                    DataActivity.AddActivity(_activity);
                    item.RuleAlert = alertEx.Message;
                }
                catch
                {

                }                
            }
            catch (ActivityException activityEx)
            {
                var _activity = GenerateRules.GenerateFailActivity(item, activityEx.Message);

                try
                {
                    DataActivity.AddActivity(_activity);
                    item.RuleAlert = activityEx.Message;
                }
                catch
                {

                }
            }

            if (!string.IsNullOrWhiteSpace(item.FeeType))
            {
                try
                {
                    var fundingRate = DataFundingRates.LoadFundingRatesByFee(DateTime.Today, item.EntityId, item.FeeType);

                    item.RebateRate = fundingRate.Fund + (decimal)item.FeeOffSet;
                }
                catch (Exception)
                {
                    item.RebateRate = item.FeeOffSet;
                }
            }

            if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
            {
                var orginalPrice = Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, item.IssueId).CurrentCashPrice);
                var amountPrice = Math.Ceiling(Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, item.IssueId).CurrentCashPrice) * Convert.ToDecimal(item.Mark));


                if (((item.Price / orginalPrice) <= 1.20m) && ((item.Price / orginalPrice) >= 1))
                {
                    item.Price = amountPrice;
                }
                else
                {
                    item.Price = item.Price;
                }
            }

            if ((contraEntityChanged) && (!string.IsNullOrWhiteSpace(item.ContraEntityId)))
            {
               var contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);
               
                var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);
               
                item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (decimal.Parse(contraEntity.BorrowColl) / 100m) : (decimal.Parse(contraEntity.LoanColl) / 100m);
                item.Price = (item.IsCustomPrice) ? item.Price : price;
                item.Amount = item.Price * item.Quantity;
            }

           if (string.IsNullOrWhiteSpace(item.ProfitId))
           {
               item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
           }

            item.Amount = SLTradeCalculator.CalculateMoney(
                Convert.ToDouble(item.Price),
                item.Quantity,
                Convert.ToDouble(item.Mark));

            if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
            {
                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                    item.TradeType,
                    SLTradeCalculator.Locale.Domestic,
                    Convert.ToDecimal(item.RebateRate),
                    Convert.ToDecimal(item.Amount),
                    item.CostOfFunds,
                    item.CollateralFlag);
            }
            else
            {
                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                  item.TradeType,
                  SLTradeCalculator.Locale.International,
                  Convert.ToDecimal(item.RebateRate),
                  Convert.ToDecimal(item.Amount),
                  item.CostOfFunds,
                  item.CollateralFlag);
            }


            var exceedPrepay = false;


            if (item.ExecutingType == SL_ExecutionSystemType.GLOBALONE)
            {
                if (!string.IsNullOrWhiteSpace(item.ContraEntityId))
                {
                    exceedPrepay = DataContraEntity.ExceededPrepayLimit(item.EntityId, item.ContraEntityId, item.Amount, item.CurrencyCode, item.SecuritySettleDate);

                    if (exceedPrepay)
                    {
                        item.RuleAlert = "Excceeded Prepay Limit";
                    }
                }
            }
         
            item.Enabled = true;

            return Json(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult TradeBreakOutModel_Update([DataSourceRequest] DataSourceRequest request, TradeBreakOutChangeModel item, bool contraEntityChanged)
        {
            try
            {
                var fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)item.EffectiveDate, item.EntityId, item.FeeType);

                item.RebateRate = fundingRate.Fund + (decimal)item.FeeOffSet;
            }
            catch (Exception)
            {
                item.RebateRate = item.FeeOffSet;
            }

            if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
            {
                var orginalPrice = Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, item.IssueId).CurrentCashPrice);
                var amountPrice = Math.Ceiling(Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, item.IssueId).CurrentCashPrice) * Convert.ToDecimal(item.Mark));


                if (((item.Price / orginalPrice) <= 1.20m) && ((item.Price / orginalPrice) >= 1))
                {
                    item.Price = amountPrice;
                }
                else
                {
                    item.Price = item.Price;
                }
            }

            if ((contraEntityChanged) && (!string.IsNullOrWhiteSpace(item.ContraEntityId)))
            {
                var contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);

                var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (decimal.Parse(contraEntity.BorrowColl) / 100m) : (decimal.Parse(contraEntity.LoanColl) / 100m);
                item.Price = price;

                if (item.TradeType == TradeType.StockBorrow)
                {
                    item.BorrowAmount = item.Price * item.BorrowQuantity;
                }
                else
                {
                    item.LoanAmount = item.Price * item.LoanQuantity;
                }
            }

            if (string.IsNullOrWhiteSpace(item.ProfitId))
            {
                item.ProfitId = PcMatrixService.SetDefaults(item, DataPCMatrix.LoadPCMatrix(item.EntityId));
            }


            if (item.TradeType == TradeType.StockBorrow)
            {
                item.BorrowAmount = SLTradeCalculator.CalculateMoney(
                    Convert.ToDouble(item.Price),
                    item.BorrowQuantity,
                    Convert.ToDouble(item.Mark));


                if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                {
                    item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                        item.TradeType,
                        SLTradeCalculator.Locale.Domestic,
                        Convert.ToDecimal(item.RebateRate),
                        Convert.ToDecimal(item.BorrowAmount),
                        0,
                        SL_CollateralFlag.C);
                }
                else
                {
                    item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                      item.TradeType,
                      SLTradeCalculator.Locale.International,
                      Convert.ToDecimal(item.RebateRate),
                      Convert.ToDecimal(item.BorrowAmount),
                      0,
                     SL_CollateralFlag.C);
                }
            }
            else
            {
                item.LoanAmount = SLTradeCalculator.CalculateMoney(
                    Convert.ToDouble(item.Price),
                    item.LoanQuantity,
                    Convert.ToDouble(item.Mark));

                if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                {
                    item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                        item.TradeType,
                        SLTradeCalculator.Locale.Domestic,
                        Convert.ToDecimal(item.RebateRate),
                        Convert.ToDecimal(item.LoanAmount),
                        0,
                        SL_CollateralFlag.C);
                }
                else
                {
                    item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                      item.TradeType,
                      SLTradeCalculator.Locale.International,
                      Convert.ToDecimal(item.RebateRate),
                      Convert.ToDecimal(item.LoanAmount),
                      0,
                     SL_CollateralFlag.C);
                }
            }

        

            item.Enabled = true;

            return Json(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult TradeModel_Destroy([DataSourceRequest] DataSourceRequest request, TradeChangeModel item)
        {
            return Json(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TradeBreakoutModel_Destroy([DataSourceRequest] DataSourceRequest request, TradeBreakOutChangeModel item)
        {
            return Json(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateBulkTradesEditor(string contraEntityId, string rate, string profitCenter, string mark, DateTime? termDate, IEnumerable<SL_TradeExtendedProjection> list)
        {
            var slTradeExtendedProjections = list as SL_TradeExtendedProjection[] ?? list.ToArray();
            if (!slTradeExtendedProjections.Any()) return Json(list);

            var pcMatrixList = DataPCMatrix.LoadPCMatrix(list.Select(x => x.EntityId).Distinct().First());
            var contraEntity = (contraEntityId.Equals("") || (contraEntityId == null)) ? null : DataContraEntity.LoadContraEntity(list.Select(x => x.EntityId).First(), contraEntityId);

            foreach (var item in slTradeExtendedProjections)
            {
                if (!contraEntityId.Equals("") && (contraEntityId != null))
                {
                    item.ContraEntityId = contraEntityId;

                    var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                    item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (double.Parse(contraEntity.BorrowColl) / 100f) : (double.Parse(contraEntity.LoanColl) / 100f);
                    item.Price = Convert.ToDouble(price);
                    item.Amount = Convert.ToDecimal(item.Price) * item.Quantity;

                    item.TradeStatus = StatusDetail.HeldLocal;
                }

                if ((rate != null) && !(rate.Equals("")))
                {
                    try
                    {
                        item.RebateRate = double.Parse(rate);

                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), item.Amount,Convert.ToDecimal(item.CostOfFunds), item.CollateralFlag);
                        item.TradeStatus = StatusDetail.HeldLocal;
                    }
                    catch { }
                }


                if (termDate != null)
                {
                    item.TermDate = termDate;
                    item.TradeStatus = StatusDetail.HeldLocal;
                }

                if ((mark != null) && !(mark.Equals("")))
                {
                    try
                    {
                        item.Mark = Convert.ToDouble(mark);

                        contraEntity = new SL_ContraEntity()
                        {
                            BorrowColl = mark.Equals("") ? "102" : mark.Replace(".", "").ToString(),
                            BorrowMarkCode = "%",
                            LoanColl = mark.Equals("") ? "102" : mark.Replace(".", "").ToString(),
                            LoanMarkCode = "%",
                            MarkRndHse = 0.9,
                            MarkRndInst = "U",
                            MarkValHse = 0.9,
                            MarkValInst = "U"
                        };

                        if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
                        {
                            var orginalPrice = Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice);
                            var amountPrice = Math.Ceiling(Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice) * Convert.ToDouble(item.Mark));


                            if (((item.Price / orginalPrice) <= 1.20) && ((item.Price / orginalPrice) >= 1))
                            {
                                item.Price = amountPrice;
                            }
                            else
                            {
                                item.Price = item.Price;
                            }
                        }
                  

                        item.Amount = SLTradeCalculator.CalculateMoney(Convert.ToDouble(item.Price),
                            item.Quantity,
                            Convert.ToDouble(item.Mark));

                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), item.Amount,Convert.ToDecimal(item.CostOfFunds), item.CollateralFlag);
                    }
                    catch { }

                    item.TradeStatus = StatusDetail.HeldLocal;
                }

                if ((profitCenter != null) && !(profitCenter.Equals("")))
                {
                    item.ProfitId = profitCenter;
                    item.TradeStatus = StatusDetail.HeldLocal;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, pcMatrixList);
                    }
                }
            }


            return Json(list);
        }

        public JsonResult UpdateBulkTradesBreakOutEditor(string contraEntityId, string rate, string profitCenter, string mark, DateTime? termDate, string fee, decimal? feeOffSet, IEnumerable<SL_TradeBreakOutExtendedProjection> list)
        {
            var slTradeExtendedProjections = list as SL_TradeBreakOutExtendedProjection[] ?? list.ToArray();
            if (!slTradeExtendedProjections.Any()) return Json(list);

            var pcMatrixList = DataPCMatrix.LoadPCMatrix(list.Select(x => x.EntityId).Distinct().First());
            var contraEntity = (contraEntityId.Equals("") || (contraEntityId == null)) ? null : DataContraEntity.LoadContraEntity(list.Select(x => x.EntityId).First(), contraEntityId);

            var fundingRate = 0m;

            foreach (var item in slTradeExtendedProjections)
            {
                if (!string.IsNullOrWhiteSpace(fee))
                {
                    item.FeeType = fee;
                    item.FeeOffset = feeOffSet;
                    try
                    {
                        fundingRate = DataFundingRates.LoadFundingRatesByFee((DateTime)item.EffectiveDate, item.EntityId, fee).Fund;

                        item.RebateRate = (double)(fundingRate + (decimal)feeOffSet);
                    }
                    catch (Exception)
                    {                        
                        item.RebateRate = (double)feeOffSet;
                    }
                }

                if (!contraEntityId.Equals("") && (contraEntityId != null))
                {
                    item.ContraEntityId = contraEntityId;

                    var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                    item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (double.Parse(contraEntity.BorrowColl) / 100f) : (double.Parse(contraEntity.LoanColl) / 100f);
                    item.Price = Convert.ToDouble(price);

                    if (item.TradeType == TradeType.StockBorrow)
                    {
                        item.BorrowAmount = Convert.ToDecimal(item.Price) * item.BorrowQuantity;
                    }
                    else
                    {
                        item.LoanAmount = Convert.ToDecimal(item.Price) * item.LoanQuantity;
                    }

                    item.TradeStatus = StatusDetail.HeldLocal;
                }

                if ((rate != null) && !(rate.Equals("")))
                {
                    try
                    {
                        item.RebateRate = double.Parse(rate);


                        if (item.TradeType == TradeType.StockBorrow)
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), item.BorrowAmount, Convert.ToDecimal(item.CostOfFunds), item.CollateralFlag);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), item.LoanAmount, Convert.ToDecimal(item.CostOfFunds), item.CollateralFlag);
                        }

                       
                        item.TradeStatus = StatusDetail.HeldLocal;
                    }
                    catch { }
                }

                if (termDate != null)
                {
                    item.TermDate = termDate;
                    item.TradeStatus = StatusDetail.HeldLocal;
                }

                if ((mark != null) && !(mark.Equals("")))
                {
                    try
                    {
                        item.Mark = Convert.ToDouble(mark);

                        contraEntity = new SL_ContraEntity()
                        {
                            BorrowColl = mark.Equals("") ? "102" : mark.Replace(".", "").ToString(),
                            BorrowMarkCode = "%",
                            LoanColl = mark.Equals("") ? "102" : mark.Replace(".", "").ToString(),
                            LoanMarkCode = "%",
                            MarkRndHse = 0.9,
                            MarkRndInst = "U",
                            MarkValHse = 0.9,
                            MarkValInst = "U"
                        };

                        if ((bool.Parse(DataSystemValues.LoadSystemValue("UseSystemPriceChecking", "false"))) || (item.Price == 0))
                        {
                            var orginalPrice = Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice);
                            var amountPrice = Math.Ceiling(Convert.ToDouble(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice) * Convert.ToDouble(item.Mark));


                            if (((item.Price / orginalPrice) <= 1.20) && ((item.Price / orginalPrice) >= 1))
                            {
                                item.Price = amountPrice;
                            }
                            else
                            {
                                item.Price = item.Price;
                            }
                        }


                        if (item.TradeType == TradeType.StockBorrow)
                        {

                            item.BorrowAmount = SLTradeCalculator.CalculateMoney(Convert.ToDouble(item.Price),
                                item.BorrowQuantity,
                                Convert.ToDouble(item.Mark));

                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), item.BorrowAmount, Convert.ToDecimal(item.CostOfFunds), item.CollateralFlag);
                        }
                        else
                        {
                            item.LoanAmount = SLTradeCalculator.CalculateMoney(Convert.ToDouble(item.Price),
                             item.LoanQuantity,
                             Convert.ToDouble(item.Mark));

                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), Convert.ToDecimal(item.CostOfFunds), item.LoanAmount,item.CollateralFlag);
                        }

                    }
                    catch { }

                    item.TradeStatus = StatusDetail.HeldLocal;
                }

                if ((profitCenter != null) && !(profitCenter.Equals("")))
                {
                    item.ProfitId = profitCenter;
                    item.TradeStatus = StatusDetail.HeldLocal;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, pcMatrixList);
                    }
                }
            }


            return Json(list);
        }

        public JsonResult SaveBulkTradesEditor(IEnumerable<SL_TradeExtendedProjection> list)
        {
            var slTradeExtendedProjections = list as SL_TradeExtendedProjection[] ?? list.ToArray();
            if (!slTradeExtendedProjections.Any()) return Json(list);



            foreach (var newItem in slTradeExtendedProjections.Select(item => new SL_TradeExtendedProjection
            {
                TradeNumber = item.TradeNumber,
                EffectiveDate = DateTime.Today,
                EntityId = item.EntityId,
                IssueId = item.IssueId,
                SecurityNumber = item.SecurityNumber,
                Ticker = item.Ticker,
                Quantity = item.Quantity,
                Amount = item.Amount,
                ProfitId = item.ProfitId,
                CollateralFlag = item.CollateralFlag,
                RebateRate = item.RebateRate,
                RebateRateId = "",
                Mark = item.Mark,
                MarkParameterId = "%",
                Price = item.Price,
                TermDate = item.TermDate,
                SecuritySettleDate = item.SecuritySettleDate,
                TradeType = item.TradeType,
                CashSettleDate = item.CashSettleDate,
                FeeOffset = item.FeeOffset,
                FeeType = item.FeeType,
                ContraEntityId = item.ContraEntityId,
                CurrencyCode = item.CurrencyCode,
                ExecutingSystem = item.ExecutingSystem,
                PriceCalcType = item.PriceCalcType,
                AmountCalcType = item.AmountCalcType,
                DeliveryCode = item.DeliveryCode
            }))
            {
                try
                {
                    int.Parse(newItem.TradeNumber);
                }
                catch
                {
                    newItem.TradeNumber = null;
                }

                newItem.TradeStatus = StatusDetail.Pending;
                
                var success = DataTrades.AddTrade(newItem);                
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveBulkTradesBreakOutEditor(IEnumerable<SL_TradeBreakOutExtendedProjection> list)
        {
            var slTradeExtendedProjections = list as SL_TradeBreakOutExtendedProjection[] ?? list.ToArray();
            if (!slTradeExtendedProjections.Any()) return Json(list);



            foreach (var newItem in slTradeExtendedProjections.Select(item => new SL_TradeBreakOutExtendedProjection
            {
                TradeNumber = item.TradeNumber,
                EffectiveDate = DateTime.Today,
                EntityId = item.EntityId,
                IssueId = item.IssueId,
                SecurityNumber = item.SecurityNumber,
                Ticker = item.Ticker,
                BorrowQuantity = (item.TradeType == TradeType.StockBorrow) ? item.BorrowQuantity : 0,
                BorrowAmount = (item.TradeType == TradeType.StockBorrow) ? item.BorrowAmount : 0,
                LoanQuantity = (item.TradeType == TradeType.StockLoan) ? item.LoanQuantity : 0,
                LoanAmount = (item.TradeType == TradeType.StockLoan) ? item.LoanAmount : 0,
                ProfitId = item.ProfitId,
                CollateralFlag = item.CollateralFlag,
                RebateRate = item.RebateRate,
                RebateRateId = "",
                Mark = item.Mark,
                MarkParameterId = "%",
                Price = item.Price,
                TermDate = item.TermDate,
                SecuritySettleDate = item.SecuritySettleDate,
                TradeType = item.TradeType,
                CashSettleDate = item.CashSettleDate,
                FeeOffset = item.FeeOffset,
                FeeId = item.FeeId,
                CostOfFunds = item.CostOfFunds,
                Callable = item.Callable,
                ClearingId = item.ClearingId,
                ExpectedEndDate = item.ExpectedEndDate,
                FeeType = item.FeeType,
                ContraEntityId = item.ContraEntityId,
                CurrencyCode = item.CurrencyCode,
                ExecutingSystem = item.ExecutingSystem,
                DeliveryCode =item.DeliveryCode,
                PriceCalcType = item.PriceCalcType,
                AmountCalcType = item.AmountCalcType,
                DividendRate = item.DividendRate,
                TradeDate = item.TradeDate,
                SecurityLoc = item.SecurityLoc,
                CashLoc = item.CashLoc,
                DividendCallable = item.DividendCallable,                             
            }))
            {
                try
                {
                    int.Parse(newItem.TradeNumber);
                }
                catch
                {
                    newItem.TradeNumber = null;
                }

                newItem.TradeStatus = StatusDetail.Pending;

                var success = DataTrades.AddTrade(newItem);
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        private ScratchPadLookupEnum GetLookupType(List<string> source)
        {
            var lookupType = ScratchPadLookupEnum.UseSpecificInventory;

            if (source.Contains(LabelHelper.Text("UseAll")))
            {
                lookupType = ScratchPadLookupEnum.UseAllInventory;
            }
            else if (source.Contains(LabelHelper.Text("UseBox")))
            {
                lookupType = ScratchPadLookupEnum.UseBox;
            }

            return lookupType;
        }

        public JsonResult UpdateTradesScratchPad(
            string contraEntityId,
            TradeType? tradeType,
            string rate,
            string profitCenter,
            string mark,
            string feeType,
            decimal? feeOffSet,
            Currency? currency,
            DateTime? tradeDate,
            DateTime? cashSettleDate,
            DateTime? securitySettleDate,
            DateTime? expectedEndDate,
            DateTime? termDate,
            SL_ExecutionSystemType? executingSystemType, 
            SL_DeliveryCode? deliveryCode,
            SL_CollateralFlag? collateralFlag,
            ScratchPadIntraDayEnum intraDayEnum,
            PushPadAvailLookupEnum scratchpadAvailLookup,
            List<string> source,
            SL_TradePriceAmountCalcType priceAmountCalcType,
            SL_TradeAmountCalcType tradeAmountCalcType,
            
            IEnumerable<TradeChangeModel> list,
            DateTime effectiveDate)
        {
            var tradeChangeModels = list as TradeChangeModel[] ?? list.ToArray();
            if (!tradeChangeModels.Any()) return Json(list);

            var pcMatrixList = DataPCMatrix.LoadPCMatrix(list.Select(x => x.EntityId).Distinct().First());
            var contraEntity = (contraEntityId.Equals("") || (contraEntityId == null)) ? null :  DataContraEntity.LoadContraEntity(list.Select(x => x.EntityId).First(), contraEntityId);

            var lookupType = GetLookupType(source);
            var boxCalcList = new List<SL_BoxCalculationExtendedProjection>();
            var inventoryList = new List<SL_Inventory>();
            var issuePriceList = new List<IssuePrice>();
            var intraDayLendingList = new List<SL_IntradayLending>();

            var feeTypes = DataFeeTypes.LoadFeeTypes(list.Select(x => x.EntityId).First());
            var fundingRates = DataFunding.LoadFundingRates(DateTime.Today, (list.Select(x => x.EntityId).First()));
            var fundingRateTrade = 0m;

            if (lookupType == ScratchPadLookupEnum.UseBox)
            {
                if (effectiveDate == DateTime.Today)
                {
                    boxCalcList = StaticDataCache.BoxCalculationStaticGet(DateTime.Today, list.Select(x => x.EntityId).Distinct().First());
                }
                else
                {
                    boxCalcList = DataBoxCalculation.LoadBoxCalculation(effectiveDate, list.Select(x => x.EntityId).Distinct().First(), 0, 100, false, false, false, false);
                }
            }
            else
            {
                inventoryList = DataInventory.LoadInventoryByIssueIdList(
                                effectiveDate,
                                list.Select(x => x.IssueId).ToList());
            }

            intraDayLendingList = DataInventory.LoadIntraDayLendingByIssueList(
               effectiveDate,
                list.Select(x => x.IssueId).ToList());

            issuePriceList = DataIssue.LoadIssuePricesFromIssueList(list.Select(x => x.IssueId).ToList());

            if (!string.IsNullOrWhiteSpace(feeType))
            {
                try
                {
                    var fundingRate = DataFundingRates.LoadFundingRatesByFee(DateTime.Today, list.Select(x => x.EntityId).First(), feeType);

                    fundingRateTrade = fundingRate.Fund + (decimal)feeOffSet;
                }
                catch
                {
                    fundingRateTrade = (decimal)feeOffSet;
                }
            }


            foreach (var item in tradeChangeModels.Where(item => item.Enabled))
            {
                if (!contraEntityId.Equals("") && (contraEntityId != null))
                {
                    item.ContraEntityId = contraEntityId;

                    var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                    item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (decimal.Parse(contraEntity.BorrowColl) / 100m) : (decimal.Parse(contraEntity.LoanColl) / 100m);
                    item.Price = price;
                    item.Amount = item.Price * item.Quantity;
                }

                if (tradeType != null)
                {
                    item.TradeType = (TradeType) tradeType;
                }

                if (deliveryCode != null)
                {
                    item.DeliveryCode = (SL_DeliveryCode) deliveryCode;
                }

                if (executingSystemType != null)
                {
                    item.ExecutingType = (SL_ExecutionSystemType) executingSystemType;
                }

                if (collateralFlag != null)
                {
                    item.CollateralFlag = (SL_CollateralFlag) collateralFlag;
                }

                item.TradePriceAmountCalcType = priceAmountCalcType;
                item.TradeAmountCalcType = tradeAmountCalcType;          

                if ((rate != null) && !(rate.Equals("")))
                {
                    try
                    {
                        item.RebateRate = decimal.Parse(rate);


                        if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.Amount),
                                0,
                                item.CollateralFlag);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.Amount),
                              0,
                              item.CollateralFlag);
                        }
                    }
                    catch { }
                }


                if (string.IsNullOrWhiteSpace(feeType))
                {
                    item.RebateRate = rate != null ? Convert.ToDecimal(rate) : Convert.ToDecimal(item.RebateRate);
                }
                else
                {
                    item.FeeType = feeType;
                    item.RebateRate = fundingRateTrade;
                }

                if ((mark != null) && !(mark.Equals("")))
                {
                    try
                    {
                        item.Mark = Convert.ToDecimal(mark);
                        item.Price = Math.Ceiling(item.MktPrice * Convert.ToDecimal(mark));

                        item.Amount = SLTradeCalculator.CalculateMoney(Convert.ToDouble(item.MktPrice),
                            item.Quantity,
                            Convert.ToDouble(item.Mark));

                        if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.Amount),
                                0,
                                item.CollateralFlag);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.Amount),
                              0,
                              item.CollateralFlag);
                        }                        
                    }
                    catch { }
                }

                if ((profitCenter != null) && !(profitCenter.Equals("")))
                {
                    item.ProfitId = profitCenter;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, pcMatrixList);
                    }
                }

                if (currency != null)
                {
                    item.CurrencyCode = (Currency)currency;
                }

                if (termDate != null)
                {
                    item.TermDate = (DateTime)termDate;
                }


                if (securitySettleDate != null)
                {
                    item.SecuritySettleDate = (DateTime)securitySettleDate;
                }

                if (cashSettleDate != null)
                {
                    item.CashSettleDate = (DateTime)cashSettleDate;
                }

                if (tradeDate != null)
                {
                    item.TradeDate = (DateTime)tradeDate;
                }

                if (expectedEndDate != null)
                {
                    item.ExpectedEndDate = (DateTime)expectedEndDate;
                }

                ListParsingService.GenerateInventoryInformation(effectiveDate, boxCalcList, inventoryList, item, source, lookupType);

                try
                {
                    GenerateRules.Process(item.EntityId, item);

                    item.RuleAlert = "";
                }
                catch (AlertException alertEx)
                {
                    var _activity = GenerateRules.GenerateFailActivity(item, alertEx.Message);

                    try
                    {
                        DataActivity.AddActivity(_activity);
                        item.RuleAlert = alertEx.Message;
                    }
                    catch
                    {

                    }
                }
                catch (ActivityException activityEx)
                {
                    var _activity = GenerateRules.GenerateFailActivity(item, activityEx.Message);

                    try
                    {
                        DataActivity.AddActivity(_activity);
                        item.RuleAlert = activityEx.Message;

                    }
                    catch
                    {

                    }
                }

            }

            return Json(list);
        }


        public JsonResult UpdateTradesBreakOutScratchPad(string contraEntityId, TradeType? tradeType, string rate, string profitCenter, string mark, Currency? currency, decimal? priceRoundTo, DateTime? effectiveDate, SL_ExecutionSystemType? executingSystemType,SL_DeliveryCode? deliveryCode, List<string> source, IEnumerable<TradeBreakOutChangeModel> list)
        {
            var tradeChangeModels = list as TradeBreakOutChangeModel[] ?? list.ToArray();
            if (!tradeChangeModels.Any()) return Json(list);

            var pcMatrixList = DataPCMatrix.LoadPCMatrix(list.Select(x => x.EntityId).Distinct().First());
            var contraEntity = (contraEntityId.Equals("") || (contraEntityId == null)) ? null : DataContraEntity.LoadContraEntity(list.Select(x => x.EntityId).First(), contraEntityId);

            var lookupType = GetLookupType(source);
            var boxCalcList = new List<SL_BoxCalculationExtendedProjection>();
            var inventoryList = new List<SL_Inventory>();
            var issuePriceList = new List<IssuePrice>();
            var intraDayLendingList = new List<SL_IntradayLending>();


            if (lookupType == ScratchPadLookupEnum.UseBox)
            {
                boxCalcList = StaticDataCache.BoxCalculationStaticGet(DateTime.Today, list.Select(x => x.EntityId).Distinct().First());
            }
            else
            {
                inventoryList = DataInventory.LoadInventoryByIssueIdList(
                                DateTime.Today,
                                list.Select(x => x.IssueId).ToList());
            }

            intraDayLendingList = DataInventory.LoadIntraDayLendingByIssueList(
                DateTime.Today,
                list.Select(x => x.IssueId).ToList());

            issuePriceList = DataIssue.LoadIssuePricesFromIssueList(list.Select(x => x.IssueId).ToList());

            foreach (var item in tradeChangeModels.Where(item => item.Enabled))
            {
                if (!contraEntityId.Equals("") && (contraEntityId != null))
                {
                    item.ContraEntityId = contraEntityId;

                    var price = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal((DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice ?? 0)), contraEntity);

                    item.Mark = (item.TradeType.Equals(TradeType.StockBorrow)) ? (decimal.Parse(contraEntity.BorrowColl) / 100m) : (decimal.Parse(contraEntity.LoanColl) / 100m);
                    item.Price = price;

                    if (item.TradeType == TradeType.StockBorrow)
                    {
                        item.BorrowAmount = item.Price * item.BorrowQuantity;
                    }
                    else
                    {
                        item.LoanAmount = item.Price * item.LoanQuantity;
                    }
                }

                if (tradeType != null)
                {
                    item.TradeType = (TradeType)tradeType;
                }

                if (deliveryCode != null)
                {
                    item.DeliveryCode = (SL_DeliveryCode)deliveryCode;
                }
                
                if (priceRoundTo != null)
                {
                    item.Price = Math.Ceiling(item.MktPrice / (decimal)priceRoundTo) * (decimal)priceRoundTo;
                }

                if (executingSystemType != null)
                {
                    item.ExecutingType = (SL_ExecutionSystemType)executingSystemType;
                }

                if ((rate != null) && !(rate.Equals("")))
                {
                    try
                    {
                        item.RebateRate = decimal.Parse(rate);


                        if (item.TradeType == TradeType.StockBorrow)
                        {
                            if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                    item.TradeType,
                                    SLTradeCalculator.Locale.Domestic,
                                    Convert.ToDecimal(item.RebateRate),
                                    Convert.ToDecimal(item.BorrowAmount),
                                    0,
                                    SL_CollateralFlag.C);
                            }
                            else
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                  item.TradeType,
                                  SLTradeCalculator.Locale.International,
                                  Convert.ToDecimal(item.RebateRate),
                                  Convert.ToDecimal(item.BorrowAmount),
                                  0,
                                  SL_CollateralFlag.C);
                            }

                            //item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), item.BorrowAmount, 0,SL_CollateralFlag.C);
                        }
                        else
                        {
                            if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                    item.TradeType,
                                    SLTradeCalculator.Locale.Domestic,
                                    Convert.ToDecimal(item.RebateRate),
                                    Convert.ToDecimal(item.LoanAmount),
                                    0,
                                    SL_CollateralFlag.C);
                            }
                            else
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                  item.TradeType,
                                  SLTradeCalculator.Locale.International,
                                  Convert.ToDecimal(item.RebateRate),
                                  Convert.ToDecimal(item.LoanAmount),
                                  0,
                                  SL_CollateralFlag.C);
                            }

                            //item.IncomeAmount = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, Convert.ToDecimal(item.RebateRate), item.LoanAmount,0, SL_CollateralFlag.C);
                        }                        
                    }
                    catch { }
                }

                if ((mark != null) && !(mark.Equals("")))
                {
                    try
                    {

                        item.Mark = Convert.ToDecimal(mark);
                        item.Price = Math.Ceiling(item.MktPrice * Convert.ToDecimal(mark));


                        if (item.TradeType == TradeType.StockBorrow)
                        {
                            item.BorrowAmount = SLTradeCalculator.CalculateMoney(Convert.ToDouble(item.MktPrice),
                                item.BorrowQuantity,
                                Convert.ToDouble(item.Mark));

                            if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                    item.TradeType,
                                    SLTradeCalculator.Locale.Domestic,
                                    Convert.ToDecimal(item.RebateRate),
                                    Convert.ToDecimal(item.BorrowAmount),
                                    0,
                                    SL_CollateralFlag.C);
                            }
                            else
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                  item.TradeType,
                                  SLTradeCalculator.Locale.International,
                                  Convert.ToDecimal(item.RebateRate),
                                  Convert.ToDecimal(item.BorrowAmount),
                                  0,
                                  SL_CollateralFlag.C);
                            }
                        }
                        else
                        {
                            item.LoanAmount = SLTradeCalculator.CalculateMoney(Convert.ToDouble(item.MktPrice),
                                item.LoanQuantity,
                                Convert.ToDouble(item.Mark));

                            if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                    item.TradeType,
                                    SLTradeCalculator.Locale.Domestic,
                                    Convert.ToDecimal(item.RebateRate),
                                    Convert.ToDecimal(item.LoanAmount),
                                    0,
                                    SL_CollateralFlag.C);
                            }
                            else
                            {
                                item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                  item.TradeType,
                                  SLTradeCalculator.Locale.International,
                                  Convert.ToDecimal(item.RebateRate),
                                  Convert.ToDecimal(item.LoanAmount),
                                  0,
                                  SL_CollateralFlag.C);
                            }
                            
                        }

            
                    }
                    catch { }
                }

                if ((profitCenter != null) && !(profitCenter.Equals("")))
                {
                    item.ProfitId = profitCenter;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.ProfitId))
                    {
                        item.ProfitId = PcMatrixService.SetDefaults(item, pcMatrixList);
                    }
                }

                if (currency != null)
                {
                    item.CurrencyCode = (Currency)currency;
                }



                if (effectiveDate != null)
                {
                    item.SettlementDate = (DateTime)effectiveDate;
                }

                ListParsingService.GenerateInventoryInformation(boxCalcList, inventoryList, item, source, lookupType);
            }

            return Json(list);
        }


        public JsonResult LoadList(string entityId,
                            string list,
                            TradeType? tradeType,
                            double? rebateRate,
                            double? mark,
                            Currency? currency,
                            string profitCenter,
                            string contraEntityId,
                            string feeType,
                            decimal? feeOffSet,
                            DateTime? tradeDate,
                            DateTime? cashSettleDate,
                            DateTime? securitySettleDate,
                            DateTime? termDate,
                            DateTime? expectedEndDate,
                            DateTime effectiveDate,
                            bool allowOddLot,
                            bool useCustomFormat,
                            SL_TradeAmountCalcType tradeAmountCalcType,
                            SL_TradePriceAmountCalcType tradePriceAmountCalcType,
                            ScratchPadIntraDayEnum? intraDayEnum,
                            SL_DeliveryCode? deliveryCode,
                            SL_CollateralFlag collateralFlag,
                            SL_ExecutionSystemType executingSystemType,
                            PushPadAvailLookupEnum scratchpadAvailLookup,
                            List<string>  source)
        {
            var listFormat = DataSystemValues.LoadSystemValue("TradeListParseFormat", "QUANTITY;TICKER;RATE;");
         
    
            List<ParsedListModel> parsedList = new List<ParsedListModel>();

            if (!useCustomFormat)
            {
                parsedList = ListParsingService.GenerateList(entityId, list, listFormat);

                contraEntityId = string.IsNullOrWhiteSpace(contraEntityId) ? "" : contraEntityId.ToUpper();
            }
            else
            {
                //Contra [0]
                //Cusip  [1]
                //Qty    [2]
                //Amt    [3]
                //Rate   [4]
                //PC     [5]

                foreach (var line in  list.Split(new char[] { '\n' }))
                {
                    ParsedListModel item = new ParsedListModel();

                    var headers = listFormat.Split(new char[] { ';' }).ToList();

                    var itemArray = line.Split(new char[] { ' ', '\t', ',' });

                    item.EntityId = entityId;

                    try
                    {
                        item.ContraEntityId = itemArray[FindInex("contra", headers)].ToString();
                    }
                    catch
                    {

                    }

                    try
                    {
                        var cusipNumer = itemArray[FindInex("cusip", headers)].ToString();

                        var issue = DataIssue.LoadIssue(cusipNumer);

                        if (item.IssueId != -1)
                        {
                            item.IssueId = issue.IssueId;
                            item.SecurityNumber = issue.Cusip;
                            item.Ticker = issue.Ticker;
                            item.Isin = issue.ISIN;
                            item.Sedol = issue.SEDOL;
                            item.SecNumber = issue.SecNumber;
                       }
                        else
                        {
                            item.SecurityNumber = cusipNumer;
                        }
                        
                        item.IssueId = issue.IssueId;
                    }
                    catch
                    {

                    }

                    try
                    {
                        item.Quantity = decimal.Parse(itemArray[FindInex("qty", headers)].ToString());
                    }
                    catch
                    {

                    }

                    try
                    {
                        item.Amount = decimal.Parse(itemArray[FindInex("amt", headers)].ToString().Replace("$", ""));
                    }
                    catch
                    {

                    }

                    try
                    {
                        item.RebateRate = decimal.Parse(itemArray[FindInex("rate", headers)].ToString());
                    }
                    catch
                    {

                    }

                    try
                    {
                        item.ProfitId = itemArray[FindInex("pc", headers)].ToString();
                    }
                    catch
                    {

                    }

                    parsedList.Add(item);               
                }
            }
  

            
            
            
           
           var _tradeParseList = ListParsingService.GenerateTrades(
                parsedList, 
                tradeType, 
                rebateRate, 
                mark,
                (currency == null) ? Currency.USDollars : (Currency)currency, 
                profitCenter,                 
                contraEntityId,
                feeType,
                feeOffSet,
                tradeDate,
                cashSettleDate,
                securitySettleDate,
                termDate,
                expectedEndDate,
                tradeAmountCalcType,
                tradePriceAmountCalcType,
                deliveryCode,
                collateralFlag,
                intraDayEnum,
                source,
                executingSystemType,
                GetLookupType(source),
                scratchpadAvailLookup,
                allowOddLot,
                effectiveDate);

            var exceedPrepay = false;

            foreach (var item in _tradeParseList)
            {
                if (item.ExecutingType == SL_ExecutionSystemType.GLOBALONE)
                {
                    if (!string.IsNullOrWhiteSpace(item.ContraEntityId))
                    {
                         exceedPrepay = DataContraEntity.ExceededPrepayLimit(item.EntityId, item.ContraEntityId, item.Amount, item.CurrencyCode, item.SecuritySettleDate);

                        if (exceedPrepay)
                        {
                            item.RuleAlert = "Excceeded Prepay Limit";                           
                        }
                    }
                }
            }

            return Extended.JsonMax(_tradeParseList, JsonRequestBehavior.AllowGet);
        }

        private int FindInex(string title, List<string> headerList)
        {
            int index = 0;

            for(int counter = 0; counter < headerList.Count(); counter++)
            {
                if (headerList[counter].ToLower().Equals(title.ToLower()))
                {
                    index = counter;
                }
            }

            return index;
        }


        /****Not Used******/
        public JsonResult LoadListBreakOut(string entityId,
                     string list,
                     TradeType? tradeType,
                     double? rebateRate,
                     double? mark,
                     Currency? currency,
                     decimal? priceRoundTo,
                     string profitCenter,
                     string contraEntityId,
                     string feeType,
                     decimal? feeOffSet,
                     SL_DeliveryCode? deliveryCode,
                     DateTime? termDate,
                     SL_ExecutionSystemType executingSystemType,
                     List<string> source)
        {

            var listFormat = DataSystemValues.LoadSystemValue("TradeListParseFormat", "QUANTITY;TICKER;RATE;");

            var parsedList =  ListParsingService.GenerateList(entityId, list, listFormat);

            contraEntityId = string.IsNullOrWhiteSpace( contraEntityId ) ? "" : contraEntityId.ToUpper();

            var _tradeBreakOutParseList = ListParsingService.GenerateTradesBreakOut(
                parsedList, 
                tradeType, 
                rebateRate, 
                mark, 
                currency, 
                profitCenter, 
                priceRoundTo, 
                contraEntityId,
                feeType,
                feeOffSet,
                termDate,
                deliveryCode,
                source, 
                GetLookupType(source));

            foreach (var item in _tradeBreakOutParseList)
            {
                item.ExecutingType = executingSystemType;
            }

            return Json(_tradeBreakOutParseList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveTrades(IEnumerable<TradeChangeModel> list)
        {
            var tradeChangeModels = list as TradeChangeModel[] ?? list.ToArray();
            var executingSystemTypes = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeType();

            if (list == null) return Json(tradeChangeModels.Where(x => x.SubmissionType != StatusDetail.Approved));

            foreach (var item in tradeChangeModels.Where(item => item.Enabled))
            {
                try
                {
                    CountryMapping countryCodeTrade1 = DataIssue.LoadCountryMapping(executingSystemTypes.Where(x => x.TradeType == item.TradeType && x.ExecutionSystemType == item.ExecutingType).Select(x => x.DefaultSecuritySettleLocation).First());

                    string countryCodeTrade1Override = executingSystemTypes.Where(x => x.TradeType == item.TradeType && x.ExecutionSystemType == item.ExecutingType).Select(x => x.DefaultSecuritySettleLocationOverride).First();                    

                    var trade = new SL_TradeExtendedProjection();

                    var issue = DataIssue.LoadIssue(item.SecurityNumber);

                    SL_ContraEntity contraEntity = null;

                    if (!string.IsNullOrWhiteSpace(item.ContraEntityId))
                    {
                        contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);
                    }
                    trade.EffectiveDate = DateTime.Today;
                    trade.EntityId = item.EntityId;
                    trade.IssueId = item.IssueId;
                    trade.SecurityNumber = item.SecurityNumber;
                    trade.Ticker = item.Ticker;
                    trade.Quantity = item.Quantity;
                    trade.Amount = item.Amount;
                    trade.TradeType = item.TradeType;
                    trade.ProfitId = item.ProfitId;
                    trade.CollateralFlag = item.CollateralFlag;
                    trade.RebateRate = Convert.ToDouble(item.RebateRate);
                    trade.RebateRateId = "";
                    trade.Mark = Convert.ToDouble(item.Mark);
                    trade.MarkParameterId = "%";
                    trade.Price = Convert.ToDouble(item.Price);
                    trade.SecuritySettleDate = item.SecuritySettleDate;
                    trade.CashSettleDate = item.CashSettleDate;
                    trade.TradeDate = item.TradeDate;
                    trade.ExpectedEndDate = item.ExpectedEndDate;
                    trade.TermDate = item.TermDate;
                    trade.BatchCode = string.IsNullOrWhiteSpace(item.BatchCode) ? "" : item.BatchCode.Trim();
                    trade.Comment = string.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment.Trim();
                    trade.CurrencyCode = executingSystemTypes.Where(x => x.TradeType == item.TradeType && x.ExecutionSystemType == item.ExecutingType).Select(x => x.DefaultCurrency).First();
                    trade.SecurityLoc = (string.IsNullOrWhiteSpace(countryCodeTrade1Override)) ? countryCodeTrade1.CountryCode.ToString() : countryCodeTrade1Override;
                    trade.CashLoc = (string.IsNullOrWhiteSpace(countryCodeTrade1Override)) ? countryCodeTrade1.CountryCode.ToString() : countryCodeTrade1Override;

                    trade.PriceCalcType = item.TradePriceAmountCalcType;
                    trade.AmountCalcType = item.TradeAmountCalcType;


                    trade.ContraEntityId = item.ContraEntityId;
                    trade.ExecutingSystem = item.ExecutingType;
                    trade.FeeType = string.IsNullOrWhiteSpace(item.FeeType) ? "" : item.FeeType;
                    trade.FeeOffset = item.FeeOffSet;
                    trade.DeliveryCode = item.DeliveryCode;

                    if ((item.Quantity % 100) != 0)
                    {
                        trade = SLTradeCalculator.GenerateDefaults(trade, issue, contraEntity, true, false);
                    }
                    else
                    {
                        trade = SLTradeCalculator.GenerateDefaults(trade, issue, contraEntity, true, bool.Parse(DataSystemValues.LoadSystemValue("Round" + trade.ExecutingSystem, true.ToString())));
                    }

                    DataTrades.AddTrade(trade);

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

        public JsonResult SaveTradesBreakOut(IEnumerable<TradeBreakOutChangeModel> list)
        {
            var tradeChangeModels = list as TradeBreakOutChangeModel[] ?? list.ToArray();

            if (list == null) return Json(tradeChangeModels.Where(x => x.SubmissionType != StatusDetail.Approved));

            foreach (var item in tradeChangeModels.Where(item => item.Enabled))
            {
                try
                {
                    var trade = new SL_TradeExtendedProjection();

                    var issue = DataIssue.LoadIssue(item.SecurityNumber);

                    SL_ContraEntity contraEntity = null;

                    if (!string.IsNullOrWhiteSpace(item.ContraEntityId))
                    {
                        contraEntity = DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);
                    }

                    trade.EffectiveDate = DateTime.Today;
                    trade.EntityId = item.EntityId;
                    trade.IssueId = item.IssueId;
                    trade.SecurityNumber = item.SecurityNumber;
                    trade.Ticker = item.Ticker;
                    trade.Quantity = (item.TradeType == TradeType.StockBorrow) ? item.BorrowQuantity :  item.LoanQuantity;
                    trade.Amount = (item.TradeType == TradeType.StockBorrow) ? item.BorrowAmount : item.LoanAmount;
                    trade.ProfitId = item.ProfitId;
                    trade.CollateralFlag = SL_CollateralFlag.C;
                    trade.RebateRate = Convert.ToDouble(item.RebateRate);
                    trade.RebateRateId = "";
                    trade.Mark = Convert.ToDouble(item.Mark);
                    trade.MarkParameterId = "%";
                    trade.Price = Convert.ToDouble(item.Price);
                    trade.SecuritySettleDate = item.SettlementDate;
                    trade.TradeType = item.TradeType;
                    trade.CashSettleDate = item.SettlementDate;
                    trade.ContraEntityId = item.ContraEntityId;
                    trade.ExecutingSystem = item.ExecutingType;
                    trade.FeeType = string.IsNullOrWhiteSpace(item.FeeType) ? "" : item.FeeType;
                    trade.FeeOffset = item.FeeOffSet;
                    trade.TermDate = item.TermDate;
                    trade.DeliveryCode = item.DeliveryCode;
                    trade.Comment = string.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment.Trim();

                    trade = SLTradeCalculator.GenerateDefaults(trade, issue, contraEntity, true, bool.Parse(DataSystemValues.LoadSystemValue("Round" + trade.ExecutingSystem, true.ToString())));

                    DataTrades.AddTrade(trade);

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

        public JsonResult ConfirmTrades(IEnumerable<TradeChangeModel> list)
        {
            var tradeChangeModels = list as TradeChangeModel[] ?? list.ToArray();

            if (list == null) return Json(tradeChangeModels.Where(x => x.SubmissionType != StatusDetail.Approved));

            var issueIdList = DataIssue.LoadIssueListById(list.Select(x => x.IssueId).Distinct().ToList());

            var entityId = list.Select(x => x.EntityId).Distinct().First();

            var contraEntityList = DataContraEntity.LoadContraEntityByEntityId(entityId);

            var tradeNumberList = new List<string>();

            var executingSystemTypes = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeType();


            foreach (var item in tradeChangeModels.Where(item => item.Enabled))
            {
                try
                {
                    CountryMapping countryCodeTrade1 = DataIssue.LoadCountryMapping(executingSystemTypes.Where(x => x.TradeType == item.TradeType && x.ExecutionSystemType == item.ExecutingType).Select(x => x.DefaultSecuritySettleLocation).First());

                    string countryCodeTrade1Override = executingSystemTypes.Where(x => x.TradeType == item.TradeType && x.ExecutionSystemType == item.ExecutingType).Select(x => x.DefaultSecuritySettleLocationOverride).First();

                    var trade = new SL_TradeExtendedProjection();

                    var issue = issueIdList.Where(x => x.IssueId == item.IssueId).First();

                    SL_ContraEntity contraEntity = null;

                    if (!string.IsNullOrWhiteSpace(item.ContraEntityId))
                    {
                        contraEntity = contraEntityList.Where(x => x.ContraEntity == item.ContraEntityId).First();
                    }

                    trade.EffectiveDate = DateTime.Today;
                    trade.EntityId = item.EntityId;
                    trade.IssueId = item.IssueId;
                    trade.SecurityNumber = item.SecurityNumber;
                    trade.Ticker = item.Ticker;
                    trade.Quantity = item.Quantity;
                    trade.Amount = item.Amount;
                    trade.TradeType = item.TradeType;
                    trade.ProfitId = item.ProfitId;
                    trade.CollateralFlag = item.CollateralFlag;
                    trade.RebateRate = Convert.ToDouble(item.RebateRate);
                    trade.RebateRateId = "";
                    trade.Mark = Convert.ToDouble(item.Mark);
                    trade.MarkParameterId = "%";
                    trade.Price = Convert.ToDouble(item.Price);
                    trade.SecuritySettleDate = item.SecuritySettleDate;                   
                    trade.CashSettleDate = item.CashSettleDate;
                    trade.TradeDate = item.TradeDate;
                    trade.ExpectedEndDate = item.ExpectedEndDate;
                    trade.TermDate = item.TermDate;
                    trade.PriceCalcType = item.TradePriceAmountCalcType;
                    trade.AmountCalcType = item.TradeAmountCalcType;
                    trade.BatchCode = string.IsNullOrWhiteSpace(item.BatchCode) ? "" : item.BatchCode.Trim();
                    trade.CurrencyCode = executingSystemTypes.Where(x => x.TradeType == item.TradeType && x.ExecutionSystemType == item.ExecutingType).Select(x => x.DefaultCurrency).First();
                    trade.SecurityLoc = (string.IsNullOrWhiteSpace(countryCodeTrade1Override)) ? countryCodeTrade1.CountryCode.ToString() : countryCodeTrade1Override;
                    trade.CashLoc = (string.IsNullOrWhiteSpace(countryCodeTrade1Override)) ? countryCodeTrade1.CountryCode.ToString() : countryCodeTrade1Override;

                    trade.ContraEntityId = item.ContraEntityId;
                    trade.ExecutingSystem = item.ExecutingType;
                    trade.FeeType = string.IsNullOrWhiteSpace(item.FeeType) ? "" : item.FeeType;
                    trade.FeeOffset = item.FeeOffSet;
                   
                    
                    trade.DeliveryCode = item.DeliveryCode;

                    if ((item.Quantity % 100) != 0)
                    {
                        trade = SLTradeCalculator.GenerateDefaults(trade, issue, contraEntity, true, false);
                    }
                    else
                    {
                        trade = SLTradeCalculator.GenerateDefaults(trade, issue, contraEntity, true, bool.Parse(DataSystemValues.LoadSystemValue("Round" + trade.ExecutingSystem, true.ToString())));
                    }                    

                    DataTrades.AddTrade(trade);
                    item.ContractNumber = trade.TradeNumber;

                    tradeNumberList.Add(trade.TradeNumber);
                }
                catch (Exception e)
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                }
            }

            var tradeList = DataTrades.LoadTradesExtended(DateTime.Today, entityId);

            foreach (var item in tradeNumberList)
            {
                var tradeExtended = tradeList.Where(x => x.TradeNumber == item).First();
                var tradeModelExtended = tradeChangeModels.Where(x => x.ContractNumber == item).First();

                if (!String.IsNullOrWhiteSpace(item))
                {
                    try
                    {

                        DataExternalOperations.AddTradeToContract(tradeExtended, true);
                        tradeModelExtended.SubmissionType = StatusDetail.Approved;
                    }
                    catch
                    {
                        tradeModelExtended.MemoInfo = "Trade saved, error confirming.";
                        tradeModelExtended.SubmissionType = StatusDetail.Rejected;
                    }
                }

            }        
            

            return Json(tradeChangeModels.Where(x => x.SubmissionType != StatusDetail.Approved));
        }

        public JsonResult ConfirmTradesBreakOut( IEnumerable<TradeBreakOutChangeModel> list )
        {
            var tradeChangeModels = list as TradeBreakOutChangeModel[] ?? list.ToArray();
            List<SL_TradeExtendedProjection> tradeExtendedList = new List<SL_TradeExtendedProjection>();

            if ( list == null ) return Json( tradeChangeModels.Where( x => x.SubmissionType != StatusDetail.Approved ) );

            var issueIdList = DataIssue.LoadIssueListById( list.Select( x => x.IssueId ).Distinct().ToList() );

            var entityId = list.Select( x => x.EntityId ).Distinct().First();

            var contraEntityList = DataContraEntity.LoadContraEntityByEntityId( entityId );

            foreach ( var item in tradeChangeModels.Where( item => item.Enabled ) )
            {
                try
                {
                    var trade = new SL_TradeExtendedProjection();

                    var issue = issueIdList.Where( x => x.IssueId == item.IssueId ).First();

                    SL_ContraEntity contraEntity = null;

                    if ( !string.IsNullOrWhiteSpace( item.ContraEntityId ) )
                    {
                        contraEntity = contraEntityList.Where( x => x.ContraEntity == item.ContraEntityId ).First();
                    }

                    trade.EffectiveDate = DateTime.Today;
                    trade.EntityId = item.EntityId;
                    trade.IssueId = item.IssueId;
                    trade.SecurityNumber = item.SecurityNumber;
                    trade.Ticker = item.Ticker;
                    trade.Quantity = ( item.TradeType == TradeType.StockBorrow ) ? item.BorrowQuantity : item.LoanQuantity;
                    trade.Amount = ( item.TradeType == TradeType.StockBorrow ) ? item.BorrowAmount : item.LoanAmount;
                    trade.ProfitId = item.ProfitId;
                    trade.CollateralFlag =  SL_CollateralFlag.C;
                    trade.RebateRate = Convert.ToDouble( item.RebateRate );
                    trade.RebateRateId = "";
                    trade.Mark = Convert.ToDouble( item.Mark );
                    trade.MarkParameterId = "%";
                    trade.Price = Convert.ToDouble( item.Price );
                    trade.SecuritySettleDate = item.SettlementDate;
                    trade.TradeType = item.TradeType;
                    trade.CashSettleDate = item.SettlementDate;
                    trade.ContraEntityId = item.ContraEntityId;
                    trade.ExecutingSystem = item.ExecutingType;
                    trade.TradeStatus = StatusDetail.Pending;
                    trade.FeeType = string.IsNullOrWhiteSpace(item.FeeType) ? "" : item.FeeType;
                    trade.FeeOffset = item.FeeOffSet;
                    trade.TermDate = item.TermDate;
                    trade.DeliveryCode = item.DeliveryCode;


                    trade = SLTradeCalculator.GenerateDefaults(trade, issue, contraEntity, true, bool.Parse(DataSystemValues.LoadSystemValue("Round" + trade.ExecutingSystem, true.ToString())));

                    DataTrades.AddTrade( trade );

                    tradeExtendedList.Add( trade );

                    item.ContractNumber = trade.TradeNumber;
                }
                catch
                {

                }
            }


            foreach ( var item in tradeExtendedList )
            {
                var tradeBrreakOutChangeModel = tradeChangeModels.Where( x => x.ContractNumber == item.TradeNumber ).First();

                try
                {
                    if ( !String.IsNullOrWhiteSpace( item.TradeNumber ) )
                    {
                        try
                        {
                            DataExternalOperations.AddTradeToContract( item, true );
                            tradeBrreakOutChangeModel.SubmissionType = StatusDetail.Approved;
                        }
                        catch
                        {
                            tradeBrreakOutChangeModel.MemoInfo = "Trade saved, error confirming.";
                            tradeBrreakOutChangeModel.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
                catch ( Exception e )
                {
                    tradeBrreakOutChangeModel.MemoInfo = e.Message;
                    tradeBrreakOutChangeModel.SubmissionType = StatusDetail.Rejected;
                }
            }

            return Json( tradeChangeModels.Where( x => x.SubmissionType != StatusDetail.Approved ) );
        }

        public JsonResult RemoveTrades(IEnumerable<TradeChangeModel> list)
        {
            var localList = new List<TradeChangeModel>();

            if (list == null) return Json(localList);
            
            localList.AddRange(list.Where(item => !item.Enabled));

            return Json(localList); 
        }

        public JsonResult RemoveTradesBreakOut(IEnumerable<TradeBreakOutChangeModel> list)
        {
            var localList = new List<TradeBreakOutChangeModel>();

            if (list == null) return Json(localList);

            localList.AddRange(list.Where(item => !item.Enabled));

            return Json(localList);
        }

        public void PullTradeBreakOutRecord(decimal id)
        {
            var tradeBreakOut = DataTrades.LoadTradeExtendedBreakOutByPk(id);

            var tradeBreakOutList = new List<SL_TradeBreakOutExtendedProjection>();

            tradeBreakOutList.Add(tradeBreakOut);

            RealTime.Broadcast.TradeBreakOut(tradeBreakOutList);
        }

        private TradeType MirrorTradeType(SL_ExecutionSystemType executionType, TradeType currentType)
        {
            if ((executionType == SL_ExecutionSystemType.LOANET) ||
                (executionType == SL_ExecutionSystemType.LOANETINTL))
            {
                switch (currentType)
                {
                    case TradeType.StockBorrow:
                        return TradeType.StockLoan;
                    default:
                    case TradeType.StockLoan:
                        return TradeType.StockBorrow;
                }
            }
            else
            {
                switch (currentType)
                {
                    case TradeType.BorrowVsBasket:
                        return TradeType.LoanVsBasket;
                    case TradeType.BorrowVsBasketFee:
                        return TradeType.LoanVsBasketFee;
                    case TradeType.BorrowVsCashPool:
                        return TradeType.LoanVsCashPool;
                    case TradeType.BorrowVsFee:
                        return TradeType.LoanVsFee;
                    case TradeType.BorrowVsPledge:
                        return TradeType.LoansVsPledge;
                    case TradeType.BorrowVsRate:
                        return TradeType.LoanVsRate;

                    case TradeType.LoanVsBasket:
                        return TradeType.BorrowVsBasket;
                    case TradeType.LoanVsBasketFee:
                        return TradeType.BorrowVsBasketFee;
                    case TradeType.LoanVsCashPool:
                        return TradeType.BorrowVsCashPool;
                    case TradeType.LoanVsFee:
                        return TradeType.BorrowVsFee;
                    case TradeType.LoansVsPledge:
                        return TradeType.BorrowVsPledge;
                    case TradeType.LoanVsRate:
                        return TradeType.BorrowVsRate;
                    default:
                        return TradeType.BorrowVsRate;
                }
            }
        }

        private SL_DeliveryCode TradeTypeDefaultDeliveryCode(TradeType currentType)
        {
            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingTradeBorrowDeliveryCode", "PTS"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingTradeLoanDeliveryCode", "CCF"));

            switch (currentType)
            {
                case TradeType.StockBorrow:
                    return deliveryCodeBorrow;
                case TradeType.StockLoan:
                    return deliveryCodeLoan;
                default:
                    return SL_DeliveryCode.NONE;
            }
        }

        public JsonResult ScratchPadMirrorTrades(string contraEntityId, 
            string rate, 
            string profitCenter, 
            string mark, 
            Currency? currency, 
            IEnumerable<TradeChangeModel> list)
        {
            var localList = new List<TradeChangeModel>();


            List<SL_CostOfFund> costofFundsList = new List<SL_CostOfFund>();

            costofFundsList = DataFunding.LoadCostOfFund(list.Select(x => x.EntityId).First());

            var tradeChangeModels = list as TradeChangeModel[] ?? list.ToArray();
            var processId = tradeChangeModels.Max(x => x.ModelId);

            if (list == null) return Json(localList);
            foreach (var item in tradeChangeModels)
            {
                processId = processId + 1;
                var model = new TradeChangeModel();
                if (item.Enabled)
                {
                    model.ModelId = processId;
                    model.EffectiveDate = DateCalculator.Default.Today;
                    model.EntityId = item.EntityId;
                    model.ContraEntityId = (string.IsNullOrWhiteSpace(contraEntityId)) ? "" : contraEntityId;
                    model.Amount = item.Amount;
                    model.Availability = item.Availability;
                    model.CurrencyCode = currency ?? item.CurrencyCode;
                    model.Enabled = item.Enabled;
                    model.IssueId = item.IssueId;
                    model.SecurityNumber = item.SecurityNumber;
                    model.Ticker = item.Ticker;
                    model.Mark = (string.IsNullOrWhiteSpace(mark)) ? item.Mark : decimal.Parse(mark);
                    model.Price = item.Price;
                    model.MktPrice = item.MktPrice;
                    model.ProfitId = (string.IsNullOrWhiteSpace(contraEntityId)) ? item.ProfitId : profitCenter;
                    model.Quantity = item.Quantity;
                    model.RebateRate = (string.IsNullOrWhiteSpace(rate)) ? 0 : decimal.Parse(rate);
                    model.SecuritySettleDate = item.SecuritySettleDate;
                    model.CashSettleDate = item.CashSettleDate;
                    model.ExpectedEndDate = item.ExpectedEndDate;
                    model.TermDate = item.TermDate;
                    model.TradeDate = item.TradeDate;
                    model.ExecutingType = item.ExecutingType;                    
                    model.SubmissionType = item.SubmissionType;

                    model.TradeType = MirrorTradeType(item.ExecutingType, item.TradeType);
                    model.DeliveryCode = TradeTypeDefaultDeliveryCode(model.TradeType);

                    model.Amount = SLTradeCalculator.CalculateMoney(
                        Convert.ToDouble(model.Price),
                        model.Quantity,
                        Convert.ToDouble(model.Mark));

                    model.CostOfFunds = SLTradeCalculator.IncludeCostOfFunds(costofFundsList, model.EntityId, model.TradeType, model.ProfitId, model.CurrencyCode);

                    if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                    {
                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                            item.TradeType,
                            SLTradeCalculator.Locale.Domestic,
                            Convert.ToDecimal(item.RebateRate),
                            Convert.ToDecimal(item.Amount),
                            item.CostOfFunds,
                            item.CollateralFlag);
                    }
                    else
                    {
                        item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                          item.TradeType,
                          SLTradeCalculator.Locale.International,
                          Convert.ToDecimal(item.RebateRate),
                          Convert.ToDecimal(item.Amount),
                          item.CostOfFunds,
                          item.CollateralFlag);
                    }    

                    localList.Add(model);
                }

                localList.Add(item);
            }

            return Json(localList);
        }



        public JsonResult ScratchPadMirrorTradesBreakOut(string contraEntityId,
            string rate,
            string profitCenter,
            string mark,
            Currency? currency,            
            IEnumerable<TradeBreakOutChangeModel> list)
        {
            var localList = new List<TradeBreakOutChangeModel>();

            var tradeChangeModels = list as TradeBreakOutChangeModel[] ?? list.ToArray();
            var processId = tradeChangeModels.Max(x => x.ModelId);

            if (list == null) return Json(localList);
            foreach (var item in tradeChangeModels)
            {
                processId = processId + 1;
                var model = new TradeBreakOutChangeModel();
                if (item.Enabled)
                {
                    model.TradeType = item.TradeType == TradeType.StockBorrow ? TradeType.StockLoan : TradeType.StockBorrow;

                    model.ModelId = processId;
                    model.EffectiveDate = item.EffectiveDate;
                    model.EntityId = item.EntityId;
                    model.ContraEntityId = (string.IsNullOrWhiteSpace(contraEntityId)) ? "" : contraEntityId;

                    if (model.TradeType == TradeType.StockBorrow)
                    {
                        model.BorrowAmount = item.LoanAmount;
                        model.BorrowQuantity = item.LoanQuantity;
                    }
                    else
                    {
                        model.LoanAmount = item.BorrowAmount;
                        model.LoanQuantity = item.BorrowQuantity;
                    }

                    model.Availability = item.Availability;
                    model.CurrencyCode = currency ?? item.CurrencyCode;
                    model.Enabled = item.Enabled;
                    model.IssueId = item.IssueId;
                    model.SecurityNumber = item.SecurityNumber;
                    model.Ticker = item.Ticker;
                    model.MktPrice = item.MktPrice;
                    model.Mark = (string.IsNullOrWhiteSpace(mark)) ? item.Mark : decimal.Parse(mark);
                    model.Price = item.Price;
                    model.ProfitId = (string.IsNullOrWhiteSpace(contraEntityId)) ? item.ProfitId : profitCenter;
                    
                    model.RebateRate = (string.IsNullOrWhiteSpace(rate)) ? 0 : decimal.Parse(rate);
                    model.SettlementDate = item.SettlementDate;
                    model.ExecutingType = item.ExecutingType;
                                                       
                    model.SubmissionType = item.SubmissionType;

                    model.TradeType = MirrorTradeType(item.ExecutingType, item.TradeType);
                    model.DeliveryCode = TradeTypeDefaultDeliveryCode(model.TradeType);


                    if (model.TradeType == TradeType.StockBorrow)
                    {
                        model.BorrowAmount = SLTradeCalculator.CalculateMoney(
                            Convert.ToDouble(model.Price),
                            model.BorrowQuantity,
                            Convert.ToDouble(model.Mark));
                        if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.BorrowAmount),
                                item.CostOfFunds,
                                SL_CollateralFlag.C);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.BorrowAmount),
                              item.CostOfFunds,
                              SL_CollateralFlag.C);
                        }
                    }
                    else
                    {
                        model.LoanAmount = SLTradeCalculator.CalculateMoney(
                            Convert.ToDouble(model.Price),
                            model.LoanQuantity,
                            Convert.ToDouble(model.Mark));

                        if (item.ExecutingType == SL_ExecutionSystemType.LOANET)
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                                item.TradeType,
                                SLTradeCalculator.Locale.Domestic,
                                Convert.ToDecimal(item.RebateRate),
                                Convert.ToDecimal(item.LoanAmount),
                                0,
                                SL_CollateralFlag.C);
                        }
                        else
                        {
                            item.IncomeAmount = SLTradeCalculator.CalculateIncome(
                              item.TradeType,
                              SLTradeCalculator.Locale.International,
                              Convert.ToDecimal(item.RebateRate),
                              Convert.ToDecimal(item.LoanAmount),
                              0,
                              SL_CollateralFlag.C);
                        }
                    }

                    localList.Add(model);
                }

                localList.Add(item);
            }

            return Json(localList);
        }

        public JsonResult Read_ClassificationTypes([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<string>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            { 
                response = StaticDataCache.BoxCalculationStaticGet(DateCalculator.Default.Today, entityId).Select(x => x.Classification).Distinct().ToList();

                if (response.Count() == 0)
                {
                    StaticDataCache.RefreshStaticBoxDataGet(DateCalculator.Default.Today, entityId);
                    response = StaticDataCache.BoxCalculationStaticGet(DateCalculator.Default.Today, entityId).Select(x => x.Classification).Distinct().ToList();
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ClassificationTypesMultiSelect([DataSourceRequest] DataSourceRequest request)
        {
            List<string> response = new List<string>();
           
            try
            {
                response = DataIssue.LoadClassification().Select(x => x.IssueSubType2Desc).Distinct().ToList();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SendPendingTradeByType(string entityId, SendPendingTradeTypeEnum type)
        {
            var trades = DataTrades.LoadTradesExtended(DateTime.Today, entityId).Where(x => x.TradeStatus == StatusDetail.Pending).ToList();

            switch (type)
            {
                case SendPendingTradeTypeEnum.EnterTime:
                    trades = trades.OrderBy(x => x.DateTimeId).ToList();
                    break;

                case SendPendingTradeTypeEnum.Money:
                    trades = trades.OrderByDescending(x => x.Amount).ToList();
                    break;

                case SendPendingTradeTypeEnum.TradeType_Borrows:
                    trades = trades.OrderByDescending(x => x.TradeType).ToList();
                    break;
                case SendPendingTradeTypeEnum.TradeType_Loans:
                    trades = trades.OrderBy(x => x.TradeType).ToList();
                    break;
            }

            var items = new List<SL_TradeExtendedProjection>();

            foreach (var trade in trades)
            {
                try
                {
                    if ((trade.TradeStatus == StatusDetail.Approved) ||
                        (trade.TradeStatus == StatusDetail.Transmitted)) continue;

                    var status = DataExternalOperations.AddTradeToContract(trade, false);

                    trade.TradeStatus = status ? StatusDetail.Approved : StatusDetail.Rejected;

                    items.Add(trade);
                }
                catch (Exception e)
                {
                    trade.TradeStatus = StatusDetail.Rejected;
                    return ThrowJsonError(e);
                }
            }

            return Json(items);
        }
        public ActionResult ReadPushListOptionsMultiSelect([DataSourceRequest] DataSourceRequest request,
      List<string> entityId,
      string otherEntityId,
      PushPadDisplayRateEnum displatRateType,
      PushPadAvailLookupEnum availLookupType,
      PushPadDisplayOCCEnum displayOCCType,
      PushPadReceivesEnum displayReceiveType,
      bool showRateComparison,
      bool pushListRollup,
      decimal quantityMin,
      double priceMin,
      DtccEligibleType? eligible,      
      List<string> issueTypes,
      float hariCut,
      int? parValue)
        {
            List<PushListModel> pushList = new List<PushListModel>();
            List<SL_RegulatoryListProjection> regList = new List<SL_RegulatoryListProjection>();

            try
            {
                if (entityId != null)
                {
                    if (entityId.Any())
                    {
                        DateTime today = DateCalculator.Default.Today;

                        List<SL_BoxCalculationExtendedProjection> boxList = new List<SL_BoxCalculationExtendedProjection>();
                        List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();
                        List<SL_CallBackSummaryExtendedProjection> callbackSummaryList = new List<SL_CallBackSummaryExtendedProjection>();
                        List<SL_ReturnActionExtendedProjection> returnActionList = new List<SL_ReturnActionExtendedProjection>();
                       
                        
                        foreach (var _entityId in entityId)
                        {
                            try { boxList.AddRange(DataBoxCalculation.LoadBoxCalculation(today, _entityId.ToString(), 0, 100, false, true, false, false)); } catch { }
                            try { contractList.AddRange(DataContracts.LoadContracts(today, DateCalculator.Default.Today, _entityId)); } catch { }
                            try { callbackSummaryList.AddRange(DataCallback.LoadCallbackSummaryExtended(today, _entityId)); } catch { }
                            try { returnActionList.AddRange(DataReturnAction.LoadReturnAction(today, _entityId)); } catch { }
                            try { regList.AddRange(DataRegulatoryList.LoadRegulatoryList(today, _entityId, SLRegulatoryType.RESTRICTED)); } catch { }
                        }

                        boxList.RemoveAll(x => x.Price < priceMin);

                        issueTypes.RemoveAll(x => x.Equals(""));

                        if (issueTypes.Count() > 0)
                        {
                            boxList.RemoveAll(x => !issueTypes.Contains(x.Classification.ToUpper()));
                        }


                        if (eligible != null) { boxList.RemoveAll(x => x.DtccEligible != eligible); }

                        /*switch (availLookupType)
                        {
                            case PushPadAvailLookupEnum.DepoPosition:
                                boxList.RemoveAll(x => x.DepositorySettled == 0);
                                break;

                            case PushPadAvailLookupEnum.ExcessDeficit:
                                boxList.RemoveAll(x => x.ExcessPositionSettled == 0);
                                break;

                            case PushPadAvailLookupEnum.FullyPaid:
                                boxList.RemoveAll(x => x.FPLPositionAvailableSettled == 0);
                                break;

                            case PushPadAvailLookupEnum.TotalAvail:
                                boxList.RemoveAll(x => x.NetPositionSettled == 0);
                                break;
                        }*/

                        if (showRateComparison)
                        {
                            contractList.AddRange(DataContracts.LoadContracts(today, today, otherEntityId));
                        }

                        switch (displayOCCType)
                        {                            
                            case PushPadDisplayOCCEnum.DisplayNonOCC:
                                boxList.RemoveAll(x => x.OccEligibleCheck == true);
                                break;

                            case PushPadDisplayOCCEnum.DisplayOnlyOCC:
                                boxList.RemoveAll(x => x.OccEligibleCheck == false);
                                break;
                        }

                        pushList = boxList.Select(x => new PushListModel()
                        {
                            EntityId = x.EntityId,
                            IssueId = (int)x.IssueId,
                            ClearingId = x.ClearingId,
                            SeucrityIdentifer = x.SecurityNumber,
                            Ticker = x.Ticker,
                            Sedol = x.Sedol,
                            Isin = x.Isin,
                            SecNumber = x.SecNumber,
                            Price = x.Price,
                            DtccEligible = x.DtccEligible,
                            IntraDayRate = x.IntradayLendingRate,
                            OnlyOCC = x.OccEligibleCheck,
                            Classification = x.Classification,
                            SLPushList = x.SLBoxCalculation,
                            UnderlyingBoxCalc = x                         
                        }).ToList();

                        pushList.ForEach(x =>
                        {
                            try
                            {
                                x.RecieveQuantity = callbackSummaryList.Where(q => q.IssueId == x.IssueId && q.EntityId == x.EntityId).Sum(q => q.LoanCallbackQuantity);
                                x.RecieveRate = callbackSummaryList.Where(q => q.IssueId == x.IssueId && q.EntityId == x.EntityId).Average(q => q.LoanAverageWeightedRate);
                            }
                            catch { }

                            try
                            {                               
                                x.SameDayDeltaQuantity = contractList.Where(q => q.SecuritySettleDate == today && q.EntityId == x.EntityId && q.IssueId == x.IssueId).Sum(q => q.Quantity * q.TradeType.GetParDirection());
                                x.SameDayDeltaAmount = contractList.Where(q => q.SecuritySettleDate == today && q.EntityId == x.EntityId && q.IssueId == x.IssueId).Sum(q => q.Amount * q.TradeType.GetParDirection());


                                x.PendingOutReturn = returnActionList.Where(q => q.EntityId == x.EntityId && q.IssueId == x.IssueId &&  ((TradeType)q.TradeType).GetParDirection() == 1 && (q.BorrowDepositoryStatus == SL_DtccItemDOPendMade.Pending || q.BorrowDepositoryStatus == SL_DtccItemDOPendMade.Unknown)).Sum(q => q.Quantity);
                                x.PendingOutNewLoan = contractList.Where(q => q.SecuritySettleDate == today && q.EntityId == x.EntityId && q.IssueId == x.IssueId && q.TradeType.GetParDirection() == -1 && (q.DepositoryStatus == SL_DtccItemDOPendMade.Pending || q.DepositoryStatus == SL_DtccItemDOPendMade.Unknown)).Sum(q => q.Quantity);
                                x.SameDayLoanRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.SecuritySettleDate == today && q.TradeType == TradeType.StockLoan && q.EntityId == x.EntityId && q.IssueId == x.IssueId));
                            }
                            catch { }

                            switch (availLookupType)
                            {
                                case PushPadAvailLookupEnum.DepoPosition:
                                    x.Quantity = x.UnderlyingBoxCalc.DepositorySettled;
                                    break;

                                case PushPadAvailLookupEnum.ExcessDeficit:
                                    x.Quantity = x.UnderlyingBoxCalc.ExcessPositionSettled;

                                    switch (displayReceiveType)
                                    {
                                        case PushPadReceivesEnum.WithReceives:
                                            if ((x.UnderlyingBoxCalc.CnsFailToRecievePositionSettled + x.UnderlyingBoxCalc.DvpFailToRecievePositionSettled + x.UnderlyingBoxCalc.BrokerFailToDeliverPositionSettled) > 0)
                                            {
                                                x.VesusRecieve = true;
                                                x.MemoInfo = "VS REC";
                                            }

                                            x.Quantity = x.Quantity + (x.UnderlyingBoxCalc.CnsFailToRecievePositionSettled + x.UnderlyingBoxCalc.DvpFailToRecievePositionSettled + x.UnderlyingBoxCalc.BrokerFailToDeliverPositionSettled);
                                            break;

                                        default:
                                        case PushPadReceivesEnum.WithoutReceives:
                                            break;
                                    }
                                    break;

                                case PushPadAvailLookupEnum.FullyPaid:
                                    x.Quantity = x.UnderlyingBoxCalc.FPLPositionAvailableSettled;
                                    break;

                                case PushPadAvailLookupEnum.TotalAvail:
                                    x.Quantity = x.UnderlyingBoxCalc.ExcessPositionSettled;
                                    break;
                            }

                            switch (displatRateType)
                            {
                                case PushPadDisplayRateEnum.AVGWeightedRate:
                                    if (contractList.Any(q => q.EntityId == x.EntityId && q.IssueId == x.IssueId))
                                    {
                                        x.IntraDayRate = SLTradeCalculator.CalculateAvgWeightedRate(x.EntityId, (int)x.IssueId, contractList);
                                    }
                                    else
                                    {
                                        x.IntraDayRate = null;
                                    }
                                    break;
                                case PushPadDisplayRateEnum.IntradayRate:
                                    x.IntraDayRate = x.UnderlyingBoxCalc.IntradayLendingRate;
                                    break;
                                case PushPadDisplayRateEnum.PredictiveRate:
                                    x.IntraDayRate = 0;
                                    break;
                            }


                            if (showRateComparison)
                            {
                                x.OtherEntityRate = SLTradeCalculator.CalculateAvgWeightedRate(otherEntityId, (int)x.IssueId, contractList);
                            }

                            x.Amount = x.Quantity * (decimal)x.Price;
                        });
                    }
                }


                if (pushListRollup)
                {
                    string entityIdRollupString = "";
                    
                    foreach (var item in entityId)
                    {
                        entityIdRollupString += item + ";";
                    }

                    var testPushList = pushList.GroupBy(x => new {
                        x.Classification,                        
                        x.DtccEligible,                        
                        x.Isin,
                        x.IssueId,
                        x.MemoInfo,
                        x.OtherEntityRate,                        
                        x.SecNumber,
                        x.Sedol,
                        x.SeucrityIdentifer,
                        x.Ticker,
                        x.VesusRecieve}).Select(q => new PushListModel()
                        {
                            EntityId = entityIdRollupString,
                            ClearingId = "****",
                            Classification = q.Key.Classification,
                            DtccEligible = q.Key.DtccEligible,
                            IntraDayRate = q.Min(x => x.IntraDayRate),
                            Isin = q.Key.Isin,
                            IssueId = q.Key.IssueId,
                            MemoInfo = q.Key.MemoInfo,
                            OnlyOCC = false,
                            OtherEntityRate = q.Key.OtherEntityRate,
                            Price = q.Max(m => m.Price),
                            SecNumber = q.Key.SecNumber,
                            Sedol = q.Key.Sedol,
                            SeucrityIdentifer = q.Key.SeucrityIdentifer,
                            Ticker = q.Key.Ticker,
                            VesusRecieve = q.Key.VesusRecieve,
                            Quantity = q.Sum(x => x.Quantity),
                            Amount = q.Sum(x => x.Amount)
                        }).ToList();

                    foreach (var item in testPushList)
                    {
                        switch (availLookupType)
                        {
                           case PushPadAvailLookupEnum.TotalAvail:
                                item.Quantity = (item.Quantity < 0) ? 0 : item.Quantity;
                                break;
                        }
                    }
                    pushList = testPushList;               
                }


                pushList.RemoveAll(x => x.Quantity < quantityMin);

                foreach (var item in pushList)
                {
                    if (regList.Any(x => x.IssueId == item.IssueId))
                    {
                        item.Quantity = 0;
                        item.Amount = 0;
                    }
                }

                if (parValue != null)
                {
                    pushList.ForEach(x =>
                    {
                        if (x.Quantity > (decimal)parValue)
                        {
                            x.Quantity = Math.Floor(x.Quantity / (decimal)parValue) * (decimal)parValue;
                        }
                        else
                        {
                            x.Quantity = 0;
                        }

                        x.Amount = x.Quantity * (decimal)x.Price;
                    });            
                }

                pushList.RemoveAll(x => x.Quantity == 0 && x.RecieveQuantity == 0 && x.SameDayDeltaQuantity == 0);
            }
            catch (Exception error)
            {
                ThrowJsonError(error);
            }

            return Extended.JsonMax(pushList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}
