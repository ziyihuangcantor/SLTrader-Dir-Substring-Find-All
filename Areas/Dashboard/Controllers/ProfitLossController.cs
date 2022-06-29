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
using BondFire.Core.Dates;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Models;
using SLTrader.Enums;
using SLTrader.Custom;

namespace SLTrader.Areas.Dashboard.Controllers
{
    public class ProfitLossController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        private List<ProfitLossModel> getProfitLossGenericByContraEntityId( DateTime startDate, DateTime stopDate, string entity, bool rollupEntity )
        {
            var startDateSpan = startDate;

            var firmFinancingList = new List<ProfitLossModel>();
            var contractList = new List<SL_ContractExtendedProjection>();

            while (startDateSpan <= stopDate)
            {
                var previousDate = DateCalculator.Default.GetBusinessDayByBusinessDays(startDateSpan, -1);

                contractList.AddRange(DataContracts.LoadContracts(startDateSpan, startDateSpan, entity));
                contractList.AddRange(DataContracts.LoadContracts(previousDate, previousDate, entity));

                startDateSpan = startDateSpan.AddDays(1.0);
            }
                    

            var effectiveDateList = contractList.Select(x => new { x.EffectiveDate, x.EntityId, x.ClearingId, x.ContraEntity, x.AccountName, x.CurrencyCode }).Distinct().ToList();

