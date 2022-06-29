using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;

using BondFire.Core.Dates;

using BondFire.Entities;
using BondFire.Entities.Projections;
using BondFire.Calculators;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Custom;
using SLTrader.Enums;
using SLTrader.Tools;
using SLTrader.Helpers;
using SLTrader.Tools.Helpers;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ContractRelatedController : BaseController
    {
        public const string message = "MSG Submitted To System : {0}";

        // GET: DomesticTrading/ContractAction
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult LoadCopyContract(List<SL_ContractBreakOutExtendedProjection> contractList)
        {
            var item = new SingleTradeModel();
            var contract = contractList.First();

            try
            {
                var company = DataEntity.LoadEntities().Where( x => x.CompanyId.ToString() == contract.EntityId ).First();

                item.Company = company;
                item.Trade1 = new SL_TradeExtendedProjection();
                item.Trade2 = new SL_TradeExtendedProjection();

                item.Trade1.ExecutingSystem = contract.ExecutingSystem;
                item.Trade1.EntityId = contract.EntityId;
                item.Trade1.IssueId = contract.IssueId;
                item.Trade1.SecurityNumber = string.IsNullOrWhiteSpace(contract.SecurityNumber) ? "" : contract.SecurityNumber;
                item.Trade1.Ticker = contract.Ticker;
                item.Trade1.Sedol = string.IsNullOrWhiteSpace(contract.Sedol) ? "" : contract.Sedol;
                item.Trade1.Isin = string.IsNullOrWhiteSpace(contract.Isin) ? "" : contract.Isin;
                item.Trade1.Quick = string.IsNullOrWhiteSpace(contract.Quick) ? "" : contract.Quick;
                item.Trade1.Price = Convert.ToDouble(contract.MktPrice);
                item.Trade1.TradeDate = DateCalculator.Default.Today;
                item.Trade1.Mark = Convert.ToDouble(contract.Mark/100);
                item.Trade1.MarkParameterId = contract.MarkParameterId;
                item.Trade1.PriceCalcType = SL_TradePriceAmountCalcType.CASH;
                item.Trade1.AmountCalcType = SL_TradeAmountCalcType.Cash;
                item.Trade1.TradeType = contract.TradeType;

                item.Trade2.ExecutingSystem = contract.ExecutingSystem;
                item.Trade2.EntityId = contract.EntityId;
                item.Trade2.IssueId = contract.IssueId;
                item.Trade2.SecurityNumber = string.IsNullOrWhiteSpace(contract.SecurityNumber) ? "" : contract.SecurityNumber;
                item.Trade2.Ticker = contract.Ticker;
                item.Trade2.Sedol = string.IsNullOrWhiteSpace(contract.Sedol) ? "" : contract.Sedol;
                item.Trade2.Isin = string.IsNullOrWhiteSpace(contract.Isin) ? "" : contract.Isin;
                item.Trade2.Quick = string.IsNullOrWhiteSpace(contract.Quick) ? "" : contract.Quick;
                item.Trade2.Price = Convert.ToDouble(contract.MktPrice);
                item.Trade2.TradeDate = DateCalculator.Default.Today;
                item.Trade2.Mark = Convert.ToDouble(contract.Mark/100);                
                item.Trade2.MarkParameterId = contract.MarkParameterId;
                item.Trade2.PriceCalcType = SL_TradePriceAmountCalcType.CASH;
                item.Trade2.AmountCalcType = SL_TradeAmountCalcType.Cash;
                item.Trade2.TradeType = contract.TradeType;

                item.Trade1.Quantity = (contract.TradeType.GetParDirection() == 1 ? contract.BorrowQuantity : contract.LoanQuantity);
                item.Trade1.Amount = (contract.TradeType.GetParDirection() == 1 ? contract.BorrowAmount : contract.LoanAmount);
                item.Trade1.TradeNumber = "";
                item.Trade1.RebateRate = Convert.ToDouble(contract.RebateRate);
                item.Trade1.RebateRateId = contract.RebateRateId;
                item.Trade1.CashSettleDate = contract.CashSettleDate;
                item.Trade1.SecuritySettleDate = contract.SecuritySettleDate;
                item.Trade1.CollateralFlag = contract.CollateralFlag;
                item.Trade1.CurrencyCode = contract.CurrencyCode;
                item.Trade1.SecurityLoc = contract.SecurityLoc;
                item.Trade1.CashLoc = contract.CashLoc;
                item.Trade1.ProfitId = contract.ProfitId;
                item.Trade1.DividendRate = Convert.ToDouble(contract.DividendRate);
                item.Trade1.IsStp = true;
                item.Trade1.FeeType = contract.FeeType;
                item.Trade1.FeeOffset = contract.FeeOffset;
                item.Trade1.RebateRate = Convert.ToDouble(contract.RebateRate);
                item.Trade1.RebateRateId = contract.RebateRateId;
                item.Trade1.EffectiveDate = contract.EffectiveDate;


                item.Trade2.Quantity = (contract.TradeType.GetParDirection() == 1 ? contract.BorrowQuantity : contract.LoanQuantity);
                item.Trade2.Amount = (contract.TradeType.GetParDirection() == 1 ? contract.BorrowAmount : contract.LoanAmount);
                item.Trade2.TradeNumber = "";
                item.Trade2.RebateRate = Convert.ToDouble(contract.RebateRate);
                item.Trade2.RebateRateId = contract.RebateRateId;
                item.Trade2.CashSettleDate = contract.CashSettleDate;
                item.Trade2.SecuritySettleDate = contract.SecuritySettleDate;
                item.Trade2.CollateralFlag = contract.CollateralFlag;
                item.Trade2.CurrencyCode = contract.CurrencyCode;
                item.Trade2.SecurityLoc = contract.SecurityLoc;
                item.Trade2.CashLoc = contract.CashLoc;
                item.Trade2.ProfitId = contract.ProfitId;
                item.Trade2.DividendRate = Convert.ToDouble(contract.DividendRate);
                item.Trade2.IsStp = true;
                item.Trade2.FeeType = contract.FeeType;
                item.Trade2.FeeOffset = contract.FeeOffset;
                item.Trade2.RebateRate = Convert.ToDouble(contract.RebateRate);
                item.Trade2.RebateRateId = contract.RebateRateId;
                item.Trade2.EffectiveDate = contract.EffectiveDate;


                if (contract.ExecutingSystem == SL_ExecutionSystemType.LOANET)
                {
                    if (contract.TradeType.GetParDirection() == 1)
                    {
                        item.Trade1.DeliveryCode = SL_DeliveryCode.PTS;
                        item.Trade2.DeliveryCode = SL_DeliveryCode.CCF;
                    }
                    else
                    {
                        item.Trade1.DeliveryCode = SL_DeliveryCode.CCF;
                        item.Trade2.DeliveryCode = SL_DeliveryCode.PTS;
                    }
                }
                else
                {
                    item.Trade1.DeliveryCode = SL_DeliveryCode.NONE;
                    item.Trade2.DeliveryCode = SL_DeliveryCode.NONE;
                }             

                item.IsMirror = false;
            }
            catch (Exception e)
            {
                ThrowJsonError(e);
            }

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_BoxCalculationTrade.cshtml", item);
        }

        public PartialViewResult Load_MemoSeg(IEnumerable<SL_BoxCalculationExtendedProjection> items)
        {
            List<MemoSegUpdateModel> memoSegList = new List<MemoSegUpdateModel>();
            long id = 1;

            try
            {

                foreach (SL_BoxCalculationExtendedProjection item in items)
                {
                    try
                    {
                        var memoSegItem = MemoSegUpdateModel.Convert(id, item);

                        memoSegList.Add(memoSegItem);
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);

            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationMemoSeg.cshtml", memoSegList);
        }

        public PartialViewResult Load_MemoSegByContract(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<MemoSegUpdateModel> memoSegList = new List<MemoSegUpdateModel>();
            long id = 1;

            try
            {

                foreach (SL_ContractBreakOutExtendedProjection item in items)
                {
                    try
                    {
                        var memoSegItem = MemoSegUpdateModel.Convert(id, item);

                        memoSegList.Add(memoSegItem);
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);

            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationMemoSeg.cshtml", memoSegList);
        }

        public PartialViewResult Load_RecallByContract(IEnumerable<SL_ContractExtendedProjection> items)
        {
            List<RecallTypeCalculationModel> recallList = new List<RecallTypeCalculationModel>();
            long id = 1;

            try
            {

                foreach (SL_ContractExtendedProjection item in items)
                {
                    try
                    {
                        var recallItem = RecallTypeCalculationModel.Convert(id, item);

                        if (recallItem.Quantity > 0)
                        {
                            recallList.Add(recallItem);
                            id += 1;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);

            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationRecall.cshtml", recallList);
        }

        public PartialViewResult Load_RecallByContractBreakOut(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<RecallTypeCalculationModel> recallList = new List<RecallTypeCalculationModel>();
            long id = 1;

            try
            {

                foreach (SL_ContractBreakOutExtendedProjection item in items)
                {
                    try
                    {
                        var recallItem = RecallTypeCalculationModel.Convert(id, item);

                        if (recallItem.Quantity > 0)
                        {
                            recallList.Add(recallItem);
                            id += 1;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);

            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationRecall.cshtml", recallList);
        }

        public PartialViewResult Load_RateChangeByContract(IEnumerable<SL_ContractExtendedProjection> items)
        {
            List<RateChangeModel> rateList = new List<RateChangeModel>();

            try
            {
                int id = 1;

                if (items != null)
                {
                    foreach (SL_ContractExtendedProjection item in items)
                    {
                        try
                        {
                            rateList.Add(RateChangeModel.Convert(id, "", item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractRateChange.cshtml", rateList);
        }

        public PartialViewResult Load_ContractBreakOutWithFees(DateTime effectiveDate, List<string> entityId)
        {
            List<SL_ContractBreakOutExtendedProjection> contractList = new List<SL_ContractBreakOutExtendedProjection>();


            foreach (var _entityId in entityId)
            {
                contractList.AddRange(DataContracts.LoadContractsBreakOut(effectiveDate, effectiveDate, _entityId).Where(x => x.FeeType != "REBATE" && x.SecuritySettleDate <= effectiveDate).ToList());
            }

            List<RateChangeModel> rateList = new List<RateChangeModel>();

            try
            {
                int id = 1;

                if (contractList != null)
                {
                    foreach (SL_ContractBreakOutExtendedProjection item in contractList)
                    {
                        try
                        {
                            rateList.Add(RateChangeModel.ConvertForFee(id, "", item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            rateList.RemoveAll(x => x.OldRate == x.NewRate);

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractRateChange.cshtml", rateList);
        }

        public PartialViewResult Load_CancelsByContract(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<ContractCancelModel> contractCancelList = new List<ContractCancelModel>();

            try
            {
                int id = 1;

                if (items != null)
                {
                    foreach (SL_ContractBreakOutExtendedProjection item in items)
                    {
                        try
                        {
                            contractCancelList.Add(ContractCancelModel.Convert(id, item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractCancelChange.cshtml", contractCancelList);
        }
        public PartialViewResult Load_RateChangeByContractBreakOut(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<RateChangeModel> rateList = new List<RateChangeModel>();

            try
            {
                int id = 1;

                if (items != null)
                {
                    foreach (SL_ContractBreakOutExtendedProjection item in items)
                    {
                        try
                        {
                            rateList.Add(RateChangeModel.Convert(id, "", item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }            

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractRateChange.cshtml", rateList);
        }

        public PartialViewResult Load_RateChangeByContractBreakOutByRate(IEnumerable<SL_ContractBreakOutByRateExtendedProjection> items)
        {
            List<RateChangeModel> rateList = new List<RateChangeModel>();

            try
            {
                int id = 1;

                if (items != null)
                {
                    foreach (SL_ContractBreakOutByRateExtendedProjection item in items)
                    {
                        try
                        {
                            rateList.Add(RateChangeModel.Convert(id, "", item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractRateChange.cshtml", rateList);
        }

        public JsonResult ProcessRateChangeExpandedOptions(List<RateChangeModel> modelList,
                    string batchCodeOverride,
                   decimal? rateChangeDelta,
                   DateTime effectiveDate,
                   decimal? newRate,
                   string newRateCode)
        {

            if (modelList != null)
            {
                foreach (var model in modelList)
                {
                    if (model.Enabled)
                    {
                        if (rateChangeDelta != null)
                        {
                            model.NewRate = model.OldRate + (decimal)rateChangeDelta;
                        }

                        if (newRate != null)
                        {
                            model.NewRate = (decimal)newRate;
                            model.NewRateCode = newRateCode;
                        }


                        if ((model.SecurityLoc == "US") || (model.SecurityLoc == ""))
                        {
                            model.NewIncomeAmount = SLTradeCalculator.CalculateIncome(model.TradeType, SLTradeCalculator.Locale.Domestic, model.NewRate, model.Amount, model.CostOfFunds, model.CollateralFlag);
                        }
                        else
                        {
                            model.NewIncomeAmount = SLTradeCalculator.CalculateIncome(model.TradeType, SLTradeCalculator.Locale.International, model.NewRate, model.Amount, model.CostOfFunds, model.CollateralFlag);

                        }
                    }
                }
            }
            else
            {
                modelList = new List<RateChangeModel>();
            }

            return Extended.JsonMax(modelList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Load_RateChangeExpandedOptionsBySuggestionBySearchCriteria(string entityId,
                   SL_ColumnType columnEnum,
                   string searchCriteria,
                   TradeType tradeType,                                     
                   decimal? rateChangeDelta,
                   DateTime effectiveDate,
                   decimal? newRate,
                   string newRateCode,
                   SL_Operator operatorType,
                   string contraEntity)
        {
            int id = 1;

            List<RateChangeModel> rateChangeList = new List<RateChangeModel>();

            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();            
            contractList = DataContracts.LoadContracts(effectiveDate, effectiveDate, entityId);

            switch (columnEnum)
            {
                case SL_ColumnType.AMOUNT:
                    contractList = contractList.Where(x => x.Amount.ToString() == searchCriteria && x.TradeType == tradeType).ToList();
                    break;

                case SL_ColumnType.CLASSIFICATION:
                    switch (operatorType)
                    {
                        case SL_Operator.contains:
                            contractList = contractList.Where(x => x.Classification.ToUpper().Contains(searchCriteria.ToUpper()) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.eq:
                            contractList = contractList.Where(x => x.Classification.ToUpper() == searchCriteria.ToUpper() && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.startswith:
                            contractList = contractList.Where(x => x.Classification.ToUpper().StartsWith(searchCriteria.ToUpper()) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.notcontain:
                            contractList = contractList.Where(x => !x.Classification.ToUpper().Contains(searchCriteria.ToUpper()) && x.TradeType == tradeType).ToList();
                            break;
                        default:
                            contractList = contractList.Where(x => x.Classification.ToUpper() == searchCriteria.ToUpper() && x.TradeType == tradeType).ToList();
                            break;
                    }            
                    break;

                case SL_ColumnType.CONTRAENTITY:
                    switch (operatorType)
                    {
                        case SL_Operator.contains:
                            contractList = contractList.Where(x => x.ContraEntity.ToUpper().Contains(searchCriteria.ToUpper()) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.eq:
                            contractList = contractList.Where(x => x.ContraEntity.ToUpper() == searchCriteria.ToUpper() && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.startswith:
                            contractList = contractList.Where(x => x.ContraEntity.ToUpper().StartsWith(searchCriteria.ToUpper()) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.notcontain:
                            contractList = contractList.Where(x => !x.ContraEntity.ToUpper().Contains(searchCriteria.ToUpper()) && x.TradeType == tradeType).ToList();
                            break;
                        default:
                            contractList = contractList.Where(x => x.ContraEntity.ToUpper() == searchCriteria.ToUpper() && x.TradeType == tradeType).ToList();
                            break;
                    }
                    break;

                case SL_ColumnType.QUANTITY:
                    switch (operatorType)
                    {
                        case SL_Operator.eq:
                            contractList = contractList.Where(x => x.Quantity == decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;                        
                        case SL_Operator.gt:
                            contractList = contractList.Where(x => x.Quantity < decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.gte:
                            contractList = contractList.Where(x => x.Quantity <= decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.lt:
                            contractList = contractList.Where(x => x.Quantity > decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.lte:
                            contractList = contractList.Where(x => x.Quantity >= decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.neq:
                            contractList = contractList.Where(x => x.Quantity != decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        default:
                            contractList = contractList.Where(x => x.Quantity == decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                    }
                    break;

                case SL_ColumnType.REBATERATE:
                    switch (operatorType)
                    {
                        case SL_Operator.eq:
                            contractList = contractList.Where(x => x.RebateRate == decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.gt:
                            contractList = contractList.Where(x => x.RebateRate > decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.gte:
                            contractList = contractList.Where(x => x.RebateRate >= decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.lt:
                            contractList = contractList.Where(x => x.RebateRate < decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.lte:
                            contractList = contractList.Where(x => x.RebateRate <= decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        case SL_Operator.neq:
                            contractList = contractList.Where(x => x.RebateRate != decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                        default:
                            contractList = contractList.Where(x => x.RebateRate == decimal.Parse(searchCriteria) && x.TradeType == tradeType).ToList();
                            break;
                    }
                    break;

                case SL_ColumnType.ISSUE:
                    Issue issue = DataIssue.LoadIssue(searchCriteria);
                    contractList = contractList.Where(x => x.IssueId.ToString() == issue.IssueId.ToString() && x.TradeType == tradeType).ToList();
                    break;
            }

            contractList.RemoveAll(x => x.Quantity == 0);

            foreach (SL_ContractExtendedProjection contractItem in contractList)
            {
                try
                {
                    RateChangeModel calcItem = RateChangeModel.Convert(id, "", contractItem);

                    if (newRate != null)
                    {
                        calcItem.NewRate = (decimal)newRate;
                        calcItem.NewRateCode = newRateCode;
                    }

                    if (rateChangeDelta != null)
                    {
                        calcItem.NewRate = calcItem.OldRate + (decimal)rateChangeDelta;                     
                    }

                    if ((calcItem.SecurityLoc == "US") || (calcItem.SecurityLoc == ""))
                    {
                        calcItem.NewIncomeAmount = SLTradeCalculator.CalculateIncome(calcItem.TradeType, SLTradeCalculator.Locale.Domestic, calcItem.NewRate, calcItem.Amount, calcItem.CostOfFunds, calcItem.CollateralFlag);
                    }
                    else
                    {
                        calcItem.NewIncomeAmount = SLTradeCalculator.CalculateIncome(calcItem.TradeType, SLTradeCalculator.Locale.International, calcItem.NewRate, calcItem.Amount, calcItem.CostOfFunds, calcItem.CollateralFlag);
                    }

                    rateChangeList.Add(calcItem);
                    id += 1;
                }
                catch (Exception e)
                {

                }
            }

            if (!string.IsNullOrWhiteSpace(contraEntity))
            {
                rateChangeList.RemoveAll(x => !x.ContraEntity.ToLower().Equals(contraEntity.ToLower()));
            }

            return Extended.JsonMax(rateChangeList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Load_ReturnExpandedOptionsBySuggestionBySearchCriteria(string entityId, 
            SL_ColumnType columnEnum, 
            string searchCriteria,
            TradeType tradeType,
            SL_EntityType entityType,
            string batchCodeOverride,
            ReturnQuantityTypeEnum quantityType
            )
        {
            int id = 1;

            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();

            string batchCode = string.IsNullOrWhiteSpace(batchCodeOverride) ?  DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "") : batchCodeOverride;
            string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
            string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");

            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));


            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            contractList = DataContracts.LoadContracts(DateTime.Today, DateTime.Today, entityId);

            switch (columnEnum)
            {
                case SL_ColumnType.AMOUNT:
                    contractList = contractList.Where(x => x.Amount.ToString() == searchCriteria && x.TradeType == tradeType).ToList();
                    break;
                case SL_ColumnType.CLASSIFICATION:
                    contractList = contractList.Where(x => x.Classification == searchCriteria && x.TradeType == tradeType).ToList();
                    break;
                case SL_ColumnType.CONTRAENTITY:
                    contractList = contractList.Where(x => x.ContraEntity == searchCriteria && x.TradeType == tradeType).ToList();
                    break;
                case SL_ColumnType.QUANTITY:
                    contractList = contractList.Where(x => x.Quantity.ToString() == searchCriteria && x.TradeType == tradeType).ToList();
                    break;
                case SL_ColumnType.REBATERATE:
                    contractList = contractList.Where(x => x.RebateRate.ToString() == searchCriteria && x.TradeType == tradeType).ToList();
                    break;

                case SL_ColumnType.ISSUE:
                    Issue issue = DataIssue.LoadIssue(searchCriteria);

                    contractList = contractList.Where(x => x.IssueId.ToString() == issue.IssueId.ToString() && x.TradeType == tradeType).ToList();
                    break;
            }

            if (quantityType == ReturnQuantityTypeEnum.StartOfDay)
            {
                foreach (SL_ContractExtendedProjection contractItem in contractList.Where(x => x.QuantityStartOfDay > 0 && x.AmountStartOfDay > 0))
                {
                    try
                    {
                        ReturnTypeCalculationModel calcItem = new ReturnTypeCalculationModel();

                        calcItem.Enabled = false;
                        calcItem.ModelId = id;
                        calcItem.GroupTitle = "[" + contractItem.SecurityNumber + "] " + contractItem.Ticker;
                        calcItem.EntityType = entityType;
                        calcItem.Entity = contractItem.EntityId;
                        calcItem.ContraEntity = contractItem.ContraEntity;
                        calcItem.ContractNumber = contractItem.ContractNumber;
                        calcItem.TradeType = contractItem.TradeType;
                        calcItem.IssueId = contractItem.IssueId;
                        calcItem.SecurityNumber = contractItem.SecurityNumber;
                        calcItem.Ticker = contractItem.Ticker;
                        calcItem.Price = (contractItem.AmountStartOfDay / contractItem.QuantityStartOfDay);
                        calcItem.Rate = contractItem.RebateRate;
                        calcItem.Quantity = contractItem.QuantityStartOfDay;
                        calcItem.EffectiveDate = DateTime.Today;
                        calcItem.ValueDate = contractItem.CashSettleDate;
                        calcItem.ReturnProfitCenter = contractItem.ProfitId;
                        calcItem.ReturnBatchCode = batchCode;
                        calcItem.CallBack = (calcItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan;
                        calcItem.DeliveryCode = (calcItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan;
                        calcItem.SubmissionType = StatusDetail.Pending;

                        calcItem.ReturnPortionQuantity = contractItem.QuantityStartOfDay;
                        calcItem.ReturnPortionAmount = contractItem.AmountStartOfDay;
                        calcItem.PortionType = PortionType.Full;

                        returnList.Add(calcItem);
                        id += 1;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            else
            {
                foreach (SL_ContractExtendedProjection contractItem in contractList.Where(x => x.Quantity > 0 && x.Amount > 0))
                {
                    try
                    {
                        ReturnTypeCalculationModel calcItem = new ReturnTypeCalculationModel();

                        calcItem.Enabled = false;
                        calcItem.ModelId = id;
                        calcItem.GroupTitle = "[" + contractItem.SecurityNumber + "] " + contractItem.Ticker;
                        calcItem.EntityType = entityType;
                        calcItem.Entity = contractItem.EntityId;
                        calcItem.ContraEntity = contractItem.ContraEntity;
                        calcItem.ContractNumber = contractItem.ContractNumber;
                        calcItem.TradeType = contractItem.TradeType;
                        calcItem.IssueId = contractItem.IssueId;
                        calcItem.SecurityNumber = contractItem.SecurityNumber;
                        calcItem.Ticker = contractItem.Ticker;
                        calcItem.Price = (contractItem.Amount / contractItem.Quantity);
                        calcItem.Rate = contractItem.RebateRate;
                        calcItem.Quantity = contractItem.Quantity;
                        calcItem.EffectiveDate = DateTime.Today;
                        calcItem.ValueDate = contractItem.CashSettleDate;
                        calcItem.ReturnProfitCenter = contractItem.ProfitId;
                        calcItem.ReturnBatchCode = batchCode;
                        calcItem.CallBack = (calcItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan;
                        calcItem.DeliveryCode = (calcItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan;
                        calcItem.SubmissionType = StatusDetail.Pending;

                        calcItem.ReturnPortionQuantity = contractItem.Quantity;
                        calcItem.ReturnPortionAmount = contractItem.Amount;
                        calcItem.PortionType = PortionType.Full;

                        returnList.Add(calcItem);
                        id += 1;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            return Extended.JsonMax(returnList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Load_ReturnBySuggestionBySearchCriteria(string entityId, SL_ColumnType columnEnum, string searchCriteria)
        {        
            int id = 1;

            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();

            string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "");
            string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
            string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");

            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));


            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            contractList = DataContracts.LoadContracts(DateTime.Today, DateTime.Today, entityId);

            switch (columnEnum)
            {
                case SL_ColumnType.AMOUNT:
                    contractList = contractList.Where(x => x.Amount.ToString() == searchCriteria && x.TradeType == TradeType.StockBorrow).ToList();
                    break;
                case SL_ColumnType.CLASSIFICATION:
                    contractList = contractList.Where(x => x.Classification == searchCriteria && x.TradeType == TradeType.StockBorrow).ToList();
                    break;
                case SL_ColumnType.CONTRAENTITY:
                    contractList = contractList.Where(x => x.ContraEntity == searchCriteria && x.TradeType == TradeType.StockBorrow).ToList();
                    break;
                case SL_ColumnType.QUANTITY:
                    contractList = contractList.Where(x => x.Quantity.ToString() == searchCriteria && x.TradeType == TradeType.StockBorrow).ToList();
                    break;
                case SL_ColumnType.REBATERATE:
                    contractList = contractList.Where(x => x.RebateRate.ToString() == searchCriteria && x.TradeType == TradeType.StockBorrow).ToList();
                    break;

                case SL_ColumnType.ISSUE:
                    Issue issue = DataIssue.LoadIssue(searchCriteria);

                    contractList = contractList.Where(x => x.IssueId.ToString() == issue.IssueId.ToString() && x.TradeType == TradeType.StockBorrow).ToList();
                    break;
            }


            foreach (SL_ContractExtendedProjection contractItem in contractList.Where(x => x.Quantity > 0 && x.Amount > 0))
            {
                try
                {
                   ReturnTypeCalculationModel calcItem = new ReturnTypeCalculationModel();

                    calcItem.Enabled = false;
                    calcItem.ModelId = id;
                    calcItem.GroupTitle = "[" + contractItem.SecurityNumber + "] " + contractItem.Ticker;
                    calcItem.EntityType = SL_EntityType.Return;
                    calcItem.Entity = contractItem.EntityId;
                    calcItem.ContraEntity = contractItem.ContraEntity;
                    calcItem.ContractNumber = contractItem.ContractNumber;
                    calcItem.TradeType = contractItem.TradeType;
                    calcItem.IssueId = contractItem.IssueId;
                    calcItem.SecurityNumber = contractItem.SecurityNumber;
                    calcItem.Ticker = contractItem.Ticker;
                    calcItem.Price = (contractItem.Amount / contractItem.Quantity);
                    calcItem.Rate = contractItem.RebateRate;
                    calcItem.Quantity = contractItem.Quantity;
                    calcItem.EffectiveDate = DateTime.Today;
                    calcItem.ValueDate = contractItem.CashSettleDate;
                    calcItem.ReturnProfitCenter = contractItem.ProfitId;
                    calcItem.ReturnBatchCode = batchCode;
                    calcItem.CallBack = (calcItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan;
                    calcItem.DeliveryCode = (calcItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan;
                    calcItem.SubmissionType = StatusDetail.Pending;

                    calcItem.ReturnPortionQuantity = contractItem.Quantity;
                    calcItem.ReturnPortionAmount = contractItem.Amount;
                    calcItem.PortionType = PortionType.Full;

                    returnList.Add(calcItem);
                    id += 1;
                }
                catch (Exception e)
                {
                    
                }
            }

            return Extended.JsonMax(returnList, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Load_RecallBySuggestion(IEnumerable<SL_BoxCalculationExtendedProjection> items, bool useTraded)
        {
            List<RecallTypeCalculationModel> recallList = new List<RecallTypeCalculationModel>();

            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            decimal quantityToReturn = 0;
            int id = 1;
            decimal suggestionRecall = 0;

            foreach (SL_BoxCalculationExtendedProjection item in items)
            {
                try
                {
                    quantityToReturn = 0;
                    suggestionRecall = 0;

                    suggestionRecall = (useTraded) ? item.SuggestionRecallTraded : item.SuggestionReturnSettled;

                    contractList = DataContracts.LoadContractsByIssue((DateTime)item.EffectiveDate,
                        (DateTime)item.EffectiveDate,
                        item.EntityId,
                        item.IssueId.ToString());

                    contractList =
                        contractList.Where(x => x.TradeType == TradeType.StockLoan && x.IssueId == item.IssueId)
                            .ToList();

                    foreach (
                        SL_ContractExtendedProjection contractItem in
                            contractList.OrderBy(x => x.SecuritySettleDate)
                                .Where(x => x.Quantity > 0 || x.QuantitySettled > 0))
                    {
                        var calcItem = new RecallTypeCalculationModel();

                        calcItem.Enabled = false;
                        calcItem.ModelId = id;
                        calcItem.GroupTitle = "[" + contractItem.SecurityNumber + "] " + contractItem.Ticker;
                        calcItem.EntityType = SL_EntityType.Recall;
                        calcItem.Entity = contractItem.EntityId;
                        calcItem.ContraEntity = contractItem.ContraEntity;
                        calcItem.ContractNumber = contractItem.ContractNumber;
                        calcItem.TradeType = contractItem.TradeType;
                        calcItem.IssueId = contractItem.IssueId;
                        calcItem.SecurityNumber = contractItem.SecurityNumber;
                        calcItem.Ticker = contractItem.Ticker;
                        calcItem.Price = (contractItem.Amount / contractItem.Quantity);
                        calcItem.Quantity = contractItem.Quantity;
                        calcItem.SettlementDate = contractItem.SecuritySettleDate;
                        calcItem.Rate = contractItem.RebateRate;
                        calcItem.RecallPortionQuantity = (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                        calcItem.RecallPortionAmount = (contractItem.Quantity - contractItem.QuantityOnRecallOpen) * calcItem.Price;
                        calcItem.PortionType = PortionType.Full;
                        calcItem.RecallDate = DateTime.Today;
                        calcItem.BuyInDate = DateCalculator.Default.GetBusinessDayByBusinessDays(DateTime.Today, 3);
                        calcItem.Reason = SL_RecallReason.SD;
                        calcItem.Flag = SL_RecallIndicator.T;
                        calcItem.SubmissionType = StatusDetail.Pending;

                        if (quantityToReturn < suggestionRecall)
                        {
                            if ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) >
                                (suggestionRecall - quantityToReturn))
                            {
                                //partial
                                calcItem.RecallPortionQuantity = (suggestionRecall - quantityToReturn);
                                calcItem.RecallPortionAmount = (suggestionRecall - quantityToReturn) * calcItem.Price;
                                calcItem.PortionType = PortionType.Partial;

                                quantityToReturn += (suggestionRecall - quantityToReturn);
                            }
                            else
                            {
                                calcItem.RecallPortionQuantity = (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                                calcItem.RecallPortionAmount = ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) *
                                                          calcItem.Price);
                                calcItem.PortionType = PortionType.Full;

                                quantityToReturn += (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                            }
                        }

                        if (calcItem.Quantity > 0)
                        {
                            recallList.Add(calcItem);
                            id += 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
                }
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationRecall.cshtml", recallList);
        }

        public PartialViewResult Load_ProfitCenterByContract(IEnumerable<SL_ContractExtendedProjection> items)
        {
            List<ProfitCenterChangeModel> pcList = new List<ProfitCenterChangeModel>();

            try
            {
                int id = 1;

                if (items != null)
                {
                    foreach (SL_ContractExtendedProjection item in items)
                    {
                        try
                        {
                            pcList.Add(ProfitCenterChangeModel.Convert(id, item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractProfitChange.cshtml", pcList);
        }

        public PartialViewResult Load_ProfitCenterByCOntractBreakOut(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<ProfitCenterChangeModel> pcList = new List<ProfitCenterChangeModel>();

            try
            {
                int id = 1;

                if (items != null)
                {
                    foreach (SL_ContractBreakOutExtendedProjection item in items)
                    {
                        try
                        {
                            pcList.Add(ProfitCenterChangeModel.Convert(id, item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractProfitChange.cshtml", pcList);
        }

        public PartialViewResult Load_ContractUpdate(IEnumerable<SL_ContractExtendedProjection> items)
        {
            List<ContractUpdateModel> contractUpdateList = new List<ContractUpdateModel>();

            try
            {
                int id = 1;

                string batchCode = DataSystemValues.LoadSystemValue("ContractUpdateBatchCode", "A");

                if (items != null)
                {
                    foreach (SL_ContractExtendedProjection item in items)
                    {
                        try
                        {
                            contractUpdateList.Add(ContractUpdateModel.Convert(id, batchCode, item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractUpdateChange.cshtml", contractUpdateList);
        }

        public PartialViewResult Load_ContractBreakOutUpdate(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<ContractUpdateModel> contractUpdateList = new List<ContractUpdateModel>();

            try
            {
                int id = 1;

                string batchCode = DataSystemValues.LoadSystemValue("ContractUpdateBatchCode", "A");

                if (items != null)
                {
                    foreach (SL_ContractBreakOutExtendedProjection item in items)
                    {
                        try
                        {
                            contractUpdateList.Add(ContractUpdateModel.Convert(id, batchCode, item));
                            id++;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractUpdateChange.cshtml", contractUpdateList);
        }

        public PartialViewResult Load_Callback(IEnumerable<SL_ContractExtendedProjection> items)
        {
            List<CallbackModel> callbackList = new List<CallbackModel>();

            try
            {
                int id = 0;

                if (items != null)
                {
                    foreach (SL_ContractExtendedProjection item in items)
                    {
                        CallbackModel callbackModel = new CallbackModel();
                        callbackModel.ModelId = id;

                        callbackModel.EffectiveDate = item.EffectiveDate;
                        callbackModel.EntityId = item.EntityId;
                        callbackModel.ContraEntity = item.ContraEntity;
                        callbackModel.TradeType = item.TradeType;
                        callbackModel.IssueId = item.IssueId;                        
                        callbackModel.EntityId = item.EntityId;


                        callbackModel.CallbackQuantity = 0;
                        callbackModel.CallbackAmount = 0;
                        callbackModel.SubmissionType = StatusDetail.Pending;

                        callbackList.Add(callbackModel);
                        id++;
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractBorrowCallbackChange.cshtml", callbackList);
        }

        public PartialViewResult Load_CallbackByContractBreakOut(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<CallbackModel> callbackList = new List<CallbackModel>();

            int index = 0;

            try
            {
                if (items != null)
                {
                    foreach (SL_ContractBreakOutExtendedProjection item in items)
                    {
                        CallbackModel model = new CallbackModel();

                        model.ModelId = index;
                        model.Enabled = true;
                        model.EntityId = item.EntityId;
                        model.ContraEntity = item.ContraEntity;
                        model.ContractNumber = item.ContractNumber;
                        model.TradeType = item.TradeType;
                        model.IssueId = item.IssueId;
                        model.SecurityNumber = item.SecurityNumber;
                        model.Ticker = item.Ticker;
                        model.SecNumber = item.SecNumber;
                        model.Classification = item.Classification;
                        model.Quantity = (item.TradeType == TradeType.StockBorrow) ? item.BorrowQuantity : item.LoanQuantity;
                        model.Amount = (item.TradeType == TradeType.StockBorrow) ? item.BorrowAmount : item.LoanAmount;
                        model.CallbackQuantity = (item.TradeType == TradeType.StockBorrow) ? item.BorrowQuantity : item.LoanQuantity;
                        model.CallbackAmount = (item.TradeType == TradeType.StockBorrow) ? item.BorrowAmount : item.LoanAmount;
                        model.MemoInfo = "";

                        model.SubmissionType = StatusDetail.Pending;

                        callbackList.Add(model);                     
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractCallbackChange.cshtml", callbackList);
        }

        public PartialViewResult Load_CallbackByContractBreakOutByRate(IEnumerable<SL_ContractBreakOutByRateExtendedProjection> items)
        {
            List<CallbackModel> borrowCallbackList = new List<CallbackModel>();

            int index = 0;

            try
            {
                if (items != null)
                {
                    foreach (SL_ContractBreakOutByRateExtendedProjection item in items)
                    {

                        CallbackModel model = new CallbackModel();
                        model.ModelId = index;
                        model.Enabled = true;
                        model.EntityId = item.EntityId;
                        model.ContraEntity = item.ContraEntity;
                        model.ContractNumber = item.ContractNumber;
                        model.TradeType = item.TradeType;
                        model.IssueId = item.IssueId;
                        model.SecurityNumber = item.SecurityNumber;
                        model.Ticker = item.Ticker;
                        model.SecNumber = item.SecNumber;
                        model.Classification = item.Classification;
                        model.Quantity = (item.TradeType == TradeType.StockBorrow) ? item.BorrowQuantity : item.LoanQuantity;
                        model.Amount = (item.TradeType == TradeType.StockBorrow) ? item.BorrowAmount : item.LoanAmount;
                        model.CallbackQuantity = (item.TradeType == TradeType.StockBorrow) ? item.BorrowQuantity : item.LoanQuantity;
                        model.CallbackAmount = (item.TradeType == TradeType.StockBorrow) ? item.BorrowAmount : item.LoanAmount;
                        model.MemoInfo = "";

                        model.SubmissionType = StatusDetail.Pending;

                        borrowCallbackList.Add(model);
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractBorrowCallbackChange.cshtml", borrowCallbackList);
        }

        public PartialViewResult Load_BankLoanPositionByBox(IEnumerable<SL_BoxCalculationExtendedProjection> releaseItems, string element)
        {
            List<SL_BankLoanPositionExtendedProjection> bankLoanPositionList = new List<SL_BankLoanPositionExtendedProjection>();

            var banks = new List<SL_Bank>();
            var bankLoans = new List<SL_BankLoanExtendedProjection>();
            var bankLoanPositions = new List<SL_BankLoanPositionExtendedProjection>();

            try
            {
                 var entityid = releaseItems.Select(x => x.EntityId).Distinct().First();

                banks = DataBankLoan.LoadBanks(entityid);


                switch (element)
                {
                    case "PledgePullback":                        
                        releaseItems.ToList().RemoveAll(x => x.PledgePullback == 0);
                        break;

                    case "CustomerBankLoanPositionSettled":
                        releaseItems.ToList().RemoveAll(x => x.CustomerBankLoanPositionSettled == 0);
                        break;

                    case "FirmBankLoanPositionSettled":                        
                        releaseItems.ToList().RemoveAll(x => x.FirmBankLoanPositionSettled == 0);
                        break;

                    case "OtherBankLoanPositionSettled":
                        releaseItems.ToList().RemoveAll(x => x.OtherBankLoanPositionSettled == 0);
                        break;

                    case "AllBankLoanPositionSettled":
                        releaseItems.ToList().RemoveAll(x => x.CustomerBankLoanPositionSettled == 0);
                        releaseItems.ToList().RemoveAll(x => x.FirmBankLoanPositionSettled == 0);
                        releaseItems.ToList().RemoveAll(x => x.OtherBankLoanPositionSettled == 0);
                        break;
                }

                foreach (var boxItem in releaseItems)
                {
                    decimal releaseQuantity = 0;


                    switch (element)
                    {
                        case "PledgePullback":
                            releaseQuantity = boxItem.PledgePullback;                            
                            break;

                        case "CustomerBankLoanPositionSettled":
                            releaseQuantity = boxItem.CustomerBankLoanPositionSettled;
                            break;

                        case "FirmBankLoanPositionSettled":
                            releaseQuantity = boxItem.FirmBankLoanPositionSettled;
                            break;

                        case "OtherBankLoanPositionSettled":
                            releaseQuantity = boxItem.OtherBankLoanPositionSettled;
                            break;

                        case "AllBankLoanPositionSettled":
                            releaseQuantity = boxItem.CustomerBankLoanPositionSettled + boxItem.FirmBankLoanPositionSettled + boxItem.OtherBankLoanPositionSettled;
                            break;
                    }

                    foreach (var bank in banks)
                    {
                        bankLoans = DataBankLoan.LoadBankLoanExtended(DateTime.Today, entityid, bank.SLBank);


                        switch (element)
                        {
                           case "CustomerBankLoanPositionSettled":
                                bankLoans.RemoveAll(x => x.BankLoanCategory != SL_AccountCategory.RetailBankLoan);
                                break;

                            case "FirmBankLoanPositionSettled":
                                bankLoans.RemoveAll(x => x.BankLoanCategory != SL_AccountCategory.FirmBankLoan);                                
                                break;

                            case "OtherBankLoanPositionSettled":
                                bankLoans.RemoveAll(x => x.BankLoanCategory != SL_AccountCategory.OtherBankLoan);
                                break;
                        }


                        foreach (var bankLoan in bankLoans)
                        {
                            bankLoanPositions = DataBankLoan.LoadBankLoanPositions(DateTime.Today, entityid, bank.SLBank, bankLoan.LoanDate, bankLoan.AccountNumber).Where(x => x.IssueId == boxItem.IssueId).ToList();

                            foreach (var bankLoanPosition in bankLoanPositions)
                            {
                                if (bankLoanPosition.Quantity <= releaseQuantity)
                                {
                                    bankLoanPositionList.Add(bankLoanPosition);
                                    releaseQuantity -= bankLoanPosition.Quantity;
                                }
                                else
                                {
                                    bankLoanPosition.Quantity = releaseQuantity;
                                    bankLoanPosition.Amount = releaseQuantity * bankLoanPosition.Price;

                                    releaseQuantity = 0;

                                    bankLoanPositionList.Add(bankLoanPosition);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }


            return PartialView("~/Areas/CashSourcing/Views/LoanManagement/_BankLoanRelease.cshtml", bankLoanPositionList);
        }

        public PartialViewResult Load_ReturnByRecall(IEnumerable<SL_RecallExtendedProjection> items)
        {
            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();
            long id = 1;

            try
            {
                string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "E");

                string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
                string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");

                SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
                SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

                foreach (SL_RecallExtendedProjection recallItem in items)
                {
                    try
                    {
                        returnList.Add(ReturnTypeCalculationModel.Convert(id, batchCode, (recallItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan, (recallItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan, recallItem));
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationReturn.cshtml", returnList);
        }

        public PartialViewResult Load_ReturnByRecallExposure(IEnumerable<SL_RecallExposureProjection> items)
        {
            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();
            long id = 1;

            try
            {
                string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "E");

                string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
                string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");


                SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
                SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

                foreach (SL_RecallExposureProjection recallItem in items)
                {
                    try
                    {
                        returnList.Add(ReturnTypeCalculationModel.Convert(id, batchCode, (recallItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan, (recallItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan, recallItem));
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationReturn.cshtml", returnList);
        }

        public PartialViewResult Load_ReturnByContract(IEnumerable<SL_ContractExtendedProjection> items)
        {
            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();
            long id = 1;

            try
            {
                string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "");

                string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
                string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");

                SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
                SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

                foreach (SL_ContractExtendedProjection contractItem in items)
                {
                    try
                    {
                        SL_ContraEntity contraEntity = DataContraEntity.LoadContraEntity(contractItem.EntityId, contractItem.ContraEntity);


                        returnList.Add(ReturnTypeCalculationModel.Convert(  id, 
                                                                            batchCode, 
                                                                            (contractItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan, 
                                                                            (contraEntity.IsFPL == true) ? SL_DeliveryCode.PHYS : ((contractItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan), 
                                                                            contractItem));
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationReturn.cshtml", returnList);
        }

        public PartialViewResult Load_ReturnByContractBreakOutByRate(IEnumerable<SL_ContractBreakOutByRateExtendedProjection> items)
        {
            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();
            long id = 1;
            string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "");

            string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
            string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");


            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

            try
            {

                foreach (SL_ContractBreakOutByRateExtendedProjection contractItem in items)
                {
                    try
                    {
                        SL_ContraEntity contraEntity = DataContraEntity.LoadContraEntity(contractItem.EntityId, contractItem.ContraEntity);

                        returnList.Add(ReturnTypeCalculationModel.Convert(  id, 
                                                                            batchCode, 
                                                                            (contractItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan,
                                                                            (contraEntity.IsFPL == true) ? SL_DeliveryCode.PHYS : ((contractItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan),
                                                                            contractItem));
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationReturn.cshtml", returnList);
        }

        public PartialViewResult LoadMarkToMarketByContractBreakOut(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<MarkToMarketModel> markList = new List<MarkToMarketModel>();
            long id = 1;
            string batchCode = DataSystemValues.LoadSystemValue("MarkToMarketBatchCode", "A");
            
            try
            {

                foreach (SL_ContractBreakOutExtendedProjection contractItem in items)
                {
                    try
                    {
                        markList.Add(MarkToMarketModel.Convert(id, batchCode, contractItem));
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ContractMarkToMarket.cshtml", markList);
        }


        public PartialViewResult Load_ReturnByContractBreakOut(IEnumerable<SL_ContractBreakOutExtendedProjection> items)
        {
            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();
            long id = 1;
            string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "");

            string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
            string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");


            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

            try
            {

                foreach (SL_ContractBreakOutExtendedProjection contractItem in items)
                {
                    try
                    {
                        SL_ContraEntity contraEntity = DataContraEntity.LoadContraEntity(contractItem.EntityId, contractItem.ContraEntity);

                        returnList.Add(ReturnTypeCalculationModel.Convert(  id, 
                                                                            batchCode, 
                                                                            (contractItem.TradeType == TradeType.StockBorrow) ? callBackBorrow:callBackLoan,
                                                                             (contraEntity.IsFPL == true) ? SL_DeliveryCode.PHYS : ((contractItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan),
                                                                            contractItem));
                        id += 1;
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationReturn.cshtml", returnList);
        }

        public PartialViewResult Load_ReturnBySuggestion(IEnumerable<SL_BoxCalculationExtendedProjection> items, bool useTraded)
        {
            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();

            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            decimal quantityToReturn = 0;
            int id = 1;
            decimal suggestionReturn = 0;

            string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "");

            string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
            string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");

            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

            foreach (SL_BoxCalculationExtendedProjection item in items)
            {
                try
                {

                    quantityToReturn = 0;
                    suggestionReturn = 0;

                    suggestionReturn = (useTraded) ? item.SuggestionReturnTraded : item.SuggestionReturnSettled;


                    contractList = DataContracts.LoadContractsByIssue((DateTime)item.EffectiveDate,
                        (DateTime)item.EffectiveDate,
                        item.EntityId,
                        item.IssueId.ToString());

                    contractList =
                        contractList.Where(x => x.TradeType == TradeType.StockBorrow && x.IssueId == item.IssueId && x.SecuritySettleDate != DateTime.Today && x.TermDate == null)
                            .ToList();

                    foreach (
                        SL_ContractExtendedProjection contractItem in
                            contractList.OrderBy(x => x.RebateRate)
                                .Where(x => x.Quantity > 0 || x.QuantitySettled > 0))
                    {
                        SL_ContraEntity  contraEntity = DataContraEntity.LoadContraEntity(contractItem.EntityId, contractItem.ContraEntity);
                        

                        ReturnTypeCalculationModel calcItem = new ReturnTypeCalculationModel();

                        calcItem.Enabled = false;
                        calcItem.ModelId = id;
                        calcItem.GroupTitle = "[" + item.SecurityNumber + "] " + item.Ticker;
                        calcItem.EntityType = SL_EntityType.Return;
                        calcItem.Entity = contractItem.EntityId;
                        calcItem.ContraEntity = contractItem.ContraEntity;
                        calcItem.ContractNumber = contractItem.ContractNumber;
                        calcItem.TradeType = contractItem.TradeType;
                        calcItem.IssueId = contractItem.IssueId;
                        calcItem.SecurityNumber = contractItem.SecurityNumber;
                        calcItem.Ticker = contractItem.Ticker;
                        calcItem.Price = (contractItem.Amount / contractItem.Quantity);
                        calcItem.Rate = contractItem.RebateRate;
                        calcItem.Quantity = contractItem.Quantity;
                        calcItem.EffectiveDate = DateTime.Today;
                        calcItem.ValueDate = contractItem.CashSettleDate;
                        calcItem.ReturnBatchCode = batchCode;
                        calcItem.CallBack = callBackBorrow;
                        calcItem.DeliveryCode = contraEntity.IsFPL == true ? SL_DeliveryCode.PHYS : deliveryCodeBorrow;
                        calcItem.SubmissionType = StatusDetail.Pending;

                        if (quantityToReturn < suggestionReturn)
                        {
                            if ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) >
                                (suggestionReturn - quantityToReturn))
                            {
                                //partial
                                calcItem.ReturnPortionQuantity = (suggestionReturn - quantityToReturn);
                                calcItem.ReturnPortionAmount = (suggestionReturn - quantityToReturn) * calcItem.Price;
                                calcItem.PortionType = PortionType.Partial;

                                quantityToReturn += (suggestionReturn - quantityToReturn);
                            }
                            else
                            {
                                calcItem.ReturnPortionQuantity = (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                                calcItem.ReturnPortionAmount = ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) *
                                                          calcItem.Price);
                                calcItem.PortionType = PortionType.Full;

                                quantityToReturn += (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                            }
                        }

                        if (calcItem.ReturnPortionQuantity == 0)
                        {
                            calcItem.PortionType = PortionType.None;
                        }

                        returnList.Add(calcItem);
                        id += 1;
                    }

                    if (returnList.Count  == 1)
                    {
                        foreach(var returnItem in returnList)
                        {
                            returnItem.Enabled = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
                }
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationReturn.cshtml", returnList);
        }

     
        public JsonResult Load_ReturnBySuggestionByContractEntityId(string entityId, string list, string contraEntityId)
        {

            decimal quantityToReturn = 0;
            int id = 1;
            decimal suggestionReturn = 0;

            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();

            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();
 
            List<ParsedListModel> parseList = new List<ParsedListModel>();
            
            parseList = ListParsingService.GenerateList(entityId, contraEntityId, list, "");

            string batchCode = DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "");

            string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
            string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");

            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

            SL_ContraEntity contraEntity = DataContraEntity.LoadContraEntity(entityId, contraEntityId);

            foreach (ParsedListModel item in parseList)
            {
                try
                {

                    quantityToReturn = 0;
                    suggestionReturn = 0;

                    suggestionReturn = item.Quantity;

                    if (string.IsNullOrWhiteSpace(contraEntityId))
                    {
                        contractList = DataContracts.LoadContractsByIssue((DateTime)DateTime.Today,
                        (DateTime)DateTime.Today,
                        item.EntityId,
                        item.IssueId.ToString()).Where(x => x.ContraEntity == item.ContraEntityId).ToList();

                    }
                    else
                    {
                        contractList = DataContracts.LoadContractsByIssue((DateTime)DateTime.Today,
                                              (DateTime)DateTime.Today,
                                              item.EntityId,
                                              item.IssueId.ToString()).Where(x => x.ContraEntity == contraEntityId).ToList();
                    }

                    contractList =
                        contractList.Where(x => x.TradeType == TradeType.StockBorrow && x.IssueId == item.IssueId && x.SecuritySettleDate != DateTime.Today && x.TermDate == null)
                            .ToList();

                    foreach (
                        SL_ContractExtendedProjection contractItem in
                            contractList.OrderBy(x => x.RebateRate)
                                .Where(x => x.Quantity > 0 || x.QuantitySettled > 0))
                    {
                        ReturnTypeCalculationModel calcItem = new ReturnTypeCalculationModel();

                        calcItem.Enabled = false;
                        calcItem.ModelId = id;
                        calcItem.GroupTitle = "[" + item.SecurityNumber + "] " + item.Ticker;
                        calcItem.EntityType = SL_EntityType.Return;
                        calcItem.Entity = contractItem.EntityId;
                        calcItem.ContraEntity = contractItem.ContraEntity;
                        calcItem.ContractNumber = contractItem.ContractNumber;
                        calcItem.TradeType = contractItem.TradeType;
                        calcItem.IssueId = contractItem.IssueId;
                        calcItem.SecurityNumber = contractItem.SecurityNumber;
                        calcItem.Ticker = contractItem.Ticker;
                        calcItem.Price = (contractItem.Amount / contractItem.Quantity);
                        calcItem.Rate = contractItem.RebateRate;
                        calcItem.Quantity = contractItem.Quantity;
                        calcItem.EffectiveDate = DateTime.Today;
                        calcItem.ValueDate = contractItem.CashSettleDate;
                        calcItem.ReturnProfitCenter = contractItem.ProfitId;
                        calcItem.ReturnBatchCode = batchCode;
                        calcItem.CallBack = (calcItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan;
                        calcItem.DeliveryCode = ((bool)contraEntity.IsFPL) ? SL_DeliveryCode.PHYS : ((calcItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan);
                        calcItem.SubmissionType = StatusDetail.Pending;

                        if (quantityToReturn < suggestionReturn)
                        {
                            if ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) >
                                (suggestionReturn - quantityToReturn))
                            {
                                //partial
                                calcItem.ReturnPortionQuantity = (suggestionReturn - quantityToReturn);
                                calcItem.ReturnPortionAmount = (suggestionReturn - quantityToReturn) * calcItem.Price;
                                calcItem.PortionType = PortionType.Partial;

                                quantityToReturn += (suggestionReturn - quantityToReturn);
                            }
                            else
                            {
                                calcItem.ReturnPortionQuantity = (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                                calcItem.ReturnPortionAmount = ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) *
                                                          calcItem.Price);
                                calcItem.PortionType = PortionType.Full;

                                quantityToReturn += (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                            }
                        }

                        if (calcItem.ReturnPortionQuantity == 0)
                        {
                            calcItem.PortionType = PortionType.None;
                        }

                        returnList.Add(calcItem);
                        id += 1;
                    }
                }
                catch (Exception e)
                {
                    return ThrowJsonError(e);
                }
            }

            return Extended.JsonMax(returnList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Load_BUlkRateChangeBySuggestionByContractEntityId(string entityId,
         string list,
         string contraEntityId,
               TradeType tradeType,
                   decimal? rateChangeDelta,
                   DateTime effectiveDate,
                   decimal? newRate,
                   string newRateCode)
        {         
            int id = 1;
         
            List<RateChangeModel> rateChangeModelList = new List<RateChangeModel>();

            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            List<ParsedListModel> parseList = new List<ParsedListModel>();

            parseList = ListParsingService.GenerateList(entityId, contraEntityId, list, "");

            foreach (ParsedListModel item in parseList)
            {
                try
                {
 
                    if (string.IsNullOrWhiteSpace(contraEntityId))
                    {

                        contractList = DataContracts.LoadContractsByIssue((DateTime)DateTime.Today,
                        (DateTime)DateTime.Today,
                        item.EntityId,
                        item.IssueId.ToString()).Where(x => x.ContraEntity.ToUpper() == item.ContraEntityId.ToUpper()).ToList();

                    }
                    else
                    {
                        contractList = DataContracts.LoadContractsByIssue((DateTime)DateTime.Today,
                                              (DateTime)DateTime.Today,
                                              item.EntityId,
                                              item.IssueId.ToString()).Where(x => x.ContraEntity.ToUpper() == contraEntityId.ToUpper()).ToList();
                    }

                    contractList =
                    contractList.Where(x => x.TradeType == tradeType && x.IssueId == item.IssueId)
                        .ToList();

                    foreach (
                        SL_ContractExtendedProjection contractItem in
                            contractList.OrderBy(x => x.RebateRate)
                                .Where(x => x.Quantity > 0 || x.QuantitySettled > 0))
                    {
                        RateChangeModel calcItem = new RateChangeModel();

                        calcItem = RateChangeModel.Convert(id, "", contractItem);

                        if (item.RebateRate != null)
                        {
                            calcItem.NewRate = (decimal)item.RebateRate;
                            calcItem.NewRateCode = "";
                        }
                        else
                        {
                            if (rateChangeDelta != null)
                            {
                                calcItem.NewRate = calcItem.OldRate + (decimal)rateChangeDelta;
                            }

                            if (newRate != null)
                            {
                                calcItem.NewRate = (decimal)newRate;
                                calcItem.NewRateCode = newRateCode;
                            }
                        }

                        if ((calcItem.SecurityLoc == "US") || (calcItem.SecurityLoc == ""))
                        {
                            calcItem.NewIncomeAmount = SLTradeCalculator.CalculateIncome(calcItem.TradeType, SLTradeCalculator.Locale.Domestic, calcItem.NewRate, calcItem.Amount, calcItem.CostOfFunds, calcItem.CollateralFlag);
                        }
                        else
                        {
                            calcItem.NewIncomeAmount = SLTradeCalculator.CalculateIncome(calcItem.TradeType, SLTradeCalculator.Locale.International, calcItem.NewRate, calcItem.Amount, calcItem.CostOfFunds, calcItem.CollateralFlag);

                        }

                        rateChangeModelList.Add(calcItem);
                        id += 1;
                    }
                }
                catch (Exception e)
                {
                    return ThrowJsonError(e);
                }
            }

            return Extended.JsonMax(rateChangeModelList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Load_ReturnCallbackBySuggestionByContractEntityId(string entityId, 
            string list, 
            string contraEntityId,
            TradeType tradeType,
            SL_EntityType entityType,
            string batchCodeOverride,
            ReturnQuantityTypeEnum quantityType,
            bool alllocateCallback)
        {

            decimal quantityToReturn = 0;
            int id = 1;
            decimal suggestionReturn = 0;

            List<ReturnTypeCalculationModel> returnList = new List<ReturnTypeCalculationModel>();

            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            List<ParsedListModel> parseList = new List<ParsedListModel>();

            parseList = ListParsingService.GenerateList(entityId, contraEntityId, list, "");

            string batchCode = string.IsNullOrWhiteSpace(batchCodeOverride) ? DataSystemValues.LoadSystemValue("TradingReturnBatchCode", "") : batchCodeOverride;

            string callBackBorrow = DataSystemValues.LoadSystemValue("ReturnBorrowCallback", "L");
            string callBackLoan = DataSystemValues.LoadSystemValue("ReturnLoanCallback", "");

            SL_DeliveryCode deliveryCodeBorrow = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnBorrowDeliveryCode", "CCF"));
            SL_DeliveryCode deliveryCodeLoan = DataSystemValues.LoadDeliveryCodeHelper(DataSystemValues.LoadSystemValue("TradingReturnLoanDeliveryCode", "CCF"));

            foreach (ParsedListModel item in parseList)
            {
                try
                {
                    quantityToReturn = 0;
                    suggestionReturn = 0;

                    suggestionReturn = item.Quantity;

                    if (string.IsNullOrWhiteSpace(contraEntityId))
                    {
                        
                        contractList = DataContracts.LoadContractsByIssue((DateTime)DateTime.Today,
                        (DateTime)DateTime.Today,
                        item.EntityId,
                        item.IssueId.ToString()).Where(x => x.ContraEntity.ToUpper() == item.ContraEntityId.ToUpper()).ToList();

                    }
                    else
                    {
                        contractList = DataContracts.LoadContractsByIssue((DateTime)DateTime.Today,
                                              (DateTime)DateTime.Today,
                                              item.EntityId,
                                              item.IssueId.ToString()).Where(x => x.ContraEntity.ToUpper() == contraEntityId.ToUpper()).ToList();
                    }

                        contractList =
                        contractList.Where(x => x.TradeType == tradeType && x.IssueId == item.IssueId && x.SecuritySettleDate != DateTime.Today && x.TermDate == null)
                            .ToList();

                        
                    if (quantityType == ReturnQuantityTypeEnum.StartOfDay)

                    {
                        foreach (
                           SL_ContractExtendedProjection contractItem in
                               contractList.OrderBy(x => x.RebateRate)
                                   .Where(x => x.QuantityStartOfDay > 0))
                        {
                            ReturnTypeCalculationModel calcItem = new ReturnTypeCalculationModel();

                            calcItem.Enabled = false;
                            calcItem.ModelId = id;
                            calcItem.GroupTitle = "[" + item.SecurityNumber + "] " + item.Ticker;
                            calcItem.EntityType = entityType;
                            calcItem.Entity = contractItem.EntityId;
                            calcItem.ContraEntity = contractItem.ContraEntity;
                            calcItem.ContractNumber = contractItem.ContractNumber;
                            calcItem.TradeType = contractItem.TradeType;
                            calcItem.IssueId = contractItem.IssueId;
                            calcItem.SecurityNumber = contractItem.SecurityNumber;
                            calcItem.Ticker = contractItem.Ticker;
                            calcItem.Price = (contractItem.AmountStartOfDay / contractItem.QuantityStartOfDay);
                            calcItem.Rate = contractItem.RebateRate;
                            calcItem.Quantity = contractItem.QuantityStartOfDay;
                            calcItem.EffectiveDate = DateTime.Today;
                            calcItem.ValueDate = contractItem.CashSettleDate;
                            calcItem.ReturnProfitCenter = contractItem.ProfitId;
                            calcItem.ReturnBatchCode = batchCode;
                            calcItem.CallBack = (calcItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan;
                            calcItem.DeliveryCode = (calcItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan;
                            calcItem.SubmissionType = StatusDetail.Pending;

                            if (quantityToReturn < suggestionReturn)
                            {
                                if ((contractItem.QuantityStartOfDay - contractItem.QuantityOnRecallOpen) >
                                    (suggestionReturn - quantityToReturn))
                                {
                                    //partial
                                    calcItem.ReturnPortionQuantity = (suggestionReturn - quantityToReturn);
                                    calcItem.ReturnPortionAmount = (suggestionReturn - quantityToReturn) * calcItem.Price;
                                    calcItem.PortionType = PortionType.Partial;

                                    quantityToReturn += (suggestionReturn - quantityToReturn);
                                }
                                else
                                {
                                    calcItem.ReturnPortionQuantity = (contractItem.QuantityStartOfDay - contractItem.QuantityOnRecallOpen);
                                    calcItem.ReturnPortionAmount = ((contractItem.QuantityStartOfDay - contractItem.QuantityOnRecallOpen) *
                                                              calcItem.Price);
                                    calcItem.PortionType = PortionType.Full;

                                    quantityToReturn += (contractItem.QuantityStartOfDay - contractItem.QuantityOnRecallOpen);
                                }
                            }

                            if (calcItem.ReturnPortionQuantity == 0)
                            {
                                calcItem.PortionType = PortionType.None;
                            }

                            returnList.Add(calcItem);
                            id += 1;
                        }
                    }
                    else
                    {
                        foreach (
                            SL_ContractExtendedProjection contractItem in
                                contractList.OrderBy(x => x.RebateRate)
                                    .Where(x => x.Quantity > 0 || x.QuantitySettled > 0))
                        {
                            ReturnTypeCalculationModel calcItem = new ReturnTypeCalculationModel();

                            calcItem.Enabled = false;
                            calcItem.ModelId = id;
                            calcItem.GroupTitle = "[" + item.SecurityNumber + "] " + item.Ticker;
                            calcItem.EntityType = entityType;
                            calcItem.Entity = contractItem.EntityId;
                            calcItem.ContraEntity = contractItem.ContraEntity;
                            calcItem.ContractNumber = contractItem.ContractNumber;
                            calcItem.TradeType = contractItem.TradeType;
                            calcItem.IssueId = contractItem.IssueId;
                            calcItem.SecurityNumber = contractItem.SecurityNumber;
                            calcItem.Ticker = contractItem.Ticker;
                            calcItem.Price = (contractItem.Amount / contractItem.Quantity);
                            calcItem.Rate = contractItem.RebateRate;
                            calcItem.Quantity = contractItem.Quantity;
                            calcItem.EffectiveDate = DateTime.Today;
                            calcItem.ValueDate = contractItem.CashSettleDate;
                            calcItem.ReturnProfitCenter = contractItem.ProfitId;
                            calcItem.ReturnBatchCode = batchCode;
                            calcItem.CallBack = (calcItem.TradeType == TradeType.StockBorrow) ? callBackBorrow : callBackLoan;
                            calcItem.DeliveryCode = (calcItem.TradeType == TradeType.StockBorrow) ? deliveryCodeBorrow : deliveryCodeLoan;
                            calcItem.SubmissionType = StatusDetail.Pending;

                            if (quantityToReturn < suggestionReturn)
                            {
                                if ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) >
                                    (suggestionReturn - quantityToReturn))
                                {
                                    //partial
                                    calcItem.ReturnPortionQuantity = (suggestionReturn - quantityToReturn);
                                    calcItem.ReturnPortionAmount = (suggestionReturn - quantityToReturn) * calcItem.Price;
                                    calcItem.PortionType = PortionType.Partial;

                                    quantityToReturn += (suggestionReturn - quantityToReturn);
                                }
                                else
                                {
                                    calcItem.ReturnPortionQuantity = (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                                    calcItem.ReturnPortionAmount = ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) *
                                                              calcItem.Price);
                                    calcItem.PortionType = PortionType.Full;

                                    quantityToReturn += (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                                }
                            }

                            if (calcItem.ReturnPortionQuantity == 0)
                            {
                                calcItem.PortionType = PortionType.None;
                            }

                            returnList.Add(calcItem);
                            id += 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    return ThrowJsonError(e);
                }
            }

            return Extended.JsonMax(returnList, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Load_ContractByFee(IEnumerable<SL_ContractExtendedProjection> list)
        {
            List<FeeChangeModel> feeChangeList = list.Select(x => new FeeChangeModel()
            {
                EffectiveDate = x.EffectiveDate,
                Enabled = false,
                Entity = x.EntityId,
                ClearingId = x.ClearingId,
                ModelId = x.SLContract,
                ContraEntity = x.ContraEntity,
                ContractNumber = x.ContractNumber,
                CollateralFlag = x.CollateralFlag,
                TradeType = x.TradeType,
                SecurityNumber = x.SecurityNumber,
                Ticker = x.Ticker,
                OldFee = x.FeeType,
                StartDate = x.SecuritySettleDate,
                StopDate = null,
                OldFeeOffSet = (decimal) x.FeeOffset,
                NewFee = "",
                NewFeeOffSet = 0            
            }).ToList();


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractAppyFee.cshtml", feeChangeList);
        }

        public PartialViewResult Load_ContractBreakOutByFee(IEnumerable<SL_ContractExtendedProjection> list)
        {
            List<FeeChangeModel> feeChangeList = list.Select(x => new FeeChangeModel()
            {
                EffectiveDate = x.EffectiveDate,
                Enabled = false,
                ModelId = x.SLContract,
                Entity = x.EntityId,
                ClearingId = x.ClearingId,
                ContraEntity = x.ContraEntity,
                ContractNumber = x.ContractNumber,
                CollateralFlag = x.CollateralFlag,
                TradeType = x.TradeType,
                IssueId = x.IssueId,
                SecurityNumber = x.SecurityNumber,
                Ticker = x.Ticker,
                StartDate = x.SecuritySettleDate,
                StopDate = null,
                OldFee = x.FeeType,
                OldFeeOffSet = (decimal)x.FeeOffset,
                NewFee = "",
                NewFeeOffSet = 0
            }).ToList();


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContractAppyFee.cshtml", feeChangeList);            
        }

        public JsonResult SuggestProfitCenter(IEnumerable<ProfitCenterChangeModel> items)
        {
            var list = DataPCMatrix.LoadPCMatrix(items.Select(x => x.Entity).Distinct().First());

            if (items != null)
            {
                foreach (ProfitCenterChangeModel model in items)
                {
                    if (model.Enabled)
                    {
                        model.NewProfitCenter = PcMatrixService.SetDefaults(model, list);

                        model.OldProfitCenter = (string.IsNullOrWhiteSpace(model.OldProfitCenter)) ? "" : model.OldProfitCenter;

                        if (!model.OldProfitCenter.Equals(model.NewProfitCenter))
                        {
                            model.SubmissionType = StatusDetail.HeldLocal;
                        }
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FeeModel_Update([DataSourceRequest] DataSourceRequest request, FeeChangeModel item)
        {
            ModelState.Clear();

            item.Enabled = true;

            if ((item.OldFee != item.NewFee) || (item.OldFeeOffSet != item.NewFeeOffSet))
            {
                item.SubmissionType = StatusDetail.HeldLocal;
            } 


            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReturnModel_Update([DataSourceRequest] DataSourceRequest request, ReturnTypeCalculationModel item)
        {
            ModelState.Clear();

            item.ReturnBatchCode = string.IsNullOrWhiteSpace(item.ReturnBatchCode) ? "" : item.ReturnBatchCode.ToUpper();

            if (item.ReturnPortionQuantity >= item.Quantity)
            {
                item.ReturnPortionQuantity = item.Quantity;
                item.PortionType = PortionType.Full;
                item.ReturnPortionAmount = item.ReturnPortionQuantity * item.Price;
            }
            else
            {
                item.PortionType = PortionType.Partial;
                item.ReturnPortionAmount = item.ReturnPortionQuantity * item.Price;
            }

            if (item.ReturnPortionQuantity == 0)
            {
                item.PortionType = PortionType.None;
                item.Enabled = false;
                item.SubmissionType = StatusDetail.Pending;
            }
            else
            {
                item.Enabled = true;
                item.SubmissionType = StatusDetail.HeldLocal;
            }



            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ContractModel_Destroy([DataSourceRequest] DataSourceRequest request, ContractProjectionWrapperModel item)
        {
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult ContractModel_Update([DataSourceRequest] DataSourceRequest request, ContractProjectionWrapperModel item)
        {
            item.Contract.IncomeAmount = (item.Contract.Amount * (item.Contract.RebateRate / 100) / 360) * -1;

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult RateChangeModel_Update([DataSourceRequest] DataSourceRequest request, RateChangeModel item)
        {
            if (item.NewUseHouseRate)
            {
                SL_ContraEntity houseRate = DataContraEntity.LoadContraEntity(item.Entity, item.ContraEntity);

                if (item.TradeType == TradeType.StockBorrow)
                {
                    item.NewRate = Convert.ToDecimal(houseRate.STKBorrowRate);
                }
                else
                {
                    item.NewRate = Convert.ToDecimal(houseRate.STKLoanRate);
                }

                item.NewRateCode = "T";
            }
            else
            {
                if (item.NewRate >= 0)
                {
                    item.NewRateCode = "";
                }
                else
                {
                    item.NewRateCode = "N";
                }
            }

            if (item.NewRateCode == null)
            {
                item.NewRateCode = "";
            }

            decimal newIncome = 0;

            item.SecurityLoc = string.IsNullOrWhiteSpace(item.SecurityLoc) ? "" : item.SecurityLoc;

            if(item.SecurityLoc.ToUpper().Equals("") || item.SecurityLoc.ToUpper().Equals("US"))
            { 
                newIncome = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.Domestic, item.NewRate, item.Amount, item.CostOfFunds, item.CollateralFlag);
            }
            else
            {
                newIncome = SLTradeCalculator.CalculateIncome(item.TradeType, SLTradeCalculator.Locale.International, item.NewRate, item.Amount, item.CostOfFunds, item.CollateralFlag);
            }


            item.Enabled = true;
            item.NewIncomeAmount = newIncome;
            item.SubmissionType = StatusDetail.HeldLocal;

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult ContractUpdateModel_Update([DataSourceRequest] DataSourceRequest request, ContractUpdateModel item)
        {

            ModelState.Clear();

            if (ModelState.IsValid)
            {
                item.Enabled = true;
                item.MemoInfo = "";
                item.SubmissionType = StatusDetail.HeldLocal;
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProfitIdChangeModel_Update([DataSourceRequest] DataSourceRequest request, ProfitCenterChangeModel item)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                item.NewProfitCenter = item.NewProfitCenter.ToUpper();
                item.Enabled = true;

                item.SubmissionType = StatusDetail.HeldLocal;
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult MemoSegModel_Update([DataSourceRequest] DataSourceRequest request, MemoSegUpdateModel item)
        {
            ModelState.Clear();

            if (ModelState.IsValid)
            {                
                item.Enabled = true;

                item.SubmissionType = StatusDetail.HeldLocal;
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult RecallModel_Update([DataSourceRequest] DataSourceRequest request, RecallTypeCalculationModel item)
        {
            ModelState.Clear();

            if (item.RecallPortionQuantity == 0)
            {
                item.RecallPortionQuantity = 0;
                item.PortionType = PortionType.None;
                item.RecallPortionAmount = 0;
                item.Enabled = false;
                item.SubmissionType = StatusDetail.Pending;
            }
            else if (item.RecallPortionQuantity >= item.Quantity)
            {
                item.RecallPortionQuantity = item.Quantity;
                item.PortionType = PortionType.Full;
                item.RecallPortionAmount = item.RecallPortionQuantity * item.Price;
                item.Enabled = true;
                item.SubmissionType = StatusDetail.HeldLocal;
            }
            else if (item.RecallPortionQuantity > 0)
            {
                item.PortionType = PortionType.Partial;
                item.RecallPortionAmount = item.RecallPortionQuantity * item.Price;
                item.Enabled = true;
                item.SubmissionType = StatusDetail.HeldLocal;
            }
            else
            {
                item.Enabled = false;
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult ContractCancelModel_Update([DataSourceRequest] DataSourceRequest request, ContractCancelModel item)
        {
            ModelState.Clear();           

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult ProcessContractUpdates(IEnumerable<ContractUpdateModel> items)
        {
            if (items != null)
            {
                List<SL_ContractBreakOutExtendedProjection> contractBreakOutList = new List<SL_ContractBreakOutExtendedProjection>();

                foreach (var item in items.Where(x => x.Enabled))
                {
                    var _contractBreakOut = ContractCalculator.ConvertToNetView(DataContracts.LoadContract(DateTime.Today, item.EntityId, item.TradeType, item.ContractNumber, item.ContraEntity), new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>());

                    _contractBreakOut.ActivityFlag = SL_ActivityFlag.Processing;
                    _contractBreakOut.EntityType = SL_EntityType.ContractUpdate;

                    contractBreakOutList.Add(_contractBreakOut);
                }

                RealTime.Broadcast.ContractBreakOut(contractBreakOutList);

                foreach (ContractUpdateModel model in items)
                {
                    if (model.Enabled)
                    {
                        try
                        {
                            if (model.SubmissionType == StatusDetail.HeldLocal)
                            {
                                bool success = false;
                                
                                success = DataExternalOperations.UpdateContract(
                                  DataContracts.LoadContract(model.SLContract),
                                    model.BatchCode,
                                    model.NewSettlementDate,
                                    model.NewMarkParameterId,
                                    Convert.ToDouble(model.NewMark),
                                    Convert.ToDecimal(model.NewDivRate),
                                    model.NewIncomeTracked,
                                    model.NewTermDate,
                                    model.NewComment,
                                    model.NewCallable,
                                    model.NewExpectedEndDate);

                                if (success)
                                {
                                    model.SubmissionType = StatusDetail.Approved;
                                    model.MemoInfo = string.Format(message, model.ExecutingSystem.ToString());
                                }
                                else
                                {
                                    model.SubmissionType = StatusDetail.Rejected;
                                }
                            }
                            else
                            {
                                model.Enabled = false;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.Enabled = false;
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
            }



            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessRecalls(IEnumerable<RecallTypeCalculationModel> items)
        {
            if (items != null)
            {
                var issueList = DataIssue.LoadIssueListById(items.Select(x => x.IssueId).Distinct().ToList());

                foreach (RecallTypeCalculationModel model in items)
                {
                    if (model.Enabled)
                    {
                        try
                        {
                            if (model.RecallPortionQuantity > 0)
                            {
                                bool success = false;

                                if (model.IsPendingRecall)
                                {
                                    try
                                    {

                                        SL_Recall recall = new SL_Recall()
                                        {
                                            Activity = "",
                                            EffectiveDate = DateTime.Today,
                                            EntityId = model.Entity,
                                            BuyInDate = DateCalculator.Default.GetBusinessDayByBusinessDays(DateTime.Today, 3),
                                            Comment = "Pending Recall",
                                            ContractNumber = model.ContractNumber,
                                            ContraEntity = model.ContraEntity,
                                            ExecutionSystemType = model.ExecutingSystem,
                                            IssueId = model.IssueId,
                                            QuantityRecalled = model.RecallPortionQuantity,
                                            RecallDate = DateTime.Today,
                                            RecallFlag = SL_RecallIndicator.T,
                                            RecallNumber = "Pending Recall",
                                            RecallReason = model.Reason,
                                            Status = SL_RecallStatus.PEND,
                                            TradeType = model.TradeType,
                                            SecurityNumber = issueList.Where(x => x.IssueId == model.IssueId).First().Cusip.Trim()
                                        };

                                        DataRecalls.AddRecall(recall);

                                        SL_Activity pendingRecall = DataTransformation.TransformPendingRecallChange(
                                            model.Entity,
                                            model.ContraEntity,
                                            model.TradeType,
                                            model.ContractNumber,
                                            model.RecallPortionQuantity,
                                            model.RecallPortionAmount,
                                            model.IssueId,
                                            SessionService.SecurityContext);

                                        DataActivity.AddActivity(pendingRecall);

                                        model.SubmissionType = StatusDetail.Approved;
                                        model.MemoInfo = "Created Pending Recall.";
                                    }
                                    catch
                                    {
                                        model.SubmissionType = StatusDetail.Rejected;
                                    }
                                }
                                else
                                {
                                    success = DataExternalOperations.AddNewRecall(model.Entity,
                                        model.ContraEntity,
                                        model.TradeType,
                                        model.ContractNumber,
                                        (model.TradeType == TradeType.StockBorrow) ? DateTime.Today.ToString("yyyyMMddhhffss") : "",
                                        model.IssueId,
                                        model.RecallPortionQuantity,
                                        model.RecallPortionAmount,
                                        model.RecallDate,
                                        model.BuyInDate,
                                        model.Reason,
                                        model.Flag);

                                    if (success)
                                    {
                                        model.SubmissionType = StatusDetail.Approved;
                                        model.MemoInfo = string.Format(message, model.ExecutingSystem.ToString());
                                    }
                                    else
                                    {
                                        model.SubmissionType = StatusDetail.Rejected;
                                    }
                                }
                            }
                            else
                            {
                                model.Enabled = false;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.Enabled = false;
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessPendingRecalls(IEnumerable<SL_RecallExtendedProjection> items)
        {
            List<SL_RecallExtendedProjection> recallList = new List<SL_RecallExtendedProjection>();

            DateTime _EffectiveDate = (DateTime)items.Select(x => x.EffectiveDate).Distinct().First();
            string _EntityId = items.Select(x => x.EntityId).Distinct().First();


            if (items != null)
            {
                recallList = DataRecalls.LoadRecallsExtended(_EffectiveDate, _EntityId);

                foreach (SL_RecallExtendedProjection model in items)
                {
                    var _recallItem = recallList.Where(x => x.SLRecall == model.SLRecall).First();

                    if ((_recallItem.IsPendingRecall) && (_recallItem.Status == SL_RecallStatus.PEND))
                    {
                        try
                        {
                            if (model.QuantityRecalled > 0)
                            {
                                try
                                {
                                    DataRecalls.SendPendingRecall(model);

                                    recallList.Add(model);
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return Extended.JsonMax(recallList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessPendingRecallsExposure(IEnumerable<SL_RecallExposureProjection> items)
        {
            List<SL_RecallExposureProjection> recallExposureList = new List<SL_RecallExposureProjection>();
            List<SL_RecallExtendedProjection> recallList = new List<SL_RecallExtendedProjection>();

            if (items != null)
            {
                recallList = DataRecalls.LoadRecallsExtended(
                    (DateTime)items.Select(x => x.EffectiveDate).Distinct().First(),
                    items.Select(x => x.EntityId).Distinct().First(),
                    SessionService.SecurityContext);

                foreach (SL_RecallExposureProjection model in items)
                {
                    if (model.IsPendingRecall)
                    {
                        var recall = recallList.Where(x => x.SLRecall == model.SLRecall).First();

                        if ((recall.TradeType == TradeType.StockBorrow) && (recall.Status == SL_RecallStatus.PEND))
                        {
                            if (model.BorrowQuantityRecalled > 0)
                            {
                                try
                                {
                                    DataRecalls.SendPendingRecall(recall);
                                }
                                catch
                                {
                                    model.BorrowStatus = SL_RecallStatus.OPEN;
                                }

                                recallExposureList.Add(model);
                            }
                        }
                        else if ((model.TradeType == TradeType.StockLoan) && (model.LoanStatus == SL_RecallStatus.PEND))
                        {
                            if (model.LoanQuantityRecalled > 0)
                            {
                                try
                                {
                                    DataRecalls.SendPendingRecall(recall);
                                }
                                catch
                                {
                                    model.BorrowStatus = SL_RecallStatus.OPEN;
                                }

                                recallExposureList.Add(model);
                            }
                        }
                    }
                }
            }

            return Extended.JsonMax(recallExposureList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessCallback(IEnumerable<CallbackModel> items)
        {
            if (items != null)
            {
                foreach (CallbackModel model in items)
                {
                    if (model.Enabled)
                    {
                        try
                        {
                            bool success = false;


                            var _model = CallbackModel.Convert(model);
                            _model.EffectiveDate = DateTime.Today;
                            _model.MemoInfo = "";

                            if (_model.TradeType == TradeType.StockBorrow)
                            {
                                DataCallback.AddCallBack(_model);
                                success = DataExternalOperations.AddNewBorrowCallback(_model);
                            }
                            else
                            {
                                success = DataExternalOperations.AddNewLoanCallback(_model);
                            }
                           

                            if (success)
                            {                                
                                model.SubmissionType = StatusDetail.Approved;
                                model.MemoInfo = "Msg submitted to Loanet.";
                            }
                            else
                            {
                                model.SubmissionType = StatusDetail.Rejected;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessMarkToMarket(IEnumerable<MarkToMarketModel> items)
        {
            if (items != null)
            {
                foreach (MarkToMarketModel model in items)
                {
                    if (model.Enabled)
                    {
                        try
                        {
                            bool success = false;

                            success = DataExternalOperations.AddMarkToMarket(model);

                            if (success)
                            {
                                model.SubmissionType = StatusDetail.Approved;
                                model.MemoInfo = "Msg submitted to Loanet.";
                            }
                            else
                            {
                                model.SubmissionType = StatusDetail.Rejected;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MarkToMarketChangeModel_Update([DataSourceRequest] DataSourceRequest request, MarkToMarketModel model)
        {
            decimal newPrice = model.NewPrice;
            double? nullable = new double?(double.Parse(newPrice.ToString()));
            decimal quantity = model.Quantity;
            newPrice = model.Mark;
            decimal newAmount = SLTradeCalculator.CalculateMoney(nullable, quantity, double.Parse(newPrice.ToString()));
            model.Enabled = true;
            model.NewContractAmount = newAmount;

            return Extended.JsonMax(new object[] { model }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CallbackChangeModel_Update([DataSourceRequest] DataSourceRequest request, CallbackModel item)
        {
            item.CallbackAmount = item.CallbackQuantity * (item.Amount / item.Quantity);
            item.Enabled = true;
            item.MemoInfo = "";
            item.SubmissionType = StatusDetail.HeldLocal;

            return Extended.JsonMax(new object[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PendingActionModel_Update([DataSourceRequest] DataSourceRequest request, PendingActionModel callBackItem)
        {
            callBackItem.PendAmount = callBackItem.PendQuantity * callBackItem.Price;

            callBackItem.Quantity = callBackItem.Quantity - callBackItem.PendQuantity;
            callBackItem.Amount = callBackItem.Amount - callBackItem.PendAmount;

            return Extended.JsonMax(new object[] { callBackItem }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult MarkToMarketChangeModel_Destroy([DataSourceRequest] DataSourceRequest request, MarkToMarketModel model)
        {
            return Extended.JsonMax(new { model }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ProcessProfitId(IEnumerable<ProfitCenterChangeModel> items)
        {
            if (items != null)
            {
                List<SL_ContractBreakOutExtendedProjection> contractBreakOutList = new List<SL_ContractBreakOutExtendedProjection>();

                foreach (var item in items.Where(x => x.Enabled))
                {
                    var _contractBreakOut = ContractCalculator.ConvertToNetView(DataContracts.LoadContract(item.EffectiveDate, item.Entity, item.TradeType, item.ContractNumber, item.ContraEntity), new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>());

                    _contractBreakOut.ActivityFlag = SL_ActivityFlag.Processing;
                    _contractBreakOut.EntityType = SL_EntityType.PoolCode;

                    contractBreakOutList.Add(_contractBreakOut);
                }

                RealTime.Broadcast.ContractBreakOut(contractBreakOutList);

                foreach (ProfitCenterChangeModel model in items)
                {
                    if (model.Enabled)
                    {
                        try
                        {
                            bool success = false;

                            success = DataExternalOperations.AddNewProfitCenter(model.Entity,
                                model.ContraEntity,
                                model.ContractNumber,
                                model.TradeType,
                                model.NewProfitCenter);

                            if (success)
                            {
                                model.SubmissionType = StatusDetail.Approved;
                                model.MemoInfo = "Msg submitted to Loanet.";
                            }
                            else
                            {
                                model.SubmissionType = StatusDetail.Rejected;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessFee(IEnumerable<FeeChangeModel> items)
        {
            var entityId = items.Select(x => x.Entity).Distinct().First();
            var rateList = new List<RateChangeModel>();
            var id = 0;

            var pendingAddFundingRateContracts = new List<SL_FundingRateContract>();
            var pendingUpdateFundingRateContracts = new List<SL_FundingRateContract>();
            var pendingActivities = new List<SL_Activity>();

            var fundingRates = new List<SL_FeeType>();
            var fundingRateContracts = new List<SL_FundingRateContract>();

            List<SL_ContractBreakOutExtendedProjection> contractList = new List<SL_ContractBreakOutExtendedProjection>();

            List<SL_ContractBreakOutExtendedProjection> _ContractBreakOut = new List<SL_ContractBreakOutExtendedProjection>();

            foreach (var item in items.Select(x => x.Entity).Distinct())
            {
                fundingRates.AddRange(DataFeeTypes.LoadFeeTypes(item));
                fundingRateContracts.AddRange(DataFundingRates.LoadFundingRateContracts(item));
                _ContractBreakOut.AddRange(DataContracts.LoadContractsBreakOut(items.Select(x => x.EffectiveDate).Distinct().First(), items.Select(x => x.EffectiveDate).Distinct().First(), item));
            }

            if (items != null)
            {
                List<SL_ContractBreakOutExtendedProjection> contractBreakOutList = new List<SL_ContractBreakOutExtendedProjection>();

                foreach (var item in items.Where(x => x.Enabled))
                {
                    var _contractBreakOut = _ContractBreakOut.Where(x => x.EffectiveDate == item.EffectiveDate && x.EntityId == item.Entity && x.ContraEntity == item.ContraEntity && x.TradeType == item.TradeType && x.ContractNumber == item.ContractNumber && x.IssueId == item.IssueId).First();

                    _contractBreakOut.ActivityFlag = SL_ActivityFlag.Processing;
                    _contractBreakOut.EntityType = SL_EntityType.Rate;

                    contractBreakOutList.Add(_contractBreakOut);
                }

                RealTime.Broadcast.ContractBreakOut(contractBreakOutList);

                foreach (FeeChangeModel model in items)
                {
                    var success = true;

                    if (model.Enabled)
                    {
                        try
                        {
                            try
                            {
                                SL_FundingRateContract newFundingRateItem = new SL_FundingRateContract();

                                newFundingRateItem.EntityId = model.Entity;
                                newFundingRateItem.ContraEntity = model.ContraEntity;
                                newFundingRateItem.TradeType = model.TradeType;
                                newFundingRateItem.IssueId = model.IssueId;
                                newFundingRateItem.ContractNumber = model.ContractNumber;
                                newFundingRateItem.FeeId = int.Parse(fundingRates.Where(x => x.EntityId == model.Entity && x.Fee.Equals(string.IsNullOrWhiteSpace(model.NewFee) ? "REBATE" : model.NewFee)).First().SLFeeType.ToString());
                                newFundingRateItem.FeeOffset = model.NewFeeOffSet;
                                newFundingRateItem.StartDate = model.StartDate;
                                newFundingRateItem.StopDate = model.StopDate;

                                pendingAddFundingRateContracts.Add(newFundingRateItem);


                                SL_Activity newActivity = new SL_Activity()
                                {
                                    EffectiveDate = DateCalculator.Default.Today,
                                    EntityType = SL_EntityType.Strategy,
                                    EntityId = model.Entity,
                                    ContraEntity = model.ContraEntity,
                                    TypeId = model.ContractNumber,
                                    TradeType = model.TradeType,
                                    IssueId = model.IssueId,
                                    Quantity = 0,
                                    Amount = 0,
                                    Activity = string.Format("Applied strategy : {0}", model.NewFee),
                                    ActivityRequest = "",
                                    ActivityResponse = "",
                                    UserName = SessionService.SecurityContext.UserName,
                                    ExecutionSystemType = SL_ExecutionSystemType.LOCAL,
                                    ActivityFlag = SL_ActivityFlag.Completed,
                                    ActivityType = SL_ActivityType.Activity,
                                    ActivitySubType = SL_ActivitySubType.ContractUpdate
                                };


                                pendingActivities.Add(newActivity);

                                success = true;
                            }
                            catch (Exception)
                            {
                                success = false;
                            }

                            if (success)
                            {
                                model.SubmissionType = StatusDetail.Approved;
                                model.MemoInfo = "Fee recorded successfully.";
                            }
                            else
                            {
                                model.SubmissionType = StatusDetail.Rejected;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }

                    contractList.AddRange(_ContractBreakOut.Where(x => x.EntityId == model.Entity && x.ContractNumber == model.ContractNumber && x.IssueId == model.IssueId && x.TradeType == model.TradeType && x.ContraEntity == model.ContraEntity).ToList());
                }


                if (pendingAddFundingRateContracts.Count > 0)
                {
                    try
                    {
                        DataFundingRates.AddFundingRateContract(pendingAddFundingRateContracts);
                    }
                    catch
                    {
                        foreach (var item in pendingAddFundingRateContracts)
                        {
                            try
                            {
                                DataFundingRates.AddFundingRateContract(item);
                            }
                            catch (Exception innerE)
                            {
                                var updateModel = items.Where(x => x.Entity == item.EntityId &&
                                                        x.ContraEntity == item.ContraEntity &&
                                                        x.TradeType == item.TradeType &&
                                                        x.IssueId == item.IssueId &&
                                                        x.ContractNumber == item.ContractNumber).First();

                                updateModel.MemoInfo = innerE.Message;
                                updateModel.SubmissionType = StatusDetail.Rejected;
                            }
                        }
                    }
                }

                if (pendingUpdateFundingRateContracts.Count > 0)
                {
                    try
                    {
                        DataFundingRates.UpdateFundingRateContractRange(pendingUpdateFundingRateContracts);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            foreach (var item in pendingActivities)
                            {
                                DataActivity.AddActivity(item);
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                if (pendingActivities.Count > 0)
                {
                    try
                    {
                        DataActivity.AddActivity(pendingActivities);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            foreach (var item in pendingActivities)
                            {
                                DataActivity.AddActivity(item);
                            }
                        }
                        catch
                        {

                        }
                    }
                }             
            }

            var refreshContracts = new List<SL_ContractBreakOutExtendedProjection>();

            foreach (var item in items.Select(x => x.Entity).Distinct())
            {
                refreshContracts.AddRange(DataContracts.LoadContractsBreakOut(items.Select(x => x.EffectiveDate).Distinct().First(), items.Select(x => x.EffectiveDate).Distinct().First(), item));
            }

            var rateContracts = new List<SL_ContractBreakOutExtendedProjection>();

            foreach (var item in refreshContracts)
            {
                if (contractList.Any( x => x.SLContract == item.SLContract))
                {
                    rateContracts.Add(item);
                }
            }


            foreach (var item in rateContracts)
            {
                try
                {
                    var model = items.Where(x => x.Entity == item.EntityId && x.IssueId == item.IssueId && x.ContraEntity == item.ContraEntity && x.ContractNumber == item.ContractNumber && x.TradeType == item.TradeType).First();

                    if (!model.SuppressRateChange)
                    {
                        rateList.Add(RateChangeModel.ConvertForFee(id, "", item));
                        id++;
                    }
                }
                catch
                {

                }

                item.ContractFlag = "Pending";
            }

            DataTransformation.ProcessContractMessage(rateContracts);

            if (rateList.Any())
            {
                ProcessRateChanges(rateList);
            }

             
            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessRateChangesBK(IEnumerable<RateChangeModel> items)
        {
            bool success = false;

            var contraRateSwing = DataContraRateSwing.LoadContraRateSwing(items.First().Entity);
            var comapnyIds = DataEntity.LoadEntities();

            foreach (RateChangeModel model in items)
            {
                if (model.Enabled)
                {
                    try
                    {
                        if (model.ContraRateSwing)
                        {
                            var swingCustodian = contraRateSwing.Where(x => x.ContraEntity == model.ContraEntity).First().SwingEntity;

                            var swingEntity = comapnyIds.Where(x => x.Custodian == swingCustodian).First().CompanyId.ToString();

                            var contracts = DataContracts.LoadContractsByIssue(model.EffectiveDate, model.EffectiveDate, swingEntity, model.IssueId.ToString());

                            //handle one to one
                            if (contracts.Any(x => x.EffectiveDate == model.EffectiveDate && x.IssueId == model.IssueId && x.ContraEntity == model.ContraEntity && x.TradeType != model.TradeType && x.Quantity == model.Quantity))
                            {
                                var _Contract = contracts.Where(x => x.EffectiveDate == model.EffectiveDate && x.IssueId == model.IssueId && x.ContraEntity == model.ContraEntity && x.TradeType != model.TradeType && x.Quantity == model.Quantity).First();

                                var swingModel = RateChangeModel.Convert(1, "", _Contract);

                                swingModel.NewRate = model.NewRate;
                                swingModel.NewRateCode = model.NewRateCode;

                                DataExternalOperations.AddRateChange(swingModel);
                            }
                            else
                            {
                                var _Contracts = contracts.Where(x => x.EffectiveDate == model.EffectiveDate && x.IssueId == model.IssueId && x.ContraEntity == model.ContraEntity && x.TradeType != model.TradeType);

                                foreach (var item in _Contracts)
                                {
                                    var swingModel = RateChangeModel.Convert(1, "", item);

                                    swingModel.NewRate = model.NewRate;
                                    swingModel.NewRateCode = model.NewRateCode;

                                    DataExternalOperations.AddRateChange(swingModel);
                                }
                            }
                        }

                        ValidationHelper.ValidateRateChangeRequest(model);

                        success = DataExternalOperations.AddRateChange(model);

                        if (success)
                        {
                            model.SubmissionType = StatusDetail.Approved;
                            model.MemoInfo = string.Format(message, model.ExecutingSystem.ToString());
                        }
                        else
                        {
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                    catch (Exception e)
                    {
                        model.MemoInfo = e.Message;
                        model.SubmissionType = StatusDetail.Rejected;
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessRateChanges(IEnumerable<RateChangeModel> items)
        {
            bool success = false;

            List<SL_ContraRateSwing> contraRateSwing = new List<SL_ContraRateSwing>();

            try
            {
                contraRateSwing = DataContraRateSwing.LoadContraRateSwing(items.First().Entity);
            }
            catch (Exception e)
            {
                contraRateSwing = new List<SL_ContraRateSwing>();                    
            }

            var comapnyIds = DataEntity.LoadEntities();
            List<RateChangeModel> rateChangeModelList = new List<RateChangeModel>();
            List<SL_ContractBreakOutExtendedProjection> contractBreakOutList = new List<SL_ContractBreakOutExtendedProjection>();

            if (items.Count() > 20)
            {
                List<SL_ContractBreakOutExtendedProjection> m_ContractBreakOutList = new List<SL_ContractBreakOutExtendedProjection>();

                var key = items.GroupBy(x => new { x.EffectiveDate, x.Entity }).ToList();

                foreach (var keyItem in key)
                {
                    m_ContractBreakOutList.AddRange(DataContracts.LoadContractsBreakOut(keyItem.Key.EffectiveDate, keyItem.Key.EffectiveDate, keyItem.Key.Entity));
                }

                var returnKey = items.GroupBy(x => new { x.EffectiveDate, x.Entity, x.TradeType, x.ContractNumber, x.ContraEntity }).ToList();

                foreach (var returnKeyItem in returnKey)
                {
                    var _contractBreakOut = m_ContractBreakOutList.Where(x => x.EffectiveDate == returnKeyItem.Key.EffectiveDate && x.EntityId == returnKeyItem.Key.Entity && x.TradeType == returnKeyItem.Key.TradeType && x.ContractNumber == returnKeyItem.Key.ContractNumber && x.ContraEntity == returnKeyItem.Key.ContraEntity).First();
                    contractBreakOutList.Add(_contractBreakOut);
                }
            }
            else
            { 
                foreach (var item in items.Where(x => x.Enabled))
                {
                    var _contractBreakOut = ContractCalculator.ConvertToNetView(DataContracts.LoadContract(item.EffectiveDate, item.Entity, item.TradeType, item.ContractNumber, item.ContraEntity), new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>());
                    contractBreakOutList.Add(_contractBreakOut);
                }
            }

            if (items.Count() <= 50)
            {
                foreach (var _contractBreakOut in contractBreakOutList)
                {
                    _contractBreakOut.ActivityFlag = SL_ActivityFlag.Processing;
                    _contractBreakOut.EntityType = SL_EntityType.Rate;
                }

                RealTime.Broadcast.ContractBreakOut(contractBreakOutList);
            }

            foreach (RateChangeModel model in items)
            {
                if (model.Enabled)
                {
                    try
                    {
                        if (model.ContraRateSwing)
                        {
                            var swingCustodian = contraRateSwing.Where(x => x.ContraEntity == model.ContraEntity).First().SwingEntity;

                            var swingEntity = comapnyIds.Where(x => x.Custodian == swingCustodian).First().CompanyId.ToString();

                            var contracts = DataContracts.LoadContractsByIssue(model.EffectiveDate, model.EffectiveDate, swingEntity, model.IssueId.ToString());

                            if (contracts.Any(x => x.EffectiveDate == model.EffectiveDate && x.IssueId == model.IssueId && x.ContraEntity == model.ContraEntity && x.TradeType != model.TradeType && x.Quantity == model.Quantity))
                            {
                                var _Contract = contracts.Where(x => x.EffectiveDate == model.EffectiveDate && x.IssueId == model.IssueId && x.ContraEntity == model.ContraEntity && x.TradeType != model.TradeType && x.Quantity == model.Quantity).First();

                                var swingModel = RateChangeModel.Convert(1, "", _Contract);

                                swingModel.NewRate = model.NewRate;
                                swingModel.NewRateCode = model.NewRateCode;

                                rateChangeModelList.Add(swingModel);
                            }
                            else
                            {
                                var _Contracts = contracts.Where(x => x.EffectiveDate == model.EffectiveDate && x.IssueId == model.IssueId && x.ContraEntity == model.ContraEntity && x.TradeType != model.TradeType);

                                foreach (var item in _Contracts)
                                {
                                    var swingModel = RateChangeModel.Convert(1, "", item);

                                    swingModel.NewRate = model.NewRate;
                                    swingModel.NewRateCode = model.NewRateCode;

                                    rateChangeModelList.Add(swingModel);
                                }
                            }
                        }                   

                        ValidationHelper.ValidateRateChangeRequest(model);
                    }
                    catch (Exception e)
                    {
                        model.MemoInfo = e.Message;
                        model.SubmissionType = StatusDetail.Rejected;
                    }

                    rateChangeModelList.Add(model);
                }               
            }

            try
            {
                while (rateChangeModelList.Count() > 0)
                {
                    var _pieceList = rateChangeModelList.Take(50).ToList();
                   
                    DataExternalOperations.AddRateChangeRangeModel(_pieceList);

                    rateChangeModelList.RemoveRange(0, _pieceList.Count());
                }

                foreach (var item in items)
                {
                    if (item.Enabled)
                    {
                        item.SubmissionType = StatusDetail.Approved;
                        item.MemoInfo = string.Format(message, item.ExecutingSystem.ToString());
                    }
                }
            }
            catch(Exception errorType)
            {
                foreach (var item in items)
                {
                    item.SubmissionType = StatusDetail.Rejected;
                    item.MemoInfo = errorType.Message;
                }
            }


            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }       

        public JsonResult ProcessMemoSegChanges(IEnumerable<MemoSegUpdateModel> items)
        {
            bool success = false;

            foreach (MemoSegUpdateModel model in items)
            {
                if (model.Enabled)
                {
                    try
                    {
                        success = DataExternalOperations.AddMemoSeg(model);

                        if (success)
                        {
                            model.SubmissionType = StatusDetail.Approved;
                            model.MemoInfo = "Msg submitted to DTCC.";
                        }
                        else
                        {
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                    catch (Exception e)
                    {
                        model.MemoInfo = e.Message;
                        model.SubmissionType = StatusDetail.Rejected;
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult ProcessReturnExpandedOptions(IEnumerable<ReturnTypeCalculationModel> items, 
            string batchCodeOverride,
            TradeType tradeType,
            SL_EntityType entityType,
            string contraEntity)
        {
            if (items != null)
            {
                foreach (ReturnTypeCalculationModel model in items.Where(x => x.Enabled == true))
                {
                    model.ReturnBatchCode = batchCodeOverride.ToUpper();
                    model.TradeType = tradeType;
                    model.EntityType = entityType;
                    model.ContraEntity = contraEntity;

                    model.SubmissionType = StatusDetail.HeldLocal;
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessReturnBatchCode(IEnumerable<ReturnTypeCalculationModel> items, string batchCode)
        {
            if (items != null)
            {
                foreach (ReturnTypeCalculationModel model in items.Where(x => x.Enabled == true))
                {
                    model.ReturnBatchCode = batchCode.ToUpper();

                    if (!(string.IsNullOrWhiteSpace(model.ReturnBatchCode) ? "" : model.ReturnBatchCode).Equals(batchCode))
                    {
                        model.SubmissionType = StatusDetail.HeldLocal;
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessReturnDeliveryCode(IEnumerable<ReturnTypeCalculationModel> items, SL_DeliveryCode deliveryCode)
        {
            if (items != null)
            {
                foreach (ReturnTypeCalculationModel model in items.Where(x => x.Enabled == true))
                {
                    model.DeliveryCode = deliveryCode;

                    model.SubmissionType = StatusDetail.HeldLocal;
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessReturns(IEnumerable<ReturnTypeCalculationModel> items)
        {
            if (items != null)
            {
                List<SL_ContractBreakOutExtendedProjection> contractBreakOutList = new List<SL_ContractBreakOutExtendedProjection>();

                foreach (var item in items.Where(x => x.Enabled))
                {
                    var _contractBreakOut = ContractCalculator.ConvertToNetView(DataContracts.LoadContract(item.EffectiveDate, item.Entity, item.TradeType, item.ContractNumber, item.ContraEntity), new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>());

                    _contractBreakOut.ActivityFlag = SL_ActivityFlag.Processing;
                    _contractBreakOut.EntityType = SL_EntityType.Return;

                    contractBreakOutList.Add(_contractBreakOut);
                }

                RealTime.Broadcast.ContractBreakOut(contractBreakOutList);

                foreach (ReturnTypeCalculationModel model in items)
                {
                    bool success = false;

                    if (model.Enabled)
                    {
                        try
                        {
                            if (model.ReturnPortionQuantity > 0)
                            {
                                if (model.IsPendingReturn)
                                {
                                    model.ReturnBatchCode = "E";
                                    model.DeliveryCode = SL_DeliveryCode.OTHER;
                                }

                                SL_ActivitySubType item = new SL_ActivitySubType();

                                if (model.PortionType == PortionType.Partial)
                                {
                                    item = SL_ActivitySubType.ReturnPartial;
                                }
                                else if (model.PortionType == PortionType.Full)
                                {
                                    item = SL_ActivitySubType.ReturnFull;
                                }

                                success = DataExternalOperations.AddNewReturn(model.Entity, model.ContraEntity, model.ContractNumber, model.TradeType, model.IssueId, model.ReturnPortionQuantity, model.ReturnPortionAmount, item, model.ReturnBatchCode, model.CallBack, model.DeliveryCode);

                                if (success)
                                {                                   
                                    model.SubmissionType = StatusDetail.Approved;
                                    model.MemoInfo = "Msg submitted to Loanet.";
                                }
                                else
                                {
                                    model.SubmissionType = StatusDetail.Rejected;
                                }
                            }
                            else
                            {
                                model.Enabled = false;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.Enabled = false;
                            model.SubmissionType = StatusDetail.Rejected;
                        }

                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ProcessBulkReturnCallbacks(IEnumerable<ReturnTypeCalculationModel> items)
        {
            if (items != null)
            {
                List<SL_ContractBreakOutExtendedProjection> contractBreakOutList = new List<SL_ContractBreakOutExtendedProjection>();

                foreach (var item in items.Where(x => x.Enabled))
                {
                    var _contractBreakOut = ContractCalculator.ConvertToNetView(DataContracts.LoadContract(item.EffectiveDate, item.Entity, item.TradeType, item.ContractNumber, item.ContraEntity), new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>());

                    _contractBreakOut.ActivityFlag = SL_ActivityFlag.Processing;
                    _contractBreakOut.EntityType = item.EntityType;

                    contractBreakOutList.Add(_contractBreakOut);
                }

                RealTime.Broadcast.ContractBreakOut(contractBreakOutList);

                foreach (ReturnTypeCalculationModel model in items)
                {
                    bool success = false;

                    if (model.Enabled)
                    {
                        try
                        {
                            if (model.EntityType == SL_EntityType.Callback)
                            {
                                SL_Callback callBack = new SL_Callback();


                                callBack.ContractNumber = model.ContractNumber;
                                callBack.ContraEntity = model.ContraEntity;
                                callBack.CurrentQuantity = model.Quantity;
                                callBack.CurrentAmount = model.Quantity * model.Price;
                                callBack.DeliveryDate = DateTime.Today;
                                callBack.EntityId = model.Entity;
                                callBack.IssueId = model.IssueId;
                                callBack.MadeIndicatorId = SL_MadeIndicator.PENDING;
                                callBack.ReturnQuantity = model.ReturnPortionQuantity;
                                callBack.ReturnAmount = model.ReturnPortionAmount;
                                callBack.TradeType = model.TradeType;
                                callBack.EffectiveDate = model.EffectiveDate;                       

                                if (model.TradeType == TradeType.StockBorrow)
                                {
                                    DataCallback.AddCallBack(callBack);
                                                                        
                                    success = DataExternalOperations.AddNewBorrowCallback(callBack);

                                    
                                }
                                else
                                {
                                    success = DataExternalOperations.AddNewLoanCallback(callBack);
                                }
                            }
                            else if (model.EntityType == SL_EntityType.Return)
                            {
                                if (model.ReturnPortionQuantity > 0)
                                {
                                    SL_ActivitySubType item = new SL_ActivitySubType();

                                    if (model.PortionType == PortionType.Partial)
                                    {
                                        item = SL_ActivitySubType.ReturnPartial;
                                    }
                                    else if (model.PortionType == PortionType.Full)
                                    {
                                        item = SL_ActivitySubType.ReturnFull;
                                    }

                                    success = DataExternalOperations.AddNewReturn(model.Entity, model.ContraEntity, model.ContractNumber, model.TradeType, model.IssueId, model.ReturnPortionQuantity, model.ReturnPortionAmount, item, model.ReturnBatchCode, model.CallBack, model.DeliveryCode);
                                }
                                else
                                {
                                    model.Enabled = false;
                                }
                            }

                            if (success)
                            {
                                model.SubmissionType = StatusDetail.Approved;
                                model.MemoInfo = "Msg submitted to Loanet.";
                            }
                            else
                            {
                                model.SubmissionType = StatusDetail.Rejected;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.Enabled = false;
                            model.SubmissionType = StatusDetail.Rejected;
                        }
                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessCancel(IEnumerable<ContractCancelModel> items)
        {
            if (items != null)
            {
                foreach (ContractCancelModel model in items)
                {
                    bool success = false;

                    if (model.Enabled)
                    {
                        try
                        {

                            SL_ActivitySubType item = new SL_ActivitySubType();

                            var contractItem = DataContracts.LoadContract(model.ModelId);

                            success = DataExternalOperations.CancelContract(contractItem);

                            if (success)
                            {
                                model.SubmissionType = StatusDetail.Approved;
                                model.MemoInfo = "Msg submitted to " + contractItem.ExecutionSystemType.ToString();
                            }
                            else
                            {
                                model.SubmissionType = StatusDetail.Rejected;
                            }
                        }
                        catch (Exception e)
                        {
                            model.MemoInfo = e.Message;
                            model.Enabled = false;
                            model.SubmissionType = StatusDetail.Rejected;
                        }

                    }
                }
            }

            return Extended.JsonMax(items, JsonRequestBehavior.AllowGet);
        }


        

        public ActionResult Read_DepoInquiryLookupMultiSelect([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            var depoList = new List<SL_DepoExtendedProjection>();

            try
            {
                foreach (var _entityId in entityId)
                {
                    depoList.AddRange(DataDTCC.LoadDepoExtended(effectiveDate, _entityId).Where(x => x.DepoQuantity > 0 || x.SegregatedQuantity != 0).ToList());
                }
            }
            catch
            {
                depoList = new List<SL_DepoExtendedProjection>();
            }

            return Extended.JsonMax(depoList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_DepoInquiryLookup([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var depoList = new List<SL_DepoExtendedProjection>();

            try
            {
                depoList = DataDTCC.LoadDepoExtended(effectiveDate, entityId).Where(x => x.DepoQuantity > 0 || x.SegregatedQuantity != 0).ToList();
            }
            catch
            {
                depoList = new List<SL_DepoExtendedProjection>();
            }

            return Extended.JsonMax(depoList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetFeeRelatedRateChanges(DateTime effectiveDate, List<string> entityId)
        {
            var itemCount = 0;

            try
            {
                foreach (var _entityId in entityId)
                {
                    itemCount += DataContracts.LoadContractsBreakOut(effectiveDate, effectiveDate, _entityId).Count(x => x.FeeType != "REBATE" && x.RebateRate != x.FeeRate && x.SecuritySettleDate <= effectiveDate);
                }
            }
            catch
            {
                itemCount = 0;
            }

            return Extended.JsonMax(itemCount, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReleaseSegregationItem(SL_DepoExtendedProjection item)
        {
            try
            {
                if (item.SegregatedQuantity != 0)
                {
                    var memoItem = new MemoSegUpdateModel();

                    memoItem.Enabled = true;
                    memoItem.IssueId = (int)item.IssueId;
                    memoItem.EntityId = item.EntityId;
                    memoItem.SecurityNumber = item.SecurityNumber;
                    memoItem.Ticker = item.Ticker;
                    memoItem.Quantity = 0;
                    memoItem.Operator = SL_MemoSegOperator.OVERLAY;
                    memoItem.SubmissionType = StatusDetail.Pending;

                    DataExternalOperations.AddMemoSeg(memoItem);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReleaseSegregationList(DateTime effectiveDate, string entityId)
        {
            var depoList = new List<SL_DepoExtendedProjection>();
            var itemsProcessed = 0;

            try
            {
                depoList = DataDTCC.LoadDepoExtended(effectiveDate, entityId);

                foreach(var item in depoList)
                {
                    if (item.SegregatedQuantity != 0)
                    {
                        var memoItem = new MemoSegUpdateModel();

                        memoItem.Enabled = true;
                        memoItem.IssueId = (int)item.IssueId;
                        memoItem.EntityId = entityId;
                        memoItem.SecurityNumber = item.SecurityNumber;
                        memoItem.Ticker = item.Ticker;
                        memoItem.Quantity = 0;
                        memoItem.Operator = SL_MemoSegOperator.OVERLAY;
                        memoItem.SubmissionType = StatusDetail.Pending;

                        DataExternalOperations.AddMemoSeg(memoItem);

                        itemsProcessed++;
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(itemsProcessed, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ContractProfitLoss(DateTime startDate, DateTime stopDate, string entityId, List<ProfitLossCategoryModel> categoryList, string excludePc)
        {
            var contractList = DataContracts.LoadContracts(startDate, stopDate, entityId).Where(x => !excludePc.Contains(x.ProfitId)).ToList();
            List<ProfitLossCategoryResultsModel> results = new List<ProfitLossCategoryResultsModel>();

            DateTime currentDateTemp = startDate;
            List<DateTime> DistinctDates = new List<DateTime>();

            while (currentDateTemp != stopDate.AddDays(1))
            {
                DistinctDates.Add(currentDateTemp);
                currentDateTemp = currentDateTemp.AddDays(1);
            }


            var totalBalance = 0m;
            var totalIncome = 0m;
            var totalSpread = 0m;


            foreach (var currentDate in DistinctDates)
            {
                contractList = DataContracts.LoadContracts(currentDate, currentDate, entityId).Where(x => !excludePc.Contains(x.ProfitId)).ToList();

                //calculate full value
                totalBalance = contractList.Where(x => x.TradeType == TradeType.StockBorrow && x.EffectiveDate == currentDate).Sum(x => x.Amount);
                totalIncome = contractList.Where(x => x.EffectiveDate == currentDate).Sum(x => x.IncomeAmount);
                totalSpread = (totalIncome * 100 * 360) / totalBalance;

                results.Add(new ProfitLossCategoryResultsModel()
                {
                    EffectiveDate = currentDate,
                    Category = "Firm Total",
                    Balance = totalBalance,
                    Income = totalIncome,
                    Spread = totalSpread
                });



                if (categoryList != null)
                {
                    if (categoryList.Any())
                    {
                        foreach (var item in categoryList)
                        {
                            List<SL_ContractExtendedProjection> contractSubSet = new List<SL_ContractExtendedProjection>();

                            switch (item.OperatorId)
                            {
                                case SL_Operator.notcontain:
                                    contractSubSet = contractList.Where(x => !(string.IsNullOrWhiteSpace(item.Value) ? "" : item.Value).Contains(x.ContraEntity) && x.EffectiveDate == currentDate).ToList();
                                    break;

                                default:
                                case SL_Operator.contains:
                                    contractSubSet = contractList.Where(x => (string.IsNullOrWhiteSpace(item.Value) ? "" : item.Value).Contains(x.ContraEntity) && x.EffectiveDate == currentDate).ToList();
                                    break;
                            }

                            totalBalance = contractSubSet.Where(x => x.TradeType == TradeType.StockBorrow).Sum(x => x.AmountSettled);
                            totalIncome = contractSubSet.Sum(x => x.IncomeAmount);

                            if (totalBalance == 0 || totalIncome == 0)
                            {

                            }
                            else
                            {
                                totalSpread = (totalIncome * 100 * 360) / totalBalance;
                            }


                            results.Add(new ProfitLossCategoryResultsModel()
                            {
                                EffectiveDate = currentDate,
                                Category = item.Category,
                                Balance = totalBalance,
                                Income = totalIncome,
                                Spread = totalSpread
                            });
                        }
                    }
                }
            }

            return Extended.JsonMax(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateProfitLossCategory([DataSourceRequest] DataSourceRequest request, ProfitLossCategoryModel item)
        {
            ModelState.Clear();

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult UpdateProfitLossCategory([DataSourceRequest] DataSourceRequest request, ProfitLossCategoryModel item)
        {
            ModelState.Clear();


            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult DeleteProfitLossCategory([DataSourceRequest] DataSourceRequest request, ProfitLossCategoryModel item)
        {
            ModelState.Clear();


            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult ProcessBulkFee(string feeType, decimal feeOffSet, IEnumerable<FeeChangeModel> list)
        {
            foreach(var item in list)
            {
                if (item.Enabled)
                {
                    item.SubmissionType = StatusDetail.HeldLocal;
                    item.NewFee = feeType;
                    item.NewFeeOffSet = feeOffSet;
                }
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        public decimal CalculateAverageWeightedRate(IEnumerable<SL_ContractBreakOutExtendedProjection> contractList)
        {
            decimal rate = 0;

            if (contractList != null)
            {
                if (contractList.Any())
                {
                    rate = BondFire.Calculators.SLTradeCalculator.CalculateAvgWeightedRate(contractList);
                }
            }

            return rate;
        }

        public decimal CalculateAverageWeightedRateByEffectiveDateAndEntity(DateTime EffectiveDate, List<string> entityIdList)
        {
            decimal rate = 0;

            List<SL_ContractBreakOutExtendedProjection> contractList = new List<SL_ContractBreakOutExtendedProjection>();

            foreach(var entity in entityIdList)
            {
                contractList.AddRange(DataContracts.LoadContractsBreakOut(EffectiveDate, EffectiveDate, entity));
            }

            if (contractList != null)
            {
                if (contractList.Any())
                {
                    rate = BondFire.Calculators.SLTradeCalculator.CalculateAvgWeightedRate(contractList);
                }
            }

            return rate;
        }

        public decimal CalculateAverageWeightedRateProfitLossModelByTradeType(IEnumerable<ProfitLossModel> contractList, TradeType tradeType)
        {
            decimal rate = 0;

            if (contractList != null)
            {
                if (contractList.Any())
                {
                    rate = ProfitLossModel.CalculateAvgWeightedRate("", contractList, tradeType);
                }
            }

            return rate;
        }

        public decimal CalculateAverageWeightedRateByTradeType(IEnumerable<SL_ContractBreakOutByRateExtendedProjection> contractList, TradeType tradeType)
        {
            decimal rate = 0;

            if (contractList != null)
            {
                if (contractList.Any())
                {
                    rate = BondFire.Calculators.SLTradeCalculator.CalculateAvgWeightedRate(contractList.Select(x => x.IssueId).Distinct().First(), contractList.Where(x => x.TradeType == tradeType).ToList());
                }
            }

            return rate;
        }
    }
}