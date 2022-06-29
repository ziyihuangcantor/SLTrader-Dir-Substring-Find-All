using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Xml.Linq;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class SimulatorController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Read_OppurtunityHistorySummary(DateTime effectiveDate, string entityId, int issueId)
        {
            var inventoryList = DataInventory.LoadInventoryHistoryByIssue(effectiveDate, entityId, issueId, 20);

            return Json(inventoryList);
        }

        public ActionResult Read_OppurtunitySummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, TradeType tradeType, IEnumerable<string> sources, bool includeOnlyRates)
        {
            List<SL_OppurtunityPnLProjection> oppurtunityList;            
         
            try
            {
                oppurtunityList = DataOppurtunity.LoadOppurtunityPnL(effectiveDate, entityId, tradeType, sources, includeOnlyRates);

                foreach (var oppurtunity in oppurtunityList)
                {
                    if (oppurtunity.SecurityLoc == "US" || oppurtunity.SecurityLoc == "")
                    {
                        oppurtunity.IncomeAmount = SLTradeCalculator.CalculateIncome(oppurtunity.TradeType, SLTradeCalculator.Locale.Domestic, oppurtunity.RebateRate, oppurtunity.Amount, 0, oppurtunity.CollateralFlag);
                        oppurtunity.MaxProfitableIncome = SLTradeCalculator.CalculateIncome(oppurtunity.TradeType, SLTradeCalculator.Locale.Domestic, oppurtunity.MaxProfitableRate ?? 0, oppurtunity.Amount, 0, oppurtunity.CollateralFlag);
                        oppurtunity.IntradayLendingRateIncome = SLTradeCalculator.CalculateIncome(oppurtunity.TradeType, SLTradeCalculator.Locale.Domestic, oppurtunity.IntradayLendingRate ?? 0, oppurtunity.Amount, 0, oppurtunity.CollateralFlag);
                    }
                    else
                    {
                        oppurtunity.IncomeAmount = SLTradeCalculator.CalculateIncome(oppurtunity.TradeType, SLTradeCalculator.Locale.International, oppurtunity.RebateRate, oppurtunity.Amount, 0, oppurtunity.CollateralFlag);
                        oppurtunity.MaxProfitableIncome = SLTradeCalculator.CalculateIncome(oppurtunity.TradeType, SLTradeCalculator.Locale.International, oppurtunity.MaxProfitableRate ?? 0, oppurtunity.Amount, 0, oppurtunity.CollateralFlag);
                        oppurtunity.IntradayLendingRateIncome = SLTradeCalculator.CalculateIncome(oppurtunity.TradeType, SLTradeCalculator.Locale.International, oppurtunity.IntradayLendingRate ?? 0, oppurtunity.Amount, 0, oppurtunity.CollateralFlag);
                    }

                    if (oppurtunity.IncomeAmount != null && oppurtunity.IntradayLendingRateIncome != null)
                    {
                        oppurtunity.IntradayLendingRateIncomeDifference = oppurtunity.IntradayLendingRateIncome - oppurtunity.IncomeAmount;
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(oppurtunityList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);   
        }

        public ActionResult Read_OppurtunityDetailSummary([DataSourceRequest] DataSourceRequest request,
            DateTime effectiveDate, string entityId, TradeType tradeType, decimal slContract)
        {
            List<SL_OppurtunityPnLDetailProjection> oppurtunityList;

            try
            {
                
                oppurtunityList = DataOppurtunity.LoadOppurtunityPnLDetail(effectiveDate, entityId, tradeType, "",
                    slContract);

                foreach (var oppurtunity in oppurtunityList)
                {
                    oppurtunity.MaxProfitableIncome = SLTradeCalculator.CalculateIncome(oppurtunity.TradeType,
                       SLTradeCalculator.Locale.Domestic, oppurtunity.InventoryRate ?? 0, oppurtunity.Amount, 0, SL_CollateralFlag.C);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(oppurtunityList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_OppurtunityInventorySourceDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<string>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                response = DataInventory.LoadClientFilesByEntity(DateTime.Today, entityId).Select(x => x.Source).Distinct().ToList();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}
