using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using BondFire.Entities.Projections;
using SLTrader.Models;
using SLTrader.Custom;

namespace SLTrader.Areas.Dashboard.Controllers
{
    public class SecurityController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }


        public PartialViewResult IssueInformationLookup(DateTime effectiveDate, string entityId, string criteria)
        {
            var model = new IssueInformationModel();

            var issue = DataIssue.LoadIssue(criteria);
            model.IssueId = issue.IssueId;
            model.SecurityNumber = issue.Cusip;
            model.Ticker = issue.Ticker;
            if (issue.CallPrice != null) model.Price = (double)issue.CallPrice;
            model.Description = issue.Description_1 + " " +  issue.Description_2 + " " + issue.Description_3 + " " + issue.Description_4;


            try
            {
                var boxCalc = DataBoxCalculation.LoadBoxCalculationByIssue(effectiveDate, entityId, criteria);
                model.LongSettled = boxCalc.CustomerLongPositionSettled + boxCalc.FirmLongPositionSettled;
                model.ShortSettled = boxCalc.CustomerShortPositionSettled + boxCalc.FirmShortPositionSettled;
                model.ExcessDeficitSettled = boxCalc.ExcessPositionSettled;
            }
            catch
            {
                model.LongSettled = 0;
                model.ShortSettled = 0;
                model.ExcessDeficitSettled = 0;
            }

            try
            {
                var contractList = DataContracts.LoadContracts(effectiveDate, effectiveDate, entityId).Where(x => x.IssueId == issue.IssueId).ToList();

                model.BorrowSettled = contractList.Where(x => x.TradeType == TradeType.StockBorrow).Sum(x => x.Quantity);
                model.BorrowProfit = contractList.Where(x => x.TradeType == TradeType.StockBorrow).Sum(x => x.IncomeAmount);

                model.LoanSettled = contractList.Where(x => x.TradeType == TradeType.StockLoan).Sum(x => x.Quantity);
                model.LoanProfit = contractList.Where(x => x.TradeType == TradeType.StockLoan).Sum(x => x.IncomeAmount);

            model.ContraPartyList = contractList.GroupBy(cl => new
                          {
                              cl.EffectiveDate,
                              cl.EntityId,
                              cl.ContraEntity,
                              cl.AccountName,
                              cl.TradeType
                          })
                          .Select(s => new CreditTotalModel
                          {
                              EffectiveDate = s.Key.EffectiveDate,
                              EntityId = s.Key.EntityId,
                              AccountName = s.Key.AccountName,
                              ContraEntityId = s.Key.ContraEntity,
                              TradeType = s.Key.TradeType,
                              Quantity = (s.Sum(c => c.QuantitySettled)),
                              Amount = (s.Sum(c => c.AmountSettled))
                          }).ToList();
            }
            catch
            {
                model.BorrowSettled = 0;
                model.LoanSettled = 0;
            }

            try
            {
                List<SL_RecallExtendedProjection> recallList = DataRecalls.LoadRecallsByIssue(effectiveDate, entityId, issue.IssueId);

                model.BorrowRecallSettled = recallList.Where(x => x.TradeType == TradeType.StockBorrow).Sum(x => x.QuantityRecalled);
                model.LoanRecallSettled = recallList.Where(x => x.TradeType == TradeType.StockLoan).Sum(x => x.QuantityRecalled);
            }
            catch
            {
                model.BorrowRecallSettled = 0;
                model.LoanRecallSettled = 0;
            }

            return PartialView("~/Areas/Dashboard/Views/Research/_IssueInformationPartial.cshtml", model);
        }
    }
}