            firmFinancingList = effectiveDateList.Select(x => new ProfitLossModel()
            {
                EffectiveDate = x.EffectiveDate,
                Entity = x.EntityId,
                IssueId = 0,
                SecurityNumber = "",
                Ticker = "",
                Sedol = "",
                Isin = "",
                Quick = "",
                SecNumber = "",
                Classification = "",
                ClearingId = x.ClearingId,
                ContraEntityId = x.ContraEntity,
                AccountName = x.AccountName,
                ContractCurrency = x.CurrencyCode,
                BorrowQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.ContraEntity == q.ContraEntity && x.CurrencyCode == q.CurrencyCode).Sum(t => t.QuantityFullSettled),
                BorrowBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.ContraEntity == q.ContraEntity && x.CurrencyCode == q.CurrencyCode).Sum(t => t.AmountFullSettled),
                BorrowIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.ContraEntity == q.ContraEntity && x.CurrencyCode == q.CurrencyCode).Sum(t => t.IncomeAmount),
                BorrowRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.ContraEntity == q.ContraEntity && q.CurrencyCode == x.CurrencyCode).Sum(t => t.QuantityOnRecallOpen),

                LoanQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.ContraEntity == q.ContraEntity && x.CurrencyCode == q.CurrencyCode).Sum(t => t.QuantityFullSettled),
                LoanBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.ContraEntity == q.ContraEntity && x.CurrencyCode == q.CurrencyCode).Sum(t => t.AmountFullSettled),
                LoanIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.ContraEntity == q.ContraEntity && x.CurrencyCode == q.CurrencyCode).Sum(t => t.IncomeAmount),
                LoanRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.ContraEntity == q.ContraEntity && q.CurrencyCode == x.CurrencyCode).Sum(t => t.QuantityOnRecallOpen),

                BorrowCustomerCharge = 0,
                BorrowCustomerRate = 0,
                BorrowCustomerQuantity = 0,

                TotalIncome = 0
            }).ToList();

            var contractBAWRList = contractList.GroupBy(x => new { x.EffectiveDate, x.TradeType, x.CurrencyCode }).Select(q => new
            {
                q.Key.EffectiveDate,
                q.Key.TradeType,
                q.Key.CurrencyCode,
                BookWeightedRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(m => m.EffectiveDate == q.Key.EffectiveDate && m.TradeType.GetParDirection() == q.Key.TradeType.GetParDirection() && m.CurrencyCode == q.Key.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList())
            }).ToList();


            firmFinancingList.ForEach(x =>
            {
                x.TotalIncome = x.BorrowIncome + x.LoanIncome;

                try
                {
                    x.BorrowAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.ContraEntity == x.ContraEntityId && x.ContractCurrency == q.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.BorrowAverageRate = 0;
                }

                try
                {
                    x.BorrowBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.BorrowBookAverageRate = 0;
                }

                try
                {
                    x.LoanAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.ContraEntity == x.ContraEntityId && x.ContractCurrency == q.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.LoanAverageRate = 0;
                }

                try
                {
                    x.LoanBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.LoanBookAverageRate = 0;
                }

                try
                {
                    x.Spread = ((x.BorrowIncome - (x.LoanIncome * -1)) * 100 * 360) / ((x.BorrowBalance > x.LoanBalance) ? x.BorrowBalance : x.LoanBalance);
                }
                catch
                {
                    x.Spread = 0;
                }
            });


            firmFinancingList.ForEach(x =>
            {
                var previousDate = firmFinancingList.Where(q => q.EffectiveDate < x.EffectiveDate).Max(m => x.EffectiveDate);

                x.SpreadCloseOfBusiness = firmFinancingList.Where(q => q.EffectiveDate == previousDate && q.Entity == x.Entity && q.ContraEntityId == x.ContraEntityId).Select(m => m.Spread).FirstOrDefault();
            });    

            firmFinancingList.RemoveAll(x => x.EffectiveDate > stopDate);
            firmFinancingList.RemoveAll(x => x.EffectiveDate < startDate);


            return firmFinancingList;
        }

        private List<ProfitLossModel> getProfitLossGenericBySecurityNumber(DateTime startDate, DateTime stopDate, string entity, bool rollupEntity)
        {
            var startDateSpan = startDate;

            var firmFinancingList = new List<ProfitLossModel>();                 
            var contractList = new List<SL_ContractExtendedProjection>();
            var customerBillingList = DataRebateBilling.LoadRebateBillingByEntity(startDate, stopDate, entity);

            while (startDateSpan <= stopDate)
            {
                var previousDate = DateCalculator.Default.GetBusinessDayByBusinessDays(startDateSpan, -1);

                contractList.AddRange(DataContracts.LoadContracts(startDateSpan, startDateSpan, entity));
                contractList.AddRange(DataContracts.LoadContracts(previousDate, previousDate, entity));
  
                startDateSpan = startDateSpan.AddDays(1.0);
            }

            var effectiveDateList = contractList.Select(x => new { x.EffectiveDate, x.EntityId, x.ClearingId, x.IssueId, x.SecurityNumber, x.Ticker, x.Sedol, x.Isin, x.SecNumber, x.Quick, x.Classification, x.CurrencyCode }).Distinct().ToList();


            firmFinancingList = effectiveDateList.Select(x => new ProfitLossModel()
            {
                EffectiveDate = x.EffectiveDate,
                Entity = x.EntityId,
                IssueId = x.IssueId,
                SecurityNumber = x.SecurityNumber,
                Ticker = x.Ticker,
                Sedol = x.Sedol ?? "",
                Isin = x.Isin ?? "",
                Quick = x.Quick ?? "",
                SecNumber = x.SecNumber ?? "",
                Classification = x.Classification ?? "",
                ClearingId = x.ClearingId,
                ContractCurrency = x.CurrencyCode,
                BorrowQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode).Sum(t => t.QuantityFullSettled),
                BorrowBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode).Sum(t => t.AmountFullSettled),
                BorrowIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode).Sum(t => t.IncomeAmount),
                BorrowRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.IssueId == x.IssueId && q.CurrencyCode == x.CurrencyCode).Sum(t => t.QuantityOnRecallOpen),

                LoanQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode).Sum(t => t.QuantityFullSettled),
                LoanBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode).Sum(t => t.AmountFullSettled),
                LoanIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode).Sum(t => t.IncomeAmount),
                LoanRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.IssueId == x.IssueId && q.CurrencyCode == x.CurrencyCode).Sum(t => t.QuantityOnRecallOpen),

                BorrowCustomerCharge = customerBillingList.Any(q => q.EffectiveDate == x.EffectiveDate && q.EntityId == x.EntityId && q.IssueId == x.IssueId) ? customerBillingList.Where(q => q.EffectiveDate == x.EffectiveDate && q.EntityId == x.EntityId && q.IssueId == x.IssueId).Sum(t => t.MarkUpRebateAmount): 0,
                BorrowCustomerRate = customerBillingList.Any(q => q.EffectiveDate == x.EffectiveDate && q.EntityId == x.EntityId && q.IssueId == x.IssueId) ? customerBillingList.Where(q => q.EffectiveDate == x.EffectiveDate && q.EntityId == x.EntityId && q.IssueId == x.IssueId).Average(t => t.MarkupBillingRate): 0,
                BorrowCustomerQuantity = customerBillingList.Any(q => q.EffectiveDate == x.EffectiveDate && q.EntityId == x.EntityId && q.IssueId == x.IssueId) ? customerBillingList.Where(q => q.EffectiveDate == x.EffectiveDate && q.EntityId == x.EntityId && q.IssueId == x.IssueId).Sum(t => t.TodayQuantity) : 0,


                TotalIncome = 0
            }).ToList();

            var contractBAWRList = contractList.GroupBy(x => new { x.EffectiveDate, x.TradeType, x.CurrencyCode }).Select(q => new
            {
                q.Key.EffectiveDate,
                q.Key.TradeType,
                q.Key.CurrencyCode,
                BookWeightedRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(m => m.EffectiveDate == q.Key.EffectiveDate && m.TradeType.GetParDirection() == q.Key.TradeType.GetParDirection() && m.CurrencyCode == q.Key.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList())
            }).ToList();
      
           
            firmFinancingList.ForEach(x =>
            {
                x.TotalIncome = x.BorrowIncome + x.LoanIncome;

                try
                {
                    x.BorrowAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.IssueId == x.IssueId && x.ContractCurrency == q.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.BorrowAverageRate = 0;
                }

                try
                {
                    x.BorrowBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.BorrowBookAverageRate = 0;
                }

                try
                {
                    x.LoanAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.IssueId == x.IssueId && x.ContractCurrency == q.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.LoanAverageRate = 0;
                }

                try
                {
                    x.LoanBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.LoanBookAverageRate = 0;
                }

                try
                {
                    x.Spread = ((x.BorrowIncome - (x.LoanIncome * -1)) * 100 * 360) / ((x.BorrowBalance > x.LoanBalance) ? x.BorrowBalance : x.LoanBalance);
                }
                catch
                {
                    x.Spread = 0;
                }
            });


            firmFinancingList.ForEach(x =>
            {
                var previousDate = firmFinancingList.Where(q => q.EffectiveDate < x.EffectiveDate).Max(m => x.EffectiveDate);

                x.SpreadCloseOfBusiness = firmFinancingList.Where(q => q.EffectiveDate == previousDate && q.Entity == x.Entity && q.IssueId == x.IssueId).Select(m => m.Spread).FirstOrDefault();
            });

            firmFinancingList.RemoveAll(x => x.EffectiveDate > stopDate);
            firmFinancingList.RemoveAll(x => x.EffectiveDate < startDate);

            return firmFinancingList;
        }

        private List<ProfitLossModel> getProfitLossGenericByClassification(DateTime startDate, DateTime stopDate, string entity, bool rollupEntity)
        {
            var startDateSpan = startDate;

            var firmFinancingList = new List<ProfitLossModel>();
            var prevfirmFinancingList = new List<ProfitLossModel>();

            var previousDateList = new List<DateTime>();

            var contractList = new List<SL_ContractExtendedProjection>();
            var prevContractList = new List<SL_ContractExtendedProjection>();



            while (startDateSpan <= stopDate)
            {
                var previousDate = DateCalculator.Default.GetBusinessDayByBusinessDays(startDateSpan, -1);

                contractList.AddRange(DataContracts.LoadContracts(startDateSpan, startDateSpan, entity));
                contractList.AddRange(DataContracts.LoadContracts(previousDate, previousDate, entity));

                startDateSpan = startDateSpan.AddDays(1.0);
            }

            var effectiveDateList = contractList.Select(x => new { x.EffectiveDate, x.EntityId, x.ClearingId, x.Classification, x.CurrencyCode }).Distinct().ToList();


            firmFinancingList = effectiveDateList.Select(x => new ProfitLossModel()
            {
                EffectiveDate = x.EffectiveDate,
                Entity = x.EntityId,
                IssueId = 0,
                SecurityNumber = "",
                Ticker = "",
                Sedol = "",
                Isin = "",
                Quick = "",
                SecNumber = "",
                Classification = x.Classification ?? "",
                ClearingId = x.ClearingId,
                ContractCurrency = x.CurrencyCode,
                BorrowQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.Classification == x.Classification && x.CurrencyCode == q.CurrencyCode).Sum(t => t.QuantityFullSettled),
                BorrowBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.Classification == x.Classification && x.CurrencyCode == q.CurrencyCode).Sum(t => t.AmountFullSettled),
                BorrowIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.Classification == x.Classification && x.CurrencyCode == q.CurrencyCode).Sum(t => t.IncomeAmount),
                BorrowRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.Classification == x.Classification && q.CurrencyCode == x.CurrencyCode).Sum(t => t.QuantityOnRecallOpen),

                LoanQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.Classification == x.Classification && x.CurrencyCode == q.CurrencyCode).Sum(t => t.QuantityFullSettled),
                LoanBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.Classification == x.Classification && x.CurrencyCode == q.CurrencyCode).Sum(t => t.AmountFullSettled),
                LoanIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.Classification == x.Classification && x.CurrencyCode == q.CurrencyCode).Sum(t => t.IncomeAmount),
                LoanRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.Classification == x.Classification && q.CurrencyCode == x.CurrencyCode).Sum(t => t.QuantityOnRecallOpen),

                BorrowCustomerCharge = 0,
                BorrowCustomerRate = 0,
                BorrowCustomerQuantity = 0,

                TotalIncome = 0
            }).ToList();

            var contractBAWRList = contractList.GroupBy(x => new { x.EffectiveDate, x.TradeType, x.CurrencyCode }).Select(q => new
            {
                q.Key.EffectiveDate,
                q.Key.TradeType,
                q.Key.CurrencyCode,
                BookWeightedRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(m => m.EffectiveDate == q.Key.EffectiveDate && m.TradeType.GetParDirection() == q.Key.TradeType.GetParDirection() && m.CurrencyCode == q.Key.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList())
            }).ToList();

            firmFinancingList.ForEach(x =>
            {
                x.TotalIncome = x.BorrowIncome + x.LoanIncome;

                try
                {
                    x.BorrowAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.Classification == x.Classification && x.ContractCurrency == q.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.BorrowAverageRate = 0;
                }


                try
                {
                    x.BorrowBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.BorrowBookAverageRate = 0;
                }

                try
                {
                    x.LoanAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.Classification == x.Classification && x.ContractCurrency == q.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.LoanAverageRate = 0;
                }


                try
                {
                    x.LoanBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.LoanBookAverageRate = 0;
                }

                try
                {
                    x.Spread = ((x.BorrowIncome - (x.LoanIncome * -1)) * 100 * 360) / ((x.BorrowBalance > x.LoanBalance) ? x.BorrowBalance : x.LoanBalance);
                }
                catch
                {
                    x.Spread = 0;
                }
            });


            firmFinancingList.ForEach(x =>
            {
                var previousDate = firmFinancingList.Where(q => q.EffectiveDate < x.EffectiveDate).Max(m => x.EffectiveDate);

                x.SpreadCloseOfBusiness = firmFinancingList.Where(q => q.EffectiveDate == previousDate && q.Entity == x.Entity && q.Classification == x.Classification).Select(m => m.Spread).FirstOrDefault();
            });

            firmFinancingList.RemoveAll(x => x.EffectiveDate > stopDate);
            firmFinancingList.RemoveAll(x => x.EffectiveDate < startDate);

            return firmFinancingList;
        }

        private List<ProfitLossModel> getProfitLossGenericBySecurityNumberAndProfitId( DateTime startDate, DateTime stopDate, string entity, bool rollupEntity)
        {
            var startDateSpan = startDate;

            var firmFinancingList = new List<ProfitLossModel>();
            var contractList = new List<SL_ContractExtendedProjection>();

            while (startDateSpan <= stopDate)
            {
                var previousDate = DateCalculator.Default.GetBusinessDayByBusinessDays(startDateSpan, -1);

                contractList.AddRange(DataContracts.LoadContracts(startDateSpan, startDateSpan, entity));
                contractList.AddRange(DataContracts.LoadContracts(previousDate, previousDate, entity));

                startDateSpan = startDateSpan.AddDays(1.0);
            }
            var effectiveDateList = contractList.Select(x => new { x.EffectiveDate, x.EntityId, x.ClearingId, x.IssueId, x.SecurityNumber, x.Ticker, x.Sedol, x.Isin, x.SecNumber, x.Quick, x.Classification,x.ProfitId, x.CurrencyCode }).Distinct().ToList();


            firmFinancingList = effectiveDateList.Select(x => new ProfitLossModel()
            {
                EffectiveDate = x.EffectiveDate,
                Entity = x.EntityId,
                IssueId = x.IssueId,
                SecurityNumber = x.SecurityNumber,
                Ticker = x.Ticker,
                Sedol = x.Sedol ?? "",
                Isin = x.Isin ?? "",
                Quick = x.Quick ?? "",
                SecNumber = x.SecNumber ?? "",
                Classification = x.Classification ?? "",
                ClearingId = x.ClearingId,
                ContractCurrency = x.CurrencyCode,
                ProfitId = x.ProfitId,
                BorrowQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode && x.ProfitId == q.ProfitId ).Sum(t => t.QuantityFullSettled),
                BorrowBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode && x.ProfitId == q.ProfitId).Sum(t => t.AmountFullSettled),
                BorrowIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode && x.ProfitId == q.ProfitId).Sum(t => t.IncomeAmount),
                BorrowRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.IssueId == x.IssueId && q.CurrencyCode == x.CurrencyCode && x.ProfitId == q.ProfitId).Sum(t => t.QuantityOnRecallOpen),

                LoanQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode && x.ProfitId == q.ProfitId).Sum(t => t.QuantityFullSettled),
                LoanBalance = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode && x.ProfitId == q.ProfitId).Sum(t => t.AmountFullSettled),
                LoanIncome = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && x.IssueId == q.IssueId && x.CurrencyCode == q.CurrencyCode && x.ProfitId == q.ProfitId).Sum(t => t.IncomeAmount),
                LoanRecallQuantity = contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.IssueId == x.IssueId && q.CurrencyCode == x.CurrencyCode && x.ProfitId == q.ProfitId).Sum(t => t.QuantityOnRecallOpen),

                BorrowCustomerCharge = 0,
                BorrowCustomerRate = 0,
                BorrowCustomerQuantity = 0,

                TotalIncome = 0
            }).ToList();

            var contractBAWRList = contractList.GroupBy(x => new { x.EffectiveDate, x.TradeType, x.CurrencyCode }).Select(q => new
            {
                q.Key.EffectiveDate,
                q.Key.TradeType,
                q.Key.CurrencyCode,
                BookWeightedRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(m => m.EffectiveDate == q.Key.EffectiveDate && m.TradeType.GetParDirection() == q.Key.TradeType.GetParDirection() && m.CurrencyCode == q.Key.CurrencyCode).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList())
            }).ToList();


            firmFinancingList.ForEach(x =>
            {
                x.TotalIncome = x.BorrowIncome + x.LoanIncome;
                try
                {
                    x.BorrowAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.IssueId == x.IssueId && x.ContractCurrency == q.CurrencyCode && x.ProfitId == q.ProfitId).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.BorrowAverageRate = 0;
                }

                try
                {
                    x.BorrowBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == 1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.BorrowBookAverageRate = 0;
                }

                try
                {
                    x.LoanAverageRate = SLTradeCalculator.CalculateAvgWeightedRate(contractList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.IssueId == x.IssueId && x.ContractCurrency == q.CurrencyCode && x.ProfitId == q.ProfitId).Select(t => ContractCalculator.ConvertToNetView(t, new List<SL_ReturnActionExtendedProjection>(), new List<SL_RecallExtendedProjection>(), new List<SL_DepoExtendedProjection>())).ToList());
                }
                catch
                {
                    x.LoanAverageRate = 0;
                }

                try
                {
                    x.LoanBookAverageRate = contractBAWRList.Where(q => q.EffectiveDate == x.EffectiveDate && q.TradeType.GetParDirection() == -1 && q.CurrencyCode == x.ContractCurrency).Select(m => m.BookWeightedRate).First();
                }
                catch
                {
                    x.LoanBookAverageRate = 0;
                }

                try
                {
                    x.Spread = ((x.BorrowIncome - (x.LoanIncome * -1)) * 100 * 360) / ((x.BorrowBalance > x.LoanBalance) ? x.BorrowBalance : x.LoanBalance);
                }
                catch
                {
                    x.Spread = 0;
                }
            });


            firmFinancingList.ForEach(x =>
            {
                var previousDate = firmFinancingList.Where(q => q.EffectiveDate < x.EffectiveDate).Max(m => x.EffectiveDate);

                x.SpreadCloseOfBusiness = firmFinancingList.Where(q => q.EffectiveDate == previousDate && q.Entity == x.Entity && q.IssueId == x.IssueId && q.ProfitId == x.ProfitId).Select(m => m.Spread).FirstOrDefault();
            });

            firmFinancingList.RemoveAll(x => x.EffectiveDate > stopDate);
            firmFinancingList.RemoveAll(x => x.EffectiveDate < startDate);

            return firmFinancingList;
        }

       public ActionResult GetProfitLossTrendReadMultiSelect([DataSourceRequest] DataSourceRequest request, DateTime startDate, DateTime stopDate, List<string> entity, ProfitLossEnum enumType, Currency currency, bool rollupEntity)
        {
            var results = new List<ProfitLossModel>();
            var fxRates = new List<FxRate>();
 
            switch (enumType)
            {
                case ProfitLossEnum.ContraID:
                    foreach (var _entityId in entity)
                    {
                        results.AddRange(getProfitLossGenericByContraEntityId(startDate, stopDate, _entityId, rollupEntity));
                    }
                    break;

                case ProfitLossEnum.Security:
                    foreach (var _entityId in entity)
                    {
                        results.AddRange(getProfitLossGenericBySecurityNumber(startDate, stopDate, _entityId, rollupEntity));
                    }
                    break;
                case ProfitLossEnum.Classification:
                    foreach (var _entityId in entity)
                    {
                        results.AddRange(getProfitLossGenericByClassification(startDate, stopDate, _entityId, rollupEntity));
                    }
                    break;

                case ProfitLossEnum.SecurityProfitId:
                    foreach (var _entityId in entity)
                    {
                        results.AddRange(getProfitLossGenericBySecurityNumberAndProfitId(startDate, stopDate, _entityId, rollupEntity));
                    }
                    break;
            }

            var currencyKey = results.GroupBy(x => new { x.EffectiveDate, x.Entity, x.ContractCurrency }).Select(x => new {
                x.Key.EffectiveDate,
                x.Key.Entity,
                x.Key.ContractCurrency
            }).ToList();

            foreach (var item in currencyKey)
            {
                fxRates.AddRange(DataFXRate.LoadFxRate((DateTime)item.EffectiveDate));
            }

            results.ForEach(x =>
            {
                if (x.ContractCurrency == currency)
                {
                    x.FxRate = 1;
                }
                else if (fxRates.Any(q => q.EffectiveDate == x.EffectiveDate && q.Currencyfrom == x.ContractCurrency && q.CurrencyTo == currency))
                {
                    x.FxRate = (decimal)fxRates.Where(q => q.EffectiveDate == x.EffectiveDate && q.Currencyfrom == x.ContractCurrency && q.CurrencyTo == currency).Select(m => m.FXRate).First();
                }
            });

      
            if (rollupEntity)
            {
                results.ForEach(x =>
                {
                    x.Entity = "****";
                    x.ClearingId = "****";
                    x.ContractCurrency = currency;
                });
 
                var newResults = results.GroupBy(x => new { x.EffectiveDate, x.ClearingId, x.Entity, x.IssueId, x.Classification, x.SecurityNumber, x.Ticker, x.Isin, x.Sedol, x.SecNumber, x.Quick, x.ProfitId, x.ContraEntityId, x.AccountName, x.ContractCurrency }).Select(x => new ProfitLossModel()
                {
                    EffectiveDate = x.Key.EffectiveDate,
                    Entity = x.Key.Entity,
                    SecurityNumber = x.Key.SecurityNumber,
                    Ticker = x.Key.Ticker,
                    Sedol = x.Key.Sedol ?? "",
                    Isin = x.Key.Isin ?? "",
                    Quick = x.Key.Quick ?? "",
                    SecNumber = x.Key.SecNumber ?? "",

                    ProfitId = x.Key.ProfitId,
                    ClearingId = x.Key.ClearingId,
                    ContractCurrency = x.Key.ContractCurrency,
                    ContraEntityId = x.Key.ContraEntityId,
                    AccountName = x.Key.AccountName,
                    BorrowQuantity = x.Sum(q => q.BorrowQuantity),
                    BorrowBalance = x.Sum(q => q.BorrowBalance),
                    BorrowIncome = x.Sum(q => q.BorrowIncome),
                    BorrowRecallQuantity = x.Sum(q => q.BorrowRecallQuantity),
                    BorrowAverageRate = x.Average(q => q.BorrowAverageRate),
                    BorrowBookAverageRate = x.Average(q => q.BorrowBookAverageRate),

                    LoanQuantity = x.Sum(q => q.LoanQuantity),
                    LoanBalance = x.Sum(q => q.LoanBalance),
                    LoanIncome = x.Sum(q => q.LoanIncome),
                    LoanRecallQuantity = x.Sum(q => q.LoanRecallQuantity),
                    LoanAverageRate = x.Sum(q => q.LoanAverageRate),
                    LoanBookAverageRate = x.Sum(q => q.LoanBookAverageRate),

                    BorrowCustomerCharge = x.Sum(q => q.BorrowCustomerCharge),
                    BorrowCustomerRate = x.Average(q => q.BorrowCustomerRate),
                    BorrowCustomerQuantity = x.Sum(q => q.BorrowCustomerQuantity),

                    IssueId = x.Key.IssueId,
                    Classification = x.Key.Classification,
                    TotalIncome = x.Sum(q => q.BorrowIncome) + x.Sum(q => q.LoanIncome),
                }).ToList();


                newResults.ForEach(x =>
                {
                    try
                    {
                        x.Spread = ((x.BorrowIncome - x.LoanIncome) * 100 * 360) / ((x.BorrowBalance > x.LoanBalance) ? x.BorrowBalance : x.LoanBalance);
                    }
                    catch
                    {
                        x.Spread = 0;
                    }
                });
      
                results = newResults;
            }


            return Extended.JsonMax(results.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}
