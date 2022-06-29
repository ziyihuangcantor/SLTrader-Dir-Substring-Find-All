using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using BondFire.Entities.Projections;
using SLTrader.Models;
using SLTrader.Models.DashboardRelatedModels;

namespace SLTrader.Areas.Dashboard.Controllers
{
    public class CreditController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetCreditTotalTrend(DateTime effectiveDate, string entity)
        {
            List<SL_ContractExtendedProjection> contractList = DataContracts.LoadContracts(effectiveDate, effectiveDate, entity);

            List<CreditTotalModel> creditList = contractList.GroupBy(cl => new
            {
                cl.EffectiveDate,
                cl.EntityId,
                cl.ContraEntity,
                cl.TradeType
            }).Select(s => new CreditTotalModel()
            {
                EffectiveDate = s.Key.EffectiveDate,
                EntityId = s.Key.EntityId,
                ContraEntityId = s.Key.ContraEntity,
                TradeType = s.Key.TradeType,
                Quantity = (Enumerable.Sum<SL_ContractExtendedProjection>(s, c => c.QuantitySettled)),
                Amount = (Enumerable.Sum<SL_ContractExtendedProjection>(s, c => c.AmountSettled))
            }).ToList();
                          
            return Json(creditList);
        }

        public ActionResult GetBoxBreakdown(DateTime effectiveDate, string entityId, string criteria)
        {
            List<SL_BoxCalculationExtendedProjection> boxList = new List<SL_BoxCalculationExtendedProjection>();

            if (string.IsNullOrWhiteSpace(criteria))
            {
                boxList = StaticDataCache.BoxCalculationStaticGet(effectiveDate, entityId);
            }
            else
            {
                try
                {
                    /*Issue issue = DataIssue.LoadIssueByCriteria(criteria, criteria, criteria, criteria, criteria).First();
                    boxList = StaticDataCache.BoxCalculationStaticGet(effectiveDate, entityId, issue.IssueId).ToList();*/
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
            }


            IEnumerable<BoxBreakDownByCategoryModel> breakDownList = boxList
                .GroupBy(x => new {x.EffectiveDate, x.EntityId})
                .Select(s => new BoxBreakDownByCategoryModel()
                {
                    EffectiveDate = (DateTime)s.Key.EffectiveDate,
                    EntityId =  s.Key.EntityId,
                    FirmLongSettled = s.Sum(q => q.FirmLongPositionSettled - q.FirmShortPositionSettled),
                    FirmLongSettledAmt = s.Sum(q => (q.FirmLongPositionSettled - q.FirmShortPositionSettled) * (decimal)q.Price),
                    RetailLongSettled = s.Sum(q => q.CustomerLongPositionSettled - q.CustomerShortPositionSettled),
                    RetailLongSettledAmt = s.Sum(q => (q.CustomerLongPositionSettled - q.CustomerShortPositionSettled) * (decimal)q.Price),

                    PledgeSettled = s.Sum(q => q.CustomerBankLoanPositionSettled + q.FirmBankLoanPositionSettled + q.OtherBankLoanPositionSettled),
                    PledgeSettledAmt = s.Sum(q => q.CustomerBankLoanPositionSettledAmt + q.FirmBankLoanPositionSettledAmt + q.OtherBankLoanPositionSettledAmt),
                    FailToDeliverSettled = s.Sum(q => q.CnsFailToDeliverPositionSettled + q.DvpFailToDeliverPositionSettled + q.BrokerFailToDeliverPositionSettled),
                    FailToDeliverSettledAmt = s.Sum(q => q.CnsFailToDeliverPositionSettledAmt + q.DvpFailToDeliverPositionSettledAmt + q.BrokerFailToDeliverPositionSettledAmt),
                    FailToReceieveSettled = s.Sum(q => q.CnsFailToDeliverPositionSettled  + q.DvpFailToDeliverPositionSettled + q.BrokerFailToDeliverPositionSettled),
                    FailToRecieveSettledAmt = s.Sum(q => q.CnsFailToDeliverPositionSettledAmt + q.DvpFailToDeliverPositionSettledAmt + q.BrokerFailToDeliverPositionSettledAmt),

                    SuggestedBorrow = s.Sum(q => q.SuggestionBorrowSettled),
                    SuggestedBorrowAmt = s.Sum(q => q.SuggestionBorrowSettledAmount),
                    StockBorrowSettled = s.Sum(q => q.StockBorrowPositionSettled),
                    StockBorrowSettledAmt = s.Sum(q => q.StockBorrowPositionSettledAmt),
                    StockLoanSettled = s.Sum(q => q.StockLoanPositionSettled),
                    StockLoanSettledAmt = s.Sum(q => q.StockLoanPositionSettledAmt)
                }).ToList();


            return Extended.JsonMax(breakDownList, JsonRequestBehavior.AllowGet);
        }
    }
}
