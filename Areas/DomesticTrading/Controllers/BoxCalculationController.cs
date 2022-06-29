using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SLTrader.Tools.Helpers;
using System.Web.SessionState;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Enums;
using SLTrader.Custom;
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class BoxCalculationController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult Read_BoxCalculationSearch([DataSourceRequest] DataSourceRequest request, string entityId, string criteria)
        {
            var item = new SL_BoxCalculationExtendedProjection();
            string path = "";

            try
            {
                if (entityId.Split(new[] { ';' }).Count() > 1)
                {
                    List<SL_BoxCalculationExtendedProjection> boxList = new List<SL_BoxCalculationExtendedProjection>();

                    foreach (var entityIdItem in entityId.Split(new[] { ';' }).ToList())
                    {
                        boxList.Add(DataBoxCalculation.LoadBoxCalculationByIssue(DateTime.Today, entityIdItem, criteria));
                    }

                    item = boxList.GroupBy(x => new { x.EffectiveDate, x.IssueId, x.SecurityNumber, x.Ticker, x.Sedol, x.SecNumber, x.Quick, x.Classification, x.Isin }).Select(q => new SL_BoxCalculationExtendedProjection()
                    {
                        EffectiveDate = q.Key.EffectiveDate,
                        EntityId = "****",
                        ClearingId = "****",
                        SLBoxCalculation = -1,
                        IssueId = q.Key.IssueId,
                        Classification = q.Key.Classification,
                        SecurityNumber = q.Key.SecurityNumber,
                        Ticker = q.Key.Ticker,
                        Isin = q.Key.Isin,
                        Sedol = q.Key.Sedol,
                        Quick = q.Key.Quick,
                        SecNumber = q.Key.SecNumber,
                        Price = q.Max(x => x.Price),
                        CountryIssued = Country.UnitedStates,
                        DtccEligible = DtccEligibleType.NonDTCC,
                        SegregationReq = q.Sum(x => x.SegregationReq),
                        CostToCarry = q.Select(x => x.CostToCarry).First(),
                        IntradayLendingRate = q.Select(x => x.IntradayLendingRate).First(),
                        BorrowAverageWeightedRate = q.Average(x => x.BorrowAverageWeightedRate),
                        LoanAverageWeightedRate = q.Average(x => x.LoanAverageWeightedRate),
                        CurrentMarketCap = 0.0,
                        RecordDate = q.Select(x => x.RecordDate).First(),
                        SharesOutstanding = 0.0m,
                        EquityFloat = 0.0m,
                        TradedVolume = 0.0m,
                        ProjectedRecallTraded = 0.0m,
                        PenaltyBoxCheck = q.Max(x => x.PenaltyBoxCheck),
                        EasyBorrowCheck = q.Max(x => x.EasyBorrowCheck),
                        RestrictedCheck = q.Max(x => x.RestrictedCheck),
                        ThresholdCheck = q.Max(x => x.ThresholdCheck),
                        OccEligibleCheck = q.Max(x => x.OccEligibleCheck),
                        PremiumCheck = q.Max(x => x.PremiumCheck),
                        LoanForPurpose = 0,
                        LoanForPurposeDayCount = (DateTime)q.Key.EffectiveDate,
                        LoanForPurposeAmount = 0,
                        ExcessPositionSettled = q.Sum(x => x.ExcessPositionSettled),
                        ExcessPositionTraded = q.Sum(x => x.ExcessPositionTraded),
                        NetPositionSettled = q.Sum(x => x.ExcessPositionSettled),
                        NetPositionTraded = q.Sum(x => x.ExcessPositionTraded),
                        CustomerLongPositionSettled = q.Sum(x => x.CustomerLongPositionSettled),
                        CustomerLongPositionTraded = q.Sum(x => x.CustomerLongPositionTraded),
                        FirmLongPositionSettled = q.Sum(x => x.FirmLongPositionSettled),
                        FirmLongPositionTraded = q.Sum(x => x.FirmLongPositionTraded),
                        OtherLongPositionSettled = q.Sum(x => x.OtherLongPositionSettled),
                        OtherLongPositionTraded = q.Sum(x => x.OtherLongPositionTraded),
                        CustomerShortPositionSettled = q.Sum(x => x.CustomerShortPositionSettled),
                        CustomerShortPositionTraded = q.Sum(x => x.CustomerShortPositionTraded),
                        FirmShortPositionSettled = q.Sum(x => x.FirmShortPositionSettled),
                        FirmShortPositionTraded = q.Sum(x => x.FirmShortPositionTraded),
                        OtherShortPositionSettled = q.Sum(x => x.OtherShortPositionSettled),
                        OtherShortPositionTraded = q.Sum(x => x.OtherShortPositionTraded),
                        StockBorrowPositionSettled = q.Sum(x => x.StockBorrowPositionSettled),
                        StockBorrowPositionSettledAmt = q.Sum(x => x.StockBorrowPositionSettledAmt),
                        StockBorrowPositionTraded = q.Sum(x => x.StockBorrowPositionTraded),
                        StockBorrowPositionTradedAmt = q.Sum(x => x.StockBorrowPositionTradedAmt),
                        StockLoanPositionSettled = q.Sum(x => x.StockLoanPositionSettled),
                        StockLoanPositionSettledAmt = q.Sum(x => x.StockLoanPositionSettledAmt),
                        StockLoanPositionTraded = q.Sum(x => x.StockLoanPositionTraded),
                        StockLoanPositionTradedAmt = q.Sum(x => x.StockLoanPositionTradedAmt),
                        StockLoanDeltaSettled = q.Sum(x => x.StockLoanDeltaSettled),
                        StockLoanDeltaSettledAmt = q.Sum(x => x.StockLoanDeltaSettledAmt),
                        DepositorySettled = q.Sum(x => x.DepositorySettled),
                        DepositorySettledAmt = q.Sum(x => x.DepositorySettledAmt),
                        DepositoryOtherSettled = q.Sum(x => x.DepositoryOtherSettled),
                        DepositoryOtherSettledAmt = q.Sum(x => x.DepositoryOtherSettledAmt),
                        CustomerBankLoanPositionSettled = q.Sum(x => x.CustomerBankLoanPositionSettled),
                        CustomerBankLoanPositionSettledAmt = q.Sum(x => x.CustomerBankLoanPositionSettledAmt),
                        FirmBankLoanPositionSettled = q.Sum(x => x.FirmBankLoanPositionSettled),
                        FirmBankLoanPositionSettledAmt = q.Sum(x => x.FirmBankLoanPositionSettledAmt),
                        OtherBankLoanPositionSettled = q.Sum(x => x.OtherBankLoanPositionSettled),
                        OtherBankLoanPositionSettledAmt = q.Sum(x => x.OtherBankLoanPositionSettledAmt),
                        CnsFailToDeliverPositionSettled = q.Sum(x => x.CnsFailToDeliverPositionSettled),
                        CnsFailToDeliverPositionSettledAmt = q.Sum(x => x.CnsFailToDeliverPositionSettledAmt),
                        CnsFailToDeliverPositionSettledDayCount = 0,
                        CnsFailToDeliverPositionTraded = q.Sum(x => x.CnsFailToDeliverPositionTraded),
                        CnsFailToDeliverPositionTradedAmt = q.Sum(x => x.CnsFailToDeliverPositionTradedAmt),
                        CnsFailToDeliverPositionTradedDayCount = 0,
                        DvpFailToDeliverPositionSettled = q.Sum(x => x.DvpFailToDeliverPositionSettled),
                        DvpFailToDeliverPositionSettledAmt = q.Sum(x => x.DvpFailToDeliverPositionSettledAmt),
                        DvpFailToDeliverPositionSettledDayCount = 0,
                        DvpFailToDeliverPositionTraded = q.Sum(x => x.DvpFailToDeliverPositionTraded),
                        DvpFailToDeliverPositionTradedAmt = q.Sum(x => x.DvpFailToDeliverPositionTradedAmt),
                        DvpFailToDeliverPositionTradedDayCount = 0,
                        BrokerFailToDeliverPositionSettled = q.Sum(x => x.BrokerFailToDeliverPositionSettled),
                        BrokerFailToDeliverPositionSettledAmt = q.Sum(x => x.BrokerFailToDeliverPositionSettledAmt),
                        BrokerFailToDeliverPositionSettledDayCount = 0,
                        BrokerFailToDeliverPositionTraded = q.Sum(x => x.BrokerFailToDeliverPositionTraded),
                        BrokerFailToDeliverPositionTradedAmt = q.Sum(x => x.BrokerFailToDeliverPositionTradedAmt),
                        BrokerFailToDeliverPositionTradedDayCount = 0,
                        OtherFailToDeliverPositionSettled = q.Sum(x => x.OtherFailToDeliverPositionSettled),
                        OtherFailToDeliverPositionSettledAmt = q.Sum(x => x.OtherFailToDeliverPositionSettledAmt),
                        OtherFailToDeliverPositionSettledDayCount = 0,
                        OtherFailToDeliverPositionTraded = q.Sum(x => x.OtherFailToDeliverPositionTraded),
                        OtherFailToDeliverPositionTradedAmt = q.Sum(x => x.OtherFailToDeliverPositionTradedAmt),
                        OtherFailToDeliverPositionTradedDayCount = 0,
                        TotalFailToDeliverPositionSettled = q.Sum(x => x.TotalFailToDeliverPositionSettled),
                        TotalFailToDeliverPositionSettledAmt = q.Sum(x => x.TotalFailToDeliverPositionSettledAmt),
                        CnsFailToRecievePositionSettled = q.Sum(x => x.CnsFailToRecievePositionSettled),
                        CnsFailToRecievePositionSettledAmt = q.Sum(x => x.CnsFailToRecievePositionSettledAmt),
                        CnsFailToRecievePositionSettledDayCount = 0,
                        CnsFailToRecievePositionTraded = q.Sum(x => x.CnsFailToRecievePositionTraded),
                        CnsFailToRecievePositionTradedAmt = q.Sum(x => x.CnsFailToRecievePositionTradedAmt),
                        CnsFailToRecievePositionTradedDayCount = 0,
                        DvpFailToRecievePositionSettled = q.Sum(x => x.DvpFailToRecievePositionSettled),
                        DvpFailToRecievePositionSettledAmt = q.Sum(x => x.DvpFailToRecievePositionSettledAmt),
                        DvpFailToRecievePositionSettledDayCount = 0,
                        DvpFailToRecievePositionTraded = q.Sum(x => x.DvpFailToRecievePositionTraded),
                        DvpFailToRecievePositionTradedAmt = q.Sum(x => x.DvpFailToRecievePositionTradedAmt),
                        DvpFailToRecievePositionTradedDayCount = 0,
                        BrokerFailToRecievePositionSettled = q.Sum(x => x.BrokerFailToRecievePositionSettled),
                        BrokerFailToRecievePositionSettledAmt = q.Sum(x => x.BrokerFailToRecievePositionSettledAmt),
                        BrokerFailToRecievePositionSettledDayCount = 0,
                        BrokerFailToRecievePositionTraded = q.Sum(x => x.BrokerFailToRecievePositionTraded),
                        BrokerFailToRecievePositionTradedAmt = q.Sum(x => x.BrokerFailToRecievePositionTradedAmt),
                        BrokerFailToRecievePositionTradedDayCount = 0,
                        OtherFailToRecievePositionSettled = q.Sum(x => x.OtherFailToRecievePositionSettled),
                        OtherFailToRecievePositionSettledAmt = q.Sum(x => x.OtherFailToRecievePositionSettledAmt),
                        OtherFailToRecievePositionSettledDayCount = 0,
                        OtherFailToRecievePositionTraded = q.Sum(x => x.OtherFailToRecievePositionTraded),
                        OtherFailToRecievePositionTradedAmt = q.Sum(x => x.OtherFailToRecievePositionTradedAmt),
                        OtherFailToRecievePositionTradedDayCount = 0,
                        TotalFailToRecievePositionSettled = q.Sum(x => x.TotalFailToRecievePositionSettled),
                        TotalFailToRecievePositionSettledAmt = q.Sum(x => x.TotalFailToRecievePositionSettledAmt),
                        SuggestionBorrowSettled = 0,
                        SuggestionBorrowTraded = 0,
                        SuggestionLoanSettled = 0,
                        SuggestionLoanTraded = 0,
                        SuggestionReturnSettled = 0,
                        SuggestionReturnTraded = 0,
                        SuggestionRecallSettled = 0,
                        SuggestionRecallTraded = 0,
                        SuggestionBorrowSettledAmount = 0,
                        SuggestionBorrowTradedAmount = 0,
                        SuggestionLoanSettledAmount = 0,
                        SuggestionLoanTradedAmount = 0,
                        SuggestionReturnSettledAmount = 0,
                        SuggestionReturnTradedAmount = 0,
                        SuggestionRecallSettledAmount = 0,
                        SuggestionRecallTradedAmount = 0,
                        ExcessPositionSettledAmount = 0,
                        ExcessPositionTradedAmount = 0,
                        NetPositionSettledAmount = 0,
                        NetPositionTradedAmount = 0,
                        BorrowForClearingFailToDeliverPercent = q.Sum(x => x.BorrowForClearingFailToDeliverPercent),
                        BorrowForDVPFailToDeliverPercent = q.Sum(x => x.BorrowForDVPFailToDeliverPercent),
                        BorrowForBrokerFailToDeliverPercent = q.Sum(x => x.BorrowForDVPFailToDeliverPercent),
                        BorrowForTotalFailToDeliverPercent = q.Sum(x => x.BorrowForTotalFailToDeliverPercent),
                        PledgePullback = q.Sum(x => x.PledgePullback),
                        PledgePullbackAmount = q.Sum(x => x.PledgePullbackAmount),
                        ClearingBorrowRequirement = q.Sum(x => x.ClearingBorrowRequirement),
                        ClearingBorrowRequirementAmount = q.Sum(x => x.ClearingBorrowRequirementAmount),
                        ClearingRecallRequirement = q.Sum(x => x.ClearingRecallRequirement),
                        ClearingRecallRequirementAmount = q.Sum(x => x.ClearingRecallRequirementAmount),
                        RiskBasedHairCut = q.Sum(x => x.RiskBasedHairCut),
                        IssueComment = -1,
                        FPLPositionSettled = q.Sum(x => x.FPLPositionSettled),
                        FPLPositionSettledAmount = q.Sum(x => x.FPLPositionSettledAmount),
                        FPLPositionTraded = q.Sum(x => x.FPLPositionTraded),
                        FPLPositionTradedAmount = q.Sum(x => x.FPLPositionTradedAmount),
                        FPLRecallPosition = q.Sum(x => x.FPLRecallPosition),
                        FPLRecallPositionAmount = q.Sum(x => x.FPLRecallPositionAmount),
                        FPLStockLoanSettled = q.Sum(x => x.FPLStockLoanSettled),
                        FPLStockLoanSettledAmount = q.Sum(x => x.FPLStockLoanSettledAmount),
                        DateTimeId = DateTime.Today,
                    }).First();
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(criteria))
                    {
                        item = DataBoxCalculation.LoadBoxCalculationByIssue(DateTime.Today, entityId, criteria);
                    }
                }
            }
            catch
            {
                item = new SL_BoxCalculationExtendedProjection();
            }   


            if (SessionService.UserPreference.SLSecurityLayoutTypeId == SL_SecurityLayoutType.TOP)
            {
                path = @"~/Areas/DomesticTrading/Views/BoxCalculation/_BoxCalculationHorizontal.cshtml";
            }
            else
            {
                path = @"~/Areas/DomesticTrading/Views/BoxCalculation/_BoxCalculation.cshtml";
            }

            return PartialView(path, item);
        }

        public ActionResult Read_CashFinancing([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_CashFinancingExtendedProjection> cashFinancingList = new List<SL_CashFinancingExtendedProjection>();

            try
            {
                if ((entityId != null) && (!entityId.Equals("")))
                {
                    cashFinancingList = DataBoxCalculation.LoadCashFinancing(effectiveDate, entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(cashFinancingList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Upate_FPLSpecificAccountAllocationModel([DataSourceRequest] DataSourceRequest request, FPLSpecificAccountAllocationModel model)
        {
            if (model.Quantity == 0)
            {
                model.IsEnabled = false;
            }

            if (model.OriginalQuantity < model.Quantity)
            {
                model.Quantity = model.OriginalQuantity;
            }

            return Json(new[] { model }.ToDataSourceResult(request, ModelState));            
        }

        public ActionResult Read_BoxCalculation([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId, string portfolioName, SettlementTypeEnum settlementType, bool rollupEntity)
        {
            List<SL_BoxCalculationExtendedProjection> boxList = new List<SL_BoxCalculationExtendedProjection>();
            List<SL_Portfolio> portfolioList = new List<SL_Portfolio>();

            ModelState.Clear();

            var includeReceives = (settlementType == SettlementTypeEnum.Projected) ? true : false;

            if (entityId == null)
            {
                return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }

            try
            {
                string entityListString = "";

                foreach (var _entityId in entityId)
                {
                    portfolioList.AddRange(DataPortfolio.LoadPortfolio(_entityId));

                    decimal? _PortfolioID = ((portfolioList.Any(x => x.Name.ToLower().Equals(portfolioName.ToLower()))) ? portfolioList.Where(x => x.Name.ToLower().Equals(portfolioName.ToLower())).Select(x => x.SLPortfolio).First() : -1);

                    var _boxList = _PortfolioID == -1 ?
                         DataBoxCalculation.LoadBoxCalculation(effectiveDate, _entityId.ToString(), 0, 100, includeReceives, true, bool.Parse(DataSystemValues.LoadSystemValue("BoxCalcIncludeDepoAsLendable", "true")), false)
                         : DataPortfolio.LoadPortfolioBox(effectiveDate, _entityId.ToString(), (decimal)_PortfolioID);

                    entityListString += _entityId + ";";

                    boxList.AddRange(_boxList);
                }

                if (rollupEntity)
                {
                    List<SL_BoxCalculationExtendedProjection> rollupEntityList = boxList.GroupBy(x => new { x.EffectiveDate, x.IssueId, x.Classification, x.SecurityNumber, x.Sedol, x.Ticker, x.Isin, x.Quick, x.SecNumber })
                        .Select(q => new SL_BoxCalculationExtendedProjection()
                        {
                            EffectiveDate = q.Key.EffectiveDate,
                            EntityId = entityListString,
                            ClearingId = "****",
                            SLBoxCalculation = -1,
                            IssueId = q.Key.IssueId,
                            Classification = q.Key.Classification,
                            SecurityNumber = q.Key.SecurityNumber,
                            Ticker = q.Key.Ticker,
                            Isin = q.Key.Isin,
                            Sedol = q.Key.Sedol,
                            Quick = q.Key.Quick,
                            SecNumber = q.Key.SecNumber,
                            Price = q.Max(x => x.Price),
                            CountryIssued = Country.UnitedStates,
                            DtccEligible = DtccEligibleType.NonDTCC,
                            SegregationReq = q.Sum(x => x.SegregationReq),
                            CostToCarry = q.Select(x => x.CostToCarry).First(),
                            IntradayLendingRate = q.Select(x => x.IntradayLendingRate).First(),
                            BorrowAverageWeightedRate = q.Average(x => x.BorrowAverageWeightedRate),
                            LoanAverageWeightedRate = q.Average(x => x.LoanAverageWeightedRate),
                            CurrentMarketCap = 0.0,
                            RecordDate = q.Select(x => x.RecordDate).First(),
                            SharesOutstanding = 0.0m,
                            EquityFloat = 0.0m,
                            TradedVolume = 0.0m,
                            ProjectedRecallTraded = 0.0m,
                            PenaltyBoxCheck = q.Max(x => x.PenaltyBoxCheck),
                            EasyBorrowCheck = q.Max(x => x.EasyBorrowCheck),
                            RestrictedCheck = q.Max(x => x.RestrictedCheck),
                            ThresholdCheck = q.Max(x => x.ThresholdCheck),
                            OccEligibleCheck = q.Max(x => x.OccEligibleCheck),
                            PremiumCheck = q.Max(x => x.PremiumCheck),
                            LoanForPurpose = 0,
                            LoanForPurposeDayCount = (DateTime)q.Key.EffectiveDate,
                            LoanForPurposeAmount = 0,
                            ExcessPositionSettled = q.Sum(x => x.ExcessPositionSettled),
                            ExcessPositionTraded = q.Sum(x => x.ExcessPositionTraded),
                            NetPositionSettled = q.Sum(x => x.ExcessPositionSettled),
                            NetPositionTraded = q.Sum(x => x.ExcessPositionTraded),
                            CustomerLongPositionSettled = q.Sum(x => x.CustomerLongPositionSettled),
                            CustomerLongPositionTraded = q.Sum(x => x.CustomerLongPositionTraded),
                            FirmLongPositionSettled = q.Sum(x => x.FirmLongPositionSettled),
                            FirmLongPositionTraded = q.Sum(x => x.FirmLongPositionTraded),
                            OtherLongPositionSettled = q.Sum(x => x.OtherLongPositionSettled),
                            OtherLongPositionTraded = q.Sum(x => x.OtherLongPositionTraded),
                            CustomerShortPositionSettled = q.Sum(x => x.CustomerShortPositionSettled),
                            CustomerShortPositionTraded = q.Sum(x => x.CustomerShortPositionTraded),
                            FirmShortPositionSettled = q.Sum(x => x.FirmShortPositionSettled),
                            FirmShortPositionTraded = q.Sum(x => x.FirmShortPositionTraded),
                            OtherShortPositionSettled = q.Sum(x => x.OtherShortPositionSettled),
                            OtherShortPositionTraded = q.Sum(x => x.OtherShortPositionTraded),
                            StockBorrowPositionSettled = q.Sum(x => x.StockBorrowPositionSettled),
                            StockBorrowPositionSettledAmt = q.Sum(x => x.StockBorrowPositionSettledAmt),
                            StockBorrowPositionTraded = q.Sum(x => x.StockBorrowPositionTraded),
                            StockBorrowPositionTradedAmt = q.Sum(x => x.StockBorrowPositionTradedAmt),
                            StockLoanPositionSettled = q.Sum(x => x.StockLoanPositionSettled),
                            StockLoanPositionSettledAmt = q.Sum(x => x.StockLoanPositionSettledAmt),
                            StockLoanPositionTraded = q.Sum(x => x.StockLoanPositionTraded),
                            StockLoanPositionTradedAmt = q.Sum(x => x.StockLoanPositionTradedAmt),
                            StockLoanDeltaSettled = q.Sum(x => x.StockLoanDeltaSettled),
                            StockLoanDeltaSettledAmt = q.Sum(x => x.StockLoanDeltaSettledAmt),
                            DepositorySettled = q.Sum(x => x.DepositorySettled),
                            DepositorySettledAmt = q.Sum(x => x.DepositorySettledAmt),
                            DepositoryOtherSettled = q.Sum(x => x.DepositoryOtherSettled),
                            DepositoryOtherSettledAmt = q.Sum(x => x.DepositoryOtherSettledAmt),
                            CustomerBankLoanPositionSettled = q.Sum(x => x.CustomerBankLoanPositionSettled),
                            CustomerBankLoanPositionSettledAmt = q.Sum(x => x.CustomerBankLoanPositionSettledAmt),
                            FirmBankLoanPositionSettled = q.Sum(x => x.FirmBankLoanPositionSettled),
                            FirmBankLoanPositionSettledAmt = q.Sum(x => x.FirmBankLoanPositionSettledAmt),
                            OtherBankLoanPositionSettled = q.Sum(x => x.OtherBankLoanPositionSettled),
                            OtherBankLoanPositionSettledAmt = q.Sum(x => x.OtherBankLoanPositionSettledAmt),
                            CnsFailToDeliverPositionSettled = q.Sum(x => x.CnsFailToDeliverPositionSettled),
                            CnsFailToDeliverPositionSettledAmt = q.Sum(x => x.CnsFailToDeliverPositionSettledAmt),
                            CnsFailToDeliverPositionSettledDayCount = 0,
                            CnsFailToDeliverPositionTraded = q.Sum(x => x.CnsFailToDeliverPositionTraded),
                            CnsFailToDeliverPositionTradedAmt = q.Sum(x => x.CnsFailToDeliverPositionTradedAmt),
                            CnsFailToDeliverPositionTradedDayCount = 0,
                            DvpFailToDeliverPositionSettled = q.Sum(x => x.DvpFailToDeliverPositionSettled),
                            DvpFailToDeliverPositionSettledAmt = q.Sum(x => x.DvpFailToDeliverPositionSettledAmt),
                            DvpFailToDeliverPositionSettledDayCount = 0,
                            DvpFailToDeliverPositionTraded = q.Sum(x => x.DvpFailToDeliverPositionTraded),
                            DvpFailToDeliverPositionTradedAmt = q.Sum(x => x.DvpFailToDeliverPositionTradedAmt),
                            DvpFailToDeliverPositionTradedDayCount = 0,
                            BrokerFailToDeliverPositionSettled = q.Sum(x => x.BrokerFailToDeliverPositionSettled),
                            BrokerFailToDeliverPositionSettledAmt = q.Sum(x => x.BrokerFailToDeliverPositionSettledAmt),
                            BrokerFailToDeliverPositionSettledDayCount = 0,
                            BrokerFailToDeliverPositionTraded = q.Sum(x => x.BrokerFailToDeliverPositionTraded),
                            BrokerFailToDeliverPositionTradedAmt = q.Sum(x => x.BrokerFailToDeliverPositionTradedAmt),
                            BrokerFailToDeliverPositionTradedDayCount = 0,
                            OtherFailToDeliverPositionSettled = q.Sum(x => x.OtherFailToDeliverPositionSettled),
                            OtherFailToDeliverPositionSettledAmt = q.Sum(x => x.OtherFailToDeliverPositionSettledAmt),
                            OtherFailToDeliverPositionSettledDayCount = 0,
                            OtherFailToDeliverPositionTraded = q.Sum(x => x.OtherFailToDeliverPositionTraded),
                            OtherFailToDeliverPositionTradedAmt = q.Sum(x => x.OtherFailToDeliverPositionTradedAmt),
                            OtherFailToDeliverPositionTradedDayCount = 0,
                            TotalFailToDeliverPositionSettled = q.Sum(x => x.TotalFailToDeliverPositionSettled),
                            TotalFailToDeliverPositionSettledAmt = q.Sum(x => x.TotalFailToDeliverPositionSettledAmt),
                            CnsFailToRecievePositionSettled = q.Sum(x => x.CnsFailToRecievePositionSettled),
                            CnsFailToRecievePositionSettledAmt = q.Sum(x => x.CnsFailToRecievePositionSettledAmt),
                            CnsFailToRecievePositionSettledDayCount = 0,
                            CnsFailToRecievePositionTraded = q.Sum(x => x.CnsFailToRecievePositionTraded),
                            CnsFailToRecievePositionTradedAmt = q.Sum(x => x.CnsFailToRecievePositionTradedAmt),
                            CnsFailToRecievePositionTradedDayCount = 0,
                            DvpFailToRecievePositionSettled = q.Sum(x => x.DvpFailToRecievePositionSettled),
                            DvpFailToRecievePositionSettledAmt = q.Sum(x => x.DvpFailToRecievePositionSettledAmt),
                            DvpFailToRecievePositionSettledDayCount = 0,
                            DvpFailToRecievePositionTraded = q.Sum(x => x.DvpFailToRecievePositionTraded),
                            DvpFailToRecievePositionTradedAmt = q.Sum(x => x.DvpFailToRecievePositionTradedAmt),
                            DvpFailToRecievePositionTradedDayCount = 0,
                            BrokerFailToRecievePositionSettled = q.Sum(x => x.BrokerFailToRecievePositionSettled),
                            BrokerFailToRecievePositionSettledAmt = q.Sum(x => x.BrokerFailToRecievePositionSettledAmt),
                            BrokerFailToRecievePositionSettledDayCount = 0,
                            BrokerFailToRecievePositionTraded = q.Sum(x => x.BrokerFailToRecievePositionTraded),
                            BrokerFailToRecievePositionTradedAmt = q.Sum(x => x.BrokerFailToRecievePositionTradedAmt),
                            BrokerFailToRecievePositionTradedDayCount = 0,
                            OtherFailToRecievePositionSettled = q.Sum(x => x.OtherFailToRecievePositionSettled),
                            OtherFailToRecievePositionSettledAmt = q.Sum(x => x.OtherFailToRecievePositionSettledAmt),
                            OtherFailToRecievePositionSettledDayCount = 0,
                            OtherFailToRecievePositionTraded = q.Sum(x => x.OtherFailToRecievePositionTraded),
                            OtherFailToRecievePositionTradedAmt = q.Sum(x => x.OtherFailToRecievePositionTradedAmt),
                            OtherFailToRecievePositionTradedDayCount = 0,
                            TotalFailToRecievePositionSettled = q.Sum(x => x.TotalFailToRecievePositionSettled),
                            TotalFailToRecievePositionSettledAmt = q.Sum(x => x.TotalFailToRecievePositionSettledAmt),
                            SuggestionBorrowSettled = 0,
                            SuggestionBorrowTraded = 0,
                            SuggestionLoanSettled = 0,
                            SuggestionLoanTraded = 0,
                            SuggestionReturnSettled = 0,
                            SuggestionReturnTraded = 0,
                            SuggestionRecallSettled = 0,
                            SuggestionRecallTraded = 0,
                            SuggestionBorrowSettledAmount = 0,
                            SuggestionBorrowTradedAmount = 0,
                            SuggestionLoanSettledAmount = 0,
                            SuggestionLoanTradedAmount = 0,
                            SuggestionReturnSettledAmount = 0,
                            SuggestionReturnTradedAmount = 0,
                            SuggestionRecallSettledAmount = 0,
                            SuggestionRecallTradedAmount = 0,
                            ExcessPositionSettledAmount = 0,
                            ExcessPositionTradedAmount = 0,
                            NetPositionSettledAmount = 0,
                            NetPositionTradedAmount = 0,
                            BorrowForClearingFailToDeliverPercent = q.Sum(x => x.BorrowForClearingFailToDeliverPercent),
                            BorrowForDVPFailToDeliverPercent = q.Sum(x => x.BorrowForDVPFailToDeliverPercent),
                            BorrowForBrokerFailToDeliverPercent = q.Sum(x => x.BorrowForDVPFailToDeliverPercent),
                            BorrowForTotalFailToDeliverPercent = q.Sum(x => x.BorrowForTotalFailToDeliverPercent),
                            PledgePullback = q.Sum(x => x.PledgePullback),
                            PledgePullbackAmount = q.Sum(x => x.PledgePullbackAmount),
                            ClearingBorrowRequirement = q.Sum(x => x.ClearingBorrowRequirement),
                            ClearingBorrowRequirementAmount = q.Sum(x => x.ClearingBorrowRequirementAmount),
                            ClearingRecallRequirement = q.Sum(x => x.ClearingRecallRequirement),
                            ClearingRecallRequirementAmount = q.Sum(x => x.ClearingRecallRequirementAmount),
                            RiskBasedHairCut = q.Sum(x => x.RiskBasedHairCut),
                            IssueComment = -1,
                            FPLPositionSettled = q.Sum(x => x.FPLPositionSettled),
                            FPLPositionSettledAmount = q.Sum(x => x.FPLPositionSettledAmount),
                            FPLPositionTraded = q.Sum(x => x.FPLPositionTraded),
                            FPLPositionTradedAmount = q.Sum(x => x.FPLPositionTradedAmount),
                            FPLRecallPosition = q.Sum(x => x.FPLRecallPosition),
                            FPLRecallPositionAmount = q.Sum(x => x.FPLRecallPositionAmount),
                            FPLStockLoanSettled = q.Sum(x => x.FPLStockLoanSettled),
                            FPLStockLoanSettledAmount = q.Sum(x => x.FPLStockLoanSettledAmount),
                            DateTimeId = DateTime.Today,
                        }).ToList();

                    boxList = new List<SL_BoxCalculationExtendedProjection>();

                    rollupEntityList =  rollupEntityList.Select(x => BoxCalculator.CalculatePosition(x, 100, 0, false, false, false)).ToList();

                    boxList.AddRange(rollupEntityList);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(boxList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_SecurityPosition([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_SecurityPositionExtendedProjection> securityPositionList = new List<SL_SecurityPositionExtendedProjection>();

            
            try
            {
                foreach (var _entityId in entityId)
                {
                    securityPositionList.AddRange(DataBoxCalculation.LoadSecurityPosition(effectiveDate, _entityId.ToString()));
                }
            }
            catch (Exception e)
            {
                securityPositionList = new List<SL_SecurityPositionExtendedProjection>();                
            }

            return Extended.JsonMax(securityPositionList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_BoxCalculationSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, SettlementTypeEnum settlementType)
        {
            List<SL_BoxCalculationExtendedSummaryByProductProjection> boxList = new List<SL_BoxCalculationExtendedSummaryByProductProjection>();

            ModelState.Clear();

            var includeReceives = (settlementType == SettlementTypeEnum.Projected) ? true : false;

            if (entityId.Equals(""))
                return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            try
            {
                boxList = DataBoxCalculation.LoadBoxCalculationByProduct(effectiveDate, entityId, 0, 100, includeReceives, true, true);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Load_Borrow(SL_BoxCalculationExtendedProjection item, bool useTraded)
        {
            SingleTradeModel singleTradeModel = new SingleTradeModel();
            singleTradeModel.IsMirror = false;

            decimal suggestionBorrow = (useTraded) ? item.SuggestionBorrowTraded : item.SuggestionBorrowSettled;

            if (item != null && ModelState.IsValid)
            {
                ModelState.Clear();

                singleTradeModel.Trade1 = new SL_TradeExtendedProjection();
                singleTradeModel.Trade1.EntityId = item.EntityId;
                singleTradeModel.Trade1.TradeType = TradeType.StockBorrow;
                singleTradeModel.Trade1.IssueId = item.IssueId;
                singleTradeModel.Trade1.SecurityNumber = item.SecurityNumber;
                singleTradeModel.Trade1.Ticker = item.Ticker;
                singleTradeModel.Trade1.Price = Convert.ToDouble(SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(item.Price), null));
                singleTradeModel.Trade1.Quantity = SLTradeCalculator.CalculatePosition(SLTradeCalculator.Rounding.Up, 100, suggestionBorrow, bool.Parse(DataSystemValues.LoadSystemValue("Round" + singleTradeModel.Trade1.ExecutingSystem, true.ToString())));
                singleTradeModel.Trade1.Mark = 1.02;
                singleTradeModel.Trade1.CollateralFlag = SL_CollateralFlag.C;
                singleTradeModel.Trade1.MarkParameterId = "%";
                singleTradeModel.Trade1.Amount = SLTradeCalculator.CalculateMoney(singleTradeModel.Trade1.Price, singleTradeModel.Trade1.Quantity, 0);
                singleTradeModel.Trade1.SecuritySettleDate = DateTime.Today;
                singleTradeModel.Trade1.CashSettleDate = DateTime.Today;
                singleTradeModel.Trade1.IncomeTracked = true;
                singleTradeModel.Trade1.CurrencyCode = Currency.USDollars;
                singleTradeModel.Trade1.AltEntityId = "";
                singleTradeModel.Trade1.CashLoc = "US";
                singleTradeModel.Trade1.SecurityLoc = "US";
                singleTradeModel.Trade1.BookRebateRate = 0.0;


                singleTradeModel.Trade2 = new SL_TradeExtendedProjection();
                singleTradeModel.Trade2.EntityId = item.EntityId;
                singleTradeModel.Trade2.TradeType = TradeType.StockLoan;
                singleTradeModel.Trade2.IssueId = item.IssueId;
                singleTradeModel.Trade2.SecurityNumber = item.SecurityNumber;
                singleTradeModel.Trade2.Ticker = item.Ticker;
                singleTradeModel.Trade2.Price = Convert.ToDouble(SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(item.Price), null));
                singleTradeModel.Trade2.Quantity = SLTradeCalculator.CalculatePosition(SLTradeCalculator.Rounding.Up, 100, suggestionBorrow, bool.Parse(DataSystemValues.LoadSystemValue("Round" + singleTradeModel.Trade1.ExecutingSystem, true.ToString())));
                singleTradeModel.Trade2.Mark = 1.02;
                singleTradeModel.Trade2.CollateralFlag = SL_CollateralFlag.C;
                singleTradeModel.Trade2.MarkParameterId = "%";
                singleTradeModel.Trade2.Amount = SLTradeCalculator.CalculateMoney(singleTradeModel.Trade2.Price, singleTradeModel.Trade2.Quantity, 0);
                singleTradeModel.Trade2.SecuritySettleDate = DateTime.Today;
                singleTradeModel.Trade2.CashSettleDate = DateTime.Today;
                singleTradeModel.Trade2.IncomeTracked = true;
                singleTradeModel.Trade2.CurrencyCode = Currency.USDollars;
                singleTradeModel.Trade2.AltEntityId = "";
                singleTradeModel.Trade2.CashLoc = "US";
                singleTradeModel.Trade2.SecurityLoc = "US";
                singleTradeModel.Trade2.BookRebateRate = 0.0;
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationTrade.cshtml", singleTradeModel);
        }

        public PartialViewResult Load_Loan(SL_BoxCalculationExtendedProjection item, bool useTraded)
        {
            SingleTradeModel singleTradeModel = new SingleTradeModel();
            singleTradeModel.IsMirror = false;

            decimal suggestionLoan = (useTraded) ? item.SuggestionLoanTraded : item.SuggestionLoanSettled;


            if (item != null && ModelState.IsValid)
            {
                singleTradeModel.Trade1 = new SL_TradeExtendedProjection();
                singleTradeModel.Trade1.EffectiveDate = item.EffectiveDate;
                singleTradeModel.Trade1.EntityId = item.EntityId;
                singleTradeModel.Trade1.TradeType = TradeType.StockLoan;
                singleTradeModel.Trade1.IssueId = item.IssueId;
                singleTradeModel.Trade1.SecurityNumber = item.SecurityNumber;
                singleTradeModel.Trade1.Ticker = item.Ticker;
                singleTradeModel.Trade1.Price = Convert.ToDouble(SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(item.Price), null));
                singleTradeModel.Trade1.Quantity = SLTradeCalculator.CalculatePosition(SLTradeCalculator.Rounding.Up, 100, suggestionLoan, bool.Parse(DataSystemValues.LoadSystemValue("Round" + singleTradeModel.Trade1.ExecutingSystem, true.ToString())));
                singleTradeModel.Trade1.Mark = 1.02;
                singleTradeModel.Trade1.CollateralFlag = SL_CollateralFlag.C;
                singleTradeModel.Trade1.MarkParameterId = "%";
                singleTradeModel.Trade1.Amount = SLTradeCalculator.CalculateMoney(singleTradeModel.Trade1.Price, singleTradeModel.Trade1.Quantity, 0);
                singleTradeModel.Trade1.SecuritySettleDate = DateTime.Today;
                singleTradeModel.Trade1.CashSettleDate = DateTime.Today;
                singleTradeModel.Trade1.IncomeTracked = true;
                singleTradeModel.Trade1.CurrencyCode = Currency.USDollars;
                singleTradeModel.Trade1.AltEntityId = "";
                singleTradeModel.Trade1.CashLoc = "US";
                singleTradeModel.Trade1.SecurityLoc = "US";
                singleTradeModel.Trade1.TradeStatus = StatusDetail.Pending;
                singleTradeModel.Trade1.BookRebateRate = 0.0;


                singleTradeModel.Trade2 = new SL_TradeExtendedProjection();
                singleTradeModel.Trade2.EffectiveDate = item.EffectiveDate;
                singleTradeModel.Trade2.EntityId = item.EntityId;
                singleTradeModel.Trade2.TradeType = TradeType.StockBorrow;
                singleTradeModel.Trade2.IssueId = item.IssueId;
                singleTradeModel.Trade2.SecurityNumber = item.SecurityNumber;
                singleTradeModel.Trade2.Ticker = item.Ticker;
                singleTradeModel.Trade2.Price = Convert.ToDouble(SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(item.Price), null));
                singleTradeModel.Trade2.Quantity = SLTradeCalculator.CalculatePosition(SLTradeCalculator.Rounding.Up, 100, suggestionLoan, bool.Parse(DataSystemValues.LoadSystemValue("Round" + singleTradeModel.Trade2.ExecutingSystem, true.ToString())));
                singleTradeModel.Trade2.Mark = 1.02;
                singleTradeModel.Trade2.CollateralFlag = SL_CollateralFlag.C;
                singleTradeModel.Trade2.MarkParameterId = "%";
                singleTradeModel.Trade2.Amount = SLTradeCalculator.CalculateMoney(singleTradeModel.Trade2.Price, singleTradeModel.Trade2.Quantity, 0);
                singleTradeModel.Trade2.SecuritySettleDate = DateTime.Today;
                singleTradeModel.Trade2.CashSettleDate = DateTime.Today;
                singleTradeModel.Trade2.IncomeTracked = true;
                singleTradeModel.Trade2.CurrencyCode = Currency.USDollars;
                singleTradeModel.Trade2.AltEntityId = "";
                singleTradeModel.Trade2.CashLoc = "US";
                singleTradeModel.Trade2.SecurityLoc = "US";
                singleTradeModel.Trade2.TradeStatus = StatusDetail.Pending;
                singleTradeModel.Trade2.BookRebateRate = 0.0;
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationTrade.cshtml", singleTradeModel);
        }

        public PartialViewResult Pend_ExcessByContract(List<SL_BoxCalculationExtendedProjection> items)
        {

            List<PendingActionModel> returnList = new List<PendingActionModel>();

            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            decimal quantityToPend = 0;
            int id = 1;
            decimal suggestionPend = 0;

            foreach (SL_BoxCalculationExtendedProjection item in items)
            {
                try
                {

                    quantityToPend = 0;
                    suggestionPend = 0;

                    suggestionPend = suggestionPend - (suggestionPend % 100);


                    contractList = DataContracts.LoadContractsByIssue((DateTime)item.EffectiveDate,
                        (DateTime)item.EffectiveDate,
                        item.EntityId,
                        item.IssueId.ToString());

                    foreach (
                        SL_ContractExtendedProjection contractItem in
                            contractList.OrderBy(x => x.RebateRate)
                                .Where(x => x.Quantity > 0 || x.QuantitySettled > 0))
                    {
                        PendingActionModel calcItem = new PendingActionModel();

                        calcItem.Enabled = false;
                        calcItem.ModelId = id;
                        calcItem.EntityId = contractItem.EntityId;
                        calcItem.ContraEntity = contractItem.ContraEntity;
                        calcItem.TypeId = contractItem.ContractNumber;
                        calcItem.TradeType = contractItem.TradeType;
                        calcItem.IssueId = contractItem.IssueId;
                        calcItem.SecurityNumber = contractItem.SecurityNumber;
                        calcItem.Ticker = contractItem.Ticker;
                        calcItem.Price = (contractItem.Amount / contractItem.Quantity);
                        calcItem.Rate = contractItem.RebateRate;
                        calcItem.Quantity = contractItem.Quantity;

                        calcItem.SubmissionType = StatusDetail.Pending;

                        if (quantityToPend < suggestionPend)
                        {
                            if ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) >
                                (suggestionPend - quantityToPend))
                            {
                                //partial
                                calcItem.PendQuantity = (suggestionPend - quantityToPend);
                                calcItem.PendAmount = (suggestionPend - quantityToPend) * calcItem.Price;
                                calcItem.PortionType = PortionType.Partial;

                                quantityToPend += (suggestionPend - quantityToPend);
                            }
                            else
                            {
                                calcItem.PendQuantity = (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                                calcItem.PendAmount = ((contractItem.Quantity - contractItem.QuantityOnRecallOpen) *
                                                          calcItem.Price);
                                calcItem.PortionType = PortionType.Full;

                                quantityToPend += (contractItem.Quantity - contractItem.QuantityOnRecallOpen);
                            }
                        }

                        if (calcItem.PendQuantity == 0)
                        {
                            calcItem.PortionType = PortionType.None;
                        }

                        returnList.Add(calcItem);
                        id += 1;
                    }
                }
                catch (Exception e)
                {
                    return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
                }
            }

            return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_BoxCalculationReturn.cshtml", returnList);
        }

        public JsonResult LoadFilters([DataSourceRequest] DataSourceRequest request, string entityId, string gridName)
        {
            List<string> filterList = new List<string>();

            try
            {
                if (entityId != null)
                {
                    if (!entityId.Equals(""))
                    {
                        filterList.AddRange(DataQuickFilter.LoadQuickFilterNamesByGridName(entityId, gridName));
                    }
                }
            }
            catch
            {

            }


            return Extended.JsonMax(filterList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult LoadFiltersMulti([DataSourceRequest] DataSourceRequest request, string entityIdList, string gridName)
        {
            List<FilterSummaryModel> filterList = new List<FilterSummaryModel>();

            try
            {
                if (entityIdList != null)
                {
                    foreach (var _entityId in entityIdList.Replace("[","").Replace("]","").Split(new char[] { ','}).ToList())
                    {
                        filterList.AddRange(DataQuickFilter.LoadQuickFilterNamesByGridName(_entityId.ToString(), gridName)
                            .Select(x => new FilterSummaryModel()
                            {
                                EntityId = _entityId.ToString(),
                                Custodian = SessionService.UserFirms.Where(q => q.CompanyId.ToString() == _entityId.ToString()).Select(q => q.Custodian).First(),
                                FilterName = x
                            }));
                    }
                }
            }
            catch
            {

            }

            return Extended.JsonMax(filterList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult QuickFilterLookup(string entityId, string gridName, string filterName)
        {
            List<SL_QuickFilter> filterList = DataQuickFilter.LoadQuickFilterByGridName(entityId, gridName, filterName);

            return Extended.JsonMax(filterList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_SettlementLadderRange([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_SettlementLadderRangeProjection> boxList = new List<SL_SettlementLadderRangeProjection>();

            ModelState.Clear();

            if (entityId.Equals(""))
                return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            try
            {
                boxList = DataSettlementLadder.LoadSettlementLadderRange(effectiveDate, entityId, 0, 100, false, false);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_DeliveryExtended([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_PendingDeliveryProjection> pendingDeliveryList = new List<SL_PendingDeliveryProjection>();

            ModelState.Clear();

            if (entityId.Equals(""))
                return Extended.JsonMax(pendingDeliveryList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            try
            {
                pendingDeliveryList = DataPendingDelivery.LoadPendingDelivery(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(pendingDeliveryList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_DeliverySummaryExtended([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_DeliverySummaryExtendedProjection> boxList = new List<SL_DeliverySummaryExtendedProjection>();

            ModelState.Clear();

            if (entityId.Equals(""))
                return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            try
            {
                boxList = DataDTCC.LoadDeliverySummaryExtended(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(boxList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDynamicLayoutDropdown([DataSourceRequest] DataSourceRequest request, string entityIdList, string gridName)
        {
            List<DynamicLayoutModel> nameList = new List<DynamicLayoutModel>();

            foreach (var _entityId in entityIdList.Replace("[", "").Replace("]", "").Split(new char[] { ',' }).ToList())
            {
                nameList.AddRange(                                       
                    DataDynamicLayout.LoadDynamicLayoutAvailableByGridName(_entityId.ToString(), gridName).Select(x => new DynamicLayoutModel()
                    {
                        EntityId = _entityId,
                        ClearingId = DataUser.LoadEntity(int.Parse(_entityId), SessionService.SecurityContext).Custodian,
                        GridName = gridName,
                        LayoutName = x
                    }).ToList());
            }

            return Extended.JsonMax(nameList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDynamicLayoutColumns([DataSourceRequest] DataSourceRequest request, string entityId, string gridName, string layoutName)
        {
            List<SL_DynamicGridLayout> layoutList = new List<SL_DynamicGridLayout>();

            try
            {
                layoutList = DataDynamicLayout.LoadDynamicLayoutByGridName(entityId, gridName, layoutName);
            }
            catch (Exception error)
            {
                layoutList = new List<SL_DynamicGridLayout>();
            }

            return Extended.JsonMax(layoutList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDynamicLayoutByEntity([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_DynamicGridLayout> layoutList = new List<SL_DynamicGridLayout>();

            try
            {
                layoutList = DataDynamicLayout.LoadDynamicLayoutByEntity(entityId);
            }
            catch (Exception error)
            {
                layoutList = new List<SL_DynamicGridLayout>();
            }

            return Extended.JsonMax(layoutList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}
