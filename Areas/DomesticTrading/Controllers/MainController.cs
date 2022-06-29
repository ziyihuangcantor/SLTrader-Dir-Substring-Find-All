 using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using SLTrader.Enums;
using SLTrader.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class MainController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult Read_UnderlyingSecuritiesByIsin([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string isin)
        {
            List<SL_BoxCalculationExtendedProjection> underlyingSecurityList = new List<SL_BoxCalculationExtendedProjection>();

                try
                {
                    if (entityId.Split(new char[] { ';' }).Count() > 1)
                    {
                        foreach (var item in entityId.Split(new char[] { ';' }))
                        {
                            underlyingSecurityList.AddRange(StaticDataCache.BoxCalculationStaticByIsinGet(effectiveDate, item, isin));
                        }
                    }
                    else
                    {
                        underlyingSecurityList = StaticDataCache.BoxCalculationStaticByIsinGet(effectiveDate, entityId, isin);
                    }
                }
                catch
                {
                    underlyingSecurityList = new List<SL_BoxCalculationExtendedProjection>();
                }
            
            return Extended.JsonMax(underlyingSecurityList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_CnsFailToDeliverByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_CnsFtdBreakdownProjection> cnsBreakdownList = new List<SL_CnsFtdBreakdownProjection>();

            try
            {

                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        cnsBreakdownList.AddRange(DataCns.LoadCnsFtdBreakdown(effectiveDate, item, issueId));
                    }
                }
                else
                {
                    cnsBreakdownList = (DataCns.LoadCnsFtdBreakdown(effectiveDate, entityId, issueId));
                }
            }
            catch
            {
                cnsBreakdownList = new List<SL_CnsFtdBreakdownProjection>();
            }

            return Extended.JsonMax(cnsBreakdownList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_TradeDetailByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_TradeDetailExtendedProjection> tradeDetailList = new List<SL_TradeDetailExtendedProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        tradeDetailList.AddRange(DataTradeDetail.LoadTradeDetailByEffectiveDateAndEntityAndIssue(effectiveDate, item, issueId));
                    }
                }
                else
                {
                    tradeDetailList = DataTradeDetail.LoadTradeDetailByEffectiveDateAndEntityAndIssue(effectiveDate, entityId, issueId);
                }
            }
            catch
            {
                tradeDetailList = new List<SL_TradeDetailExtendedProjection>();
            }

            return Extended.JsonMax(tradeDetailList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);          
        }

        public ActionResult Read_StockRecordByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_StockRecordProjection> stockRecordList = new List<SL_StockRecordProjection>();
        
            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        stockRecordList.AddRange(DataStockRecord.LoadStockRecordByIssue(effectiveDate, item, issueId.ToString()));
                    }
                }
                else
                {
                    Company company = DataEntity.LoadEntity(entityId);

                    if (string.IsNullOrWhiteSpace(company.OverrideStockRecord))
                    {
                        stockRecordList.AddRange(DataStockRecord.LoadStockRecordByIssue(effectiveDate, entityId, issueId.ToString()));
                    }
                    else
                    {
                        stockRecordList.AddRange(DataStockRecord.LoadStockRecordByIssue(effectiveDate, company.OverrideStockRecord, issueId.ToString()));
                    }
                }
            }
            catch
            {
                stockRecordList = new List<SL_StockRecordProjection>();
            }

            return Extended.JsonMax(stockRecordList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            
        }

        public ActionResult Read_SettlementLadderByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_SettlementLadderExtendedProjection> settlementLadderList = new List<SL_SettlementLadderExtendedProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        settlementLadderList.AddRange(DataSettlementLadder.LoadSettlementLadder(effectiveDate, item, issueId));
                    }
                }
                else
                {
                    settlementLadderList = DataSettlementLadder.LoadSettlementLadder(effectiveDate, entityId, issueId);
                }
            }
            catch
            {
                settlementLadderList = new List<SL_SettlementLadderExtendedProjection>();
            }
            
            return Extended.JsonMax(settlementLadderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_DTCCByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_DTCCItemProjection> dtccList = new List<SL_DTCCItemProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        dtccList.AddRange(DataDTCC.LoadDTCCItem(effectiveDate, item, issueId));
                    }
                }
                else
                {
                    dtccList = DataDTCC.LoadDTCCItem(effectiveDate, entityId, issueId);
                }
            }
            catch
            {
                dtccList = new List<SL_DTCCItemProjection>();
            }

            return Extended.JsonMax(dtccList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_InventoryByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_InventoryProjection> inventoryList;

            try
            {
                if ( effectiveDate == DateTime.Today )
                {
                    if ( StaticDataCache.InventoryExistsByCriteria( issueId ) )
                    {
                        inventoryList = StaticDataCache.InventoryStaticGet( effectiveDate, entityId, issueId );
                    }
                    else
                    {
                        inventoryList = DataInventory.LoadInventoryByIssue( effectiveDate, entityId, issueId );
                        StaticDataCache.InventoryAdd( issueId, inventoryList );
                    }
                }
                else
                {
                    inventoryList = DataInventory.LoadInventoryByIssue( effectiveDate, entityId, issueId );
                }
            }
            catch
            {
                inventoryList = new List<SL_InventoryProjection>();
            }

            return Extended.JsonMax(inventoryList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);            
        }

        public ActionResult Read_ActivityByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_ActivityProjection> activityList = new List<SL_ActivityProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {                        
                        activityList.AddRange(DataServerStatus.LoadActivityByEntity(effectiveDate, item).Where(x => x.IssueId == issueId).ToList());
                    }
                }
                else
                {
                    activityList = DataServerStatus.LoadActivityByEntity(effectiveDate, entityId).Where(x => x.IssueId == issueId).ToList();
                }             
            }
            catch
            {
                activityList = new List<SL_ActivityProjection>();
            }

            return Extended.JsonMax(activityList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_CommentByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_IssueCommentItemProjection> issueCommentList = new List<SL_IssueCommentItemProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        issueCommentList.AddRange(DataIssue.LoadIssueCOmmebtByEntityAndIssueId(item, issueId));
                    }
                }
                else
                {
                    issueCommentList = DataIssue.LoadIssueCOmmebtByEntityAndIssueId(entityId, issueId);
                }
            }
            catch
            {
                issueCommentList = new List<SL_IssueCommentItemProjection>();
            }


            return Extended.JsonMax(issueCommentList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);            
        }

        public ActionResult Read_FailHistoryByIssue( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId )
        {
            List<SL_BoxCalculationFailProjection> activityList = new List<SL_BoxCalculationFailProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        activityList.AddRange(DataCns.LoadFailHistory(effectiveDate, item, issueId));
                    }
                }
                else
                {
                    activityList = DataCns.LoadFailHistory(effectiveDate, entityId, issueId);
                }
            }
            catch
            {
                activityList = new List<SL_BoxCalculationFailProjection>();
            }

            return Extended.JsonMax(activityList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);            
        }

        public ActionResult Read_ContractByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            try
            
            {
                if ( effectiveDate == DateTime.Today )
                {
                    if (entityId.Split(new char[] { ';' }).Count() > 1)
                    {
                        foreach (var item in entityId.Split(new char[] { ';' }))
                        {
                            contractList.AddRange(StaticDataCache.ContractStaticGet(effectiveDate, item, issueId));
                        }
                    }
                    else
                    {
                        contractList = StaticDataCache.ContractStaticGet(effectiveDate, entityId, issueId);
                    }                  
                }
                else
                {
                    if (entityId.Split(new char[] { ';' }).Count() > 1)
                    {
                        foreach (var item in entityId.Split(new char[] { ';' }))
                        {
                            contractList.AddRange(DataContracts.LoadContractsByIssue(effectiveDate, effectiveDate, item, issueId.ToString()));
                        }
                    }
                    else
                    {
                        contractList = DataContracts.LoadContractsByIssue(effectiveDate, effectiveDate, entityId, issueId.ToString());
                    }
                   
                }

                foreach (var contract in contractList)
                {
                    if (contract.SecurityLoc != "")
                    {
                        contract.IncomeAmount =
                        SLTradeCalculator.CalculateIncome(contract.TradeType,
                            SLTradeCalculator.Locale.International, contract.RebateRate,
                            contract.Amount, contract.CostOfFunds, contract.CollateralFlag);
                    }
                    else
                    {
                        contract.IncomeAmount =
                            SLTradeCalculator.CalculateIncome(contract.TradeType,
                                SLTradeCalculator.Locale.Domestic, contract.RebateRate,
                                contract.Amount, contract.CostOfFunds, contract.CollateralFlag);
                    }
                }
            }
            catch
            {
                contractList = new List<SL_ContractExtendedProjection>();
            }

            return Extended.JsonMax(contractList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ContractBreakOutByRateByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_ContractBreakOutByRateExtendedProjection> contractList = new List<SL_ContractBreakOutByRateExtendedProjection>();

            try

            {
                if (effectiveDate == DateTime.Today)
                {
                    if (entityId.Split(new char[] { ';' }).Count() > 1)
                    {
                        foreach (var item in entityId.Split(new char[] { ';' }))
                        {
                            contractList.AddRange(StaticDataCache.ContractBreakOutByRateStaticGet(effectiveDate, item, issueId));
                        }
                    }
                    else
                    {
                        contractList = StaticDataCache.ContractBreakOutByRateStaticGet(effectiveDate, entityId, issueId);
                    }
                }
                else
                {
                    if (entityId.Split(new char[] { ';' }).Count() > 1)
                    {
                        foreach (var item in entityId.Split(new char[] { ';' }))
                        {
                            contractList = DataContracts.LoadContractsBreakOutByRate(effectiveDate, effectiveDate, entityId).Where(x => x.IssueId == issueId).ToList();

                        }
                    }
                    else
                    {
                        contractList = DataContracts.LoadContractsBreakOutByRate(effectiveDate, effectiveDate, entityId).Where(x => x.IssueId == issueId).ToList();
                    }

                }

                /*foreach (var contract in contractList)
                {
                   if (contract.SecurityLoc != "")
                    {
                        contract.IncomeAmount =
                        SLTradeCalculator.CalculateIncome(contract.TradeType,
                            SLTradeCalculator.Locale.International, contract.RebateRate,
                            contract.Amount, contract.CostOfFunds, contract.CollateralFlag);
                    }
                    else
                    {
                        contract.IncomeAmount =
                            SLTradeCalculator.CalculateIncome(contract.TradeType,
                                SLTradeCalculator.Locale.Domestic, contract.RebateRate,
                                contract.Amount, contract.CostOfFunds, contract.CollateralFlag);
                    }
                }*/
            }
            catch
            {
                contractList = new List<SL_ContractBreakOutByRateExtendedProjection>();
            }

            return Extended.JsonMax(contractList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ContractHistoryByIssue( [DataSourceRequest] DataSourceRequest request, 
            DateTime effectiveDate,             
            string entityId, 
            string contraEntity,
            string contractNumber,
            TradeType tradeType,            
            int issueId )
        {
            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            try
            {
                var contractItem = DataContracts.LoadContract( DataContracts.LoadContract( effectiveDate, entityId, tradeType, contractNumber, contraEntity ).SLContract );

                contractList = DataContracts.LoadContractHistory( contractItem );
            }
            catch
            {
                contractList = new List<SL_ContractExtendedProjection>();
            }

            return Extended.JsonMax(contractList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);        
        }

        public ActionResult Read_RecallByIssue( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId )
        {
            List<SL_RecallExtendedProjection> recallList = new List<SL_RecallExtendedProjection>();

            try
            {
                if (entityId.Split(new char[] { ';' }).Count() > 1)
                {
                    foreach (var item in entityId.Split(new char[] { ';' }))
                    {
                        recallList.AddRange(DataRecalls.LoadRecallsByIssue(effectiveDate, item, issueId));
                    }
                }
                else
                {
                    recallList = DataRecalls.LoadRecallsByIssue(effectiveDate, entityId, issueId);
                }  
            }
            catch
            {
                recallList = new List<SL_RecallExtendedProjection>();
            }

            return Extended.JsonMax(recallList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GetSharedDetail( DateTime? effectiveDate, string entityId, string criteria, SharedDetail shared )
        {
            var result = new PartialViewResult();

            try
            {

                PartialViewModel model = new PartialViewModel();

                model.EntityId = entityId;
                model.EffectiveDate = ( effectiveDate == null ) ? DateTime.Today : (DateTime)effectiveDate;

                if ( !String.IsNullOrWhiteSpace( criteria ) )
                {
                    var issue = DataIssue.LoadIssue( criteria );

                    model.IssueId = issue.IssueId;
                    model.SecurityNumber = issue.Cusip;
                    model.Ticker = issue.Ticker;
                    model.ContractNumber = "";
                    model.ContraEntity = "";
                }


                switch ( shared )
                {
                    case SharedDetail.FailHistory:

                        result =
                          PartialView(
                              "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailFailHistory.cshtml",
                              model );
                        break;

                    case SharedDetail.TradeDetail:
                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailTradeDetail.cshtml",
                                model );
                        break;

                    case SharedDetail.Activity:

                        result =
                            PartialView( "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailActivity.cshtml",
                                model );
                        break;
                    case SharedDetail.SecurityComment:

                        result =
                            PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailIssueComment.cshtml",
                                model);
                        break;

                    case SharedDetail.Contracts:

                        result =
                            PartialView( "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailContract.cshtml",
                                model );
                        break;
                    case SharedDetail.ContractsBreakOut:
                        result =
                            PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailContractBreakOutByRate.cshtml",
                                model);
                        break;
                    case SharedDetail.Recalls:
                        result = PartialView(
                            "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailRecall.cshtml", model );
                        break;

                    case SharedDetail.Returns:
                        result = PartialView(
                            "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailReturnAction.cshtml", model );
                        break;



                    case SharedDetail.Inventory:
                        var inventoryList = DataInventory.LoadInventoryHistoryByIssueByDayCount( model.EffectiveDate, model.EntityId, model.IssueId );
                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailInventoryTrend.cshtml",
                                inventoryList );
                        break;

                    case SharedDetail.StockRecord:

                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailStockRecord.cshtml",
                                model );
                        break;

                    case SharedDetail.SettlementLadder:

                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailSettlementLadder.cshtml",
                                model );
                        break;

                    case SharedDetail.DTCC:

                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailDTCC.cshtml",
                                model );
                        break;

                    case SharedDetail.UnderlyingSecurities:
                        model.SecurityNumber = criteria;

                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailUnderlyingSecurities.cshtml",
                                model);
                        break;

                }

            }
            catch ( Exception e )
            {
                string action = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString( "action" );
                string controller = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString( "controller" );

                DataError.LogError( controller, action, e.Message );
            }

            return result;
        }

        public PartialViewResult GetSharedDetailByContract(DateTime? effectiveDate, string entityId, string criteria, SharedDetail shared, string contraEntity, TradeType tradeType, string contractNumber)
        {
            var result = new PartialViewResult();

            try
            {

                PartialViewModel model = new PartialViewModel();

                model.EntityId = entityId;
                model.EffectiveDate = (effectiveDate == null) ? DateTime.Today : (DateTime) effectiveDate;

                if (!String.IsNullOrWhiteSpace(criteria))
                {
                    var issue = DataIssue.LoadIssue(criteria);

                    model.IssueId = issue.IssueId;
                    model.SecurityNumber = issue.Cusip;
                    model.Ticker = issue.Ticker;
                    model.TradeType = tradeType;
                    model.ContractNumber = contractNumber;
                    model.ContraEntity = contraEntity;
                }
                

                switch (shared)
                {
                    case SharedDetail.FailHistory:
                        
                        result =
                          PartialView(
                              "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailFailHistory.cshtml",
                              model);
                        break;

                    case SharedDetail.TradeDetail:                      
                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailTradeDetail.cshtml",
                                model);
                        break;

                    case SharedDetail.Activity:
                       
                        result =
                            PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailActivity.cshtml",
                                model);
                        break;

                    case SharedDetail.Contracts:

                        result =
                            PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailContract.cshtml",
                                model);
                        break;
                    case SharedDetail.ContractsBreakOut:
                        result =
                            PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailContractBreakOutByRate.cshtml",
                                model);
                        break;
                    case SharedDetail.Recalls:                      
                        result = PartialView(
                            "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailRecall.cshtml", model);
                        break;

                    case SharedDetail.Contract_History:
                        result = PartialView(
                            "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailContractHistory.cshtml", model );
                        break;

                    case SharedDetail.Returns:
                        result = PartialView(
                            "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailReturnAction.cshtml", model);
                        break;



                    case SharedDetail.Inventory:
                        var inventoryList = DataInventory.LoadInventoryByIssue(model.EffectiveDate, model.EntityId, model.IssueId);
                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailInventoryTrend.cshtml",
                                inventoryList);
                        break;

                    case SharedDetail.StockRecord:
                        
                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailStockRecord.cshtml",
                                model);
                        break;

                    case SharedDetail.SettlementLadder:
                      
                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailSettlementLadder.cshtml",
                                model);
                        break;

                    case SharedDetail.DTCC:

                        result =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Shared/Templates/_SharedDetailDTCC.cshtml",
                                model);
                        break;

                }

            }
            catch(Exception e)
            {
                string action = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("action");
                string controller = System.Web.HttpContext.Current.Request.RequestContext.RouteData.GetRequiredString("controller");

                    DataError.LogError(controller, action, e.Message);
            }

            return result;
        }

        public PartialViewResult GetErrorDetail()
        {
            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_Error.cshtml");
        }

        public PartialViewResult GetPartialRefresh()
        {
            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_PartialRefresh.cshtml");
        }

        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }
    }
}
