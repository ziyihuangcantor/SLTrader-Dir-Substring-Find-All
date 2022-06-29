using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Enums;
using SLTrader.Models;
using SLTrader.Tools;

namespace SLTrader.Areas.Monitor.Controllers
{
    public class MonitorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadObligations([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, ObligationType type)
        {
            var obligationList = new List<ObligationModel>();

            switch (type)
            {
                case ObligationType.StockBorrow:
                    obligationList = GetContracts(effectiveDate, entityId, TradeType.StockBorrow);
                    break;
                case ObligationType.StockLoan:
                    obligationList = GetContracts(effectiveDate, entityId, TradeType.StockLoan);
                    break;

                case ObligationType.FailToDeliver:
                    obligationList = GetFailToDeliver(effectiveDate, entityId);
                    break;

                case ObligationType.FailToRecieve:
                    obligationList = GetFailToRecieve(effectiveDate, entityId);
                    break;
            }

            return Json(obligationList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_EntityDropdown([DataSourceRequest] DataSourceRequest request)
        {
            return Json(SessionService.UserFirms);
        }

        private static List<ObligationModel> GetContracts(DateTime effectiveDate, string entityId, TradeType type)
        {
            var contractList = DataContracts.LoadContracts(effectiveDate, effectiveDate, entityId);

            return (from item in contractList
                where item.SecuritySettleDate.Equals(effectiveDate) && item.TradeType.Equals(type)
                select new ObligationModel
                {
                    EffectiveDate = effectiveDate, EntityId = entityId, IssueId = item.IssueId, SecurityNumber = item.SecurityNumber, Ticker = item.Ticker, Quantity = item.Quantity, Amount = item.Amount, Type = item.TradeType == TradeType.StockBorrow ? ObligationType.StockBorrow.ToString() : ObligationType.StockLoan.ToString()
                }).ToList();
        }

        private static List<ObligationModel> GetFailToDeliver(DateTime effectiveDate, string entityId)
        {
            var boxList = DataBoxCalculation.LoadBoxCalculation(DateTime.Today, entityId, 0, 100, false, false, false, false);

            return (from item in boxList
                where (item.BrokerFailToDeliverPositionSettled + item.CnsFailToDeliverPositionSettled + item.DvpFailToDeliverPositionSettled) > 0
                let issueId = item.IssueId
                where issueId != null
                let price = item.Price
                where price != null
                select new ObligationModel
                {
                    EffectiveDate = effectiveDate, EntityId = entityId, IssueId = (int) issueId, SecurityNumber = item.SecurityNumber, Ticker = item.Ticker, Quantity = (item.BrokerFailToDeliverPositionSettled + item.CnsFailToDeliverPositionSettled + item.DvpFailToDeliverPositionSettled), Amount = ((item.BrokerFailToDeliverPositionSettled + item.CnsFailToDeliverPositionSettled + item.DvpFailToDeliverPositionSettled)*(decimal) price), Type = ObligationType.FailToDeliver.ToString()
                }).ToList();
        }

        private static List<ObligationModel> GetFailToRecieve(DateTime effectiveDate, string entityId)
        {
            var boxList = DataBoxCalculation.LoadBoxCalculation(effectiveDate, entityId, 0, 100, false, false, false, false);

            return (from item in boxList
                where (item.BrokerFailToRecievePositionSettled + item.CnsFailToRecievePositionSettledDayCount + item.DvpFailToRecievePositionSettled) > 0
                let issueId = item.IssueId
                where issueId != null
                let price = item.Price
                where price != null
                select new ObligationModel
                {
                    EffectiveDate = effectiveDate, EntityId = entityId, IssueId = (int) issueId, SecurityNumber = item.SecurityNumber, Ticker = item.Ticker, Quantity = (item.BrokerFailToRecievePositionSettled + item.CnsFailToRecievePositionSettledDayCount + item.DvpFailToRecievePositionSettled), Amount = (item.BrokerFailToRecievePositionSettled + item.CnsFailToRecievePositionSettledDayCount + item.DvpFailToRecievePositionSettled)*(decimal) price, Type = ObligationType.FailToRecieve.ToString()
                }).ToList();
        }

        public PartialViewResult LoadMonitor(DateTime effectiveDate, string entityId, int issueId)
        {
            var monitorList = DataMonitor.LoadMonitor(effectiveDate, entityId).Where(x => x.IssueId == issueId).ToList();

            return PartialView("~/Areas/Monitor/Views/Monitor/_Monitor.cshtml", monitorList);
        }

        public PartialViewResult Read_BoxCalculationSearch([DataSourceRequest] DataSourceRequest request, string entityId, string criteria)
        {
            var item = new SL_BoxCalculationExtendedProjection();

            try
            {
                item = DataBoxCalculation.LoadBoxCalculationByIssue(DateTime.Today, entityId, criteria);              
            }
            catch (Exception)
            {
                
            }

            return PartialView("~/Areas/DomesticTrading/Views/BoxCalculation/_BoxCalculation.cshtml", item);
        }

        public PartialViewResult Read_IssueSearch([DataSourceRequest] DataSourceRequest request, string criteria)
        {
            var item = new SecurityPanelModel();

            try
            {
                item.Issue = DataIssue.LoadIssue(criteria);


                if (item.Issue != null)
                {
                    var list = DataRegulatoryList.LoadRegulatoryListByIssue(DateTime.Today, "42", item.Issue.IssueId.ToString(CultureInfo.InvariantCulture));

                    item.IssueList = new IssueListModel();

                    foreach (var listItem in list)
                    {
                        switch (listItem.RegulatoryType)
                        {
                            case SLRegulatoryType.EASYBORROW:
                                item.IssueList.Easy = true;
                                break;

                            case SLRegulatoryType.PENALTYBOX:
                                item.IssueList.PenaltyBox = true;
                                break;

                            case SLRegulatoryType.RESTRICTED:
                                item.IssueList.Premium = true;
                                break;

                            case SLRegulatoryType.THRESHOLD:
                                item.IssueList.Threshold = true;
                                break;
                        }
                    }
                }

                ModelState.Clear();
                ViewData.Model = item;
            }
            catch (Exception)
            {

            }

            return PartialView("~/Areas/DomesticTrading/Views/Issue/Index.cshtml", ViewData.Model);
        }

    }
}
