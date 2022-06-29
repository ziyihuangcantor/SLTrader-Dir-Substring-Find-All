using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using BondFire.Entities;
using BondFire.Entities.Projections;
using BondFire.Core.Dates;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using SLTrader.Custom;
using SLTrader.Models;
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ContractController : BaseController
    {        
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProfitLossInformation( DateTime startDate, DateTime stopDate, string entityId )
        {
            string financingBook = DataSystemValues.LoadSystemValue( "ProfitLossFinancingBook", "0696,BGCP,0197" );

            var list = new List<SL_ProfitLossProjection>();

            try
            {
                list = DataContracts.LoadProfitLoss( startDate, stopDate, entityId, financingBook );
            }
            catch
            {
                list = new List<SL_ProfitLossProjection>();
            }

            return Json( list );
        }

        public PartialViewResult LoadContractByTradingModel( DateTime effectiveDate, string entityId, string issue )
        {
            var contractList = DataContracts.LoadContractsByIssue( effectiveDate, effectiveDate, entityId, issue );

            return PartialView( "~/Areas/DomesticTrading/Views/Shared/Templates/_ContractDetail.cshtml", contractList );
        }

        public PartialViewResult LoadProfitLoss( string entityId )
        {
            return PartialView( "~/Areas/DomesticTrading/Views/Contract/_ContractProfitLoss.cshtml", entityId );
        }

        public PartialViewResult ReadContractTradingPartial( string entityId, string issue )
        {
            List<SecurityTradingModel> model = DataServerStatus.LoadSecurityTrading( DateCalculator.Default.PreviousBusinessDay, entityId, issue );

            var path = "";

            if (SessionService.UserPreference.SLSecurityLayoutTypeId == SL_SecurityLayoutType.SIDE)
            {
                path = "~/Areas/DomesticTrading/Views/Trade/_SecurityTrading.cshtml";
            }
            else
            {
                path = "~/Areas/DomesticTrading/Views/Trade/_SecurityTradingHorizontal.cshtml";
            }

            return PartialView( path, model );
        }


        public ActionResult Read_CashPoolSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId, string contraEntityId)
        {
            List<SL_CashPoolExtendedProjection> cashPoolList = new List<SL_CashPoolExtendedProjection>();

            try
            {
                foreach (string _entityId in entityId)
                {
                    cashPoolList.AddRange(DataContracts.LoadCashPool(effectiveDate, _entityId, contraEntityId));
                }
            }
            catch (Exception)
            {
                cashPoolList = new List<SL_CashPoolExtendedProjection>();
            }

            return Extended.JsonMax(cashPoolList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_MarkToMarketSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_MarkToMarketSummaryExtendedProjection> cashPoolList = new List<SL_MarkToMarketSummaryExtendedProjection>();

            try
            {
                foreach (string _entityId in entityId)
                {
                    cashPoolList.AddRange(DataContracts.LoadMarkToMarketSummary(effectiveDate, _entityId));
                }
            }
            catch (Exception error)
            {
                cashPoolList = new List<SL_MarkToMarketSummaryExtendedProjection>();
            }

            return Extended.JsonMax(cashPoolList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_MarkToMarket([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string contraEntityId, SL_CollateralFlag type)
        {
            List<SL_MarkToMarketExtendedProjection> markList = new List<SL_MarkToMarketExtendedProjection>();

            try
            {
                markList = DataContracts.LoadMarkToMarket(effectiveDate, entityId);

                if (markList.Any(x => x.ContraEntityId == contraEntityId && x.CollateralType == type))
                {
                    markList = markList.Where(x => x.ContraEntityId == contraEntityId && x.CollateralType == type).ToList();
                }
                else
                {
                    markList = new List<SL_MarkToMarketExtendedProjection>();
                }
            }
            catch (Exception)
            {
                markList = new List<SL_MarkToMarketExtendedProjection>();
            }

            return Extended.JsonMax(markList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        

        public PartialViewResult ReadContractPartial( DateTime effectiveDate, string entityId, string contraEntityId )
        {
            List<SL_ContractExtendedProjection> contractList = new List<SL_ContractExtendedProjection>();

            try
            {

                contractList = DataContracts.LoadContracts( effectiveDate, effectiveDate, entityId );

                contractList = contractList.Where( x => x.ContraEntity.Equals( contraEntityId ) ).ToList();
            }
            catch ( Exception e )
            {
                ThrowJsonError( e );
            }

            return PartialView( "~/Areas/DomesticTrading/Views/Shared/Templates/_ContractDetail.cshtml", contractList );
        }



        public ActionResult Read_ContractExtendedResearch( [DataSourceRequest] DataSourceRequest request, DateTime startDate, DateTime stopDate, string entityId, string criteria )
        {
            List<SL_ContractExtendedProjection> contractList;

            try
            {
                if ( String.IsNullOrWhiteSpace( criteria ) )
                {
                    contractList = DataContracts.LoadContracts( startDate, stopDate, entityId );
                }
                else
                {
                    try
                    {
                        int issue = DataIssue.LoadIssue( criteria ).IssueId;
                        contractList = DataContracts.LoadContractsByIssue( startDate, stopDate, entityId, issue.ToString() );
                    }
                    catch
                    {
                        contractList = new List<SL_ContractExtendedProjection>();
                    }
                }
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( contractList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ContractExtendedSummary( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            List<SL_ContractExtendedProjection> contractList;

            try
            {
                contractList = DataContracts.LoadContracts( effectiveDate, effectiveDate, entityId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( contractList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ContractBreakOutExtendedSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_ContractBreakOutExtendedProjection> contractList = new List<SL_ContractBreakOutExtendedProjection>();

            try
            {
                if (entityId != null)
                {
                    foreach (var _entityId in entityId)
                    {
                        contractList.AddRange(DataContracts.LoadContractsBreakOut(effectiveDate, effectiveDate, _entityId));
                    }
                }
            }
            catch (Exception e)
            {
                contractList = new List<SL_ContractBreakOutExtendedProjection>();
            }

            return Extended.JsonMax(contractList.AsQueryable().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ContractExtendedSummaryBySecurity( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            var contractList = new List<SL_ContractSummaryByEntityAndSecurityProjection>();

            try
            {
                if ( !entityId.Equals( "" ) )
                {
                    contractList = DataContracts.LoadContractSummaryBySecurity( effectiveDate, entityId );
                }
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( contractList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ContractExtendedSummaryByContraEntityAndSecurity( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string criteria )
        {

            var contractList = new List<SL_ContractSummaryByContraEntityAndSecurityProjection>();

            try
            {
                if ( !entityId.Equals( "" ) )
                {
                    var issueId = DataIssue.LoadIssue( criteria ).IssueId;

                    var contractsList = DataContracts.LoadContractsByIssue( effectiveDate, effectiveDate, entityId, issueId.ToString() );

                    List<SL_ContractSummaryByContraEntityAndSecurityProjection> summaryListBorrows = new List<SL_ContractSummaryByContraEntityAndSecurityProjection>();


                    if ( contractsList.Count( x => x.TradeType == TradeType.StockBorrow ) > 0 )
                    {
                        foreach ( var contractItem in contractsList.Where( x => x.TradeType == TradeType.StockBorrow ) )
                        {
                            summaryListBorrows.Add( new SL_ContractSummaryByContraEntityAndSecurityProjection()
                            {
                                DateTimeId = contractItem.DateTimeId,
                                EffectiveDate = contractItem.EffectiveDate,
                                SLContract = contractItem.SLContract,
                                EntityId = contractItem.EntityId,
                                ContractNumber = contractItem.ContractNumber,
                                TradeType = contractItem.TradeType,
                                ContraEntity = contractItem.ContraEntity,
                                AccountName = contractItem.AccountName,
                                IssueId = contractItem.IssueId,
                                SecurityNumber = contractItem.SecurityNumber,
                                Ticker = contractItem.Ticker,
                                Classification = contractItem.Classification,
                                RecordDate = contractItem.RecordDate,
                                MktPrice = contractItem.MktPrice,
                                BorrowQuantityOnRecall = contractItem.QuantityOnRecallOpen,
                                BorrowQuantityStartOfDay = contractItem.QuantityStartOfDay,
                                BorrowQuantityFullSettled = contractItem.QuantityFullSettled,
                                BorrowQuantityDelta = contractItem.QuantityDelta,
                                BorrowQuantity = contractItem.Quantity,
                                BorrowQuantitySettled = contractItem.QuantitySettled,
                                BorrowAmountStartOfDay = contractItem.AmountStartOfDay,
                                BorrowAmountFullSettled = contractItem.AmountFullSettled,
                                BorrowAmountDelta = contractItem.AmountDelta,
                                BorrowAmount = contractItem.Amount,
                                BorrowAmountSettled = contractItem.AmountSettled,
                                BorrowDepositoryStatus = contractItem.DepositoryStatus,
                                LoanQuantityOnRecall = 0,
                                LoanQuantityStartOfDay = 0,
                                LoanQuantityFullSettled = 0,
                                LoanQuantityDelta = 0,
                                LoanQuantity = 0,
                                LoanQuantitySettled = 0,
                                LoanAmountStartOfDay = 0,
                                LoanAmountFullSettled = 0,
                                LoanAmountDelta = 0,
                                LoanAmount = 0,
                                LoanAmountSettled = 0,
                                LoanDepositoryStatus = SL_DtccItemDOPendMade.Unknown,
                                CollateralFlag = contractItem.CollateralFlag,
                                ValueDate = contractItem.CashSettleDate,
                                SettlementDate = contractItem.SecuritySettleDate,
                                TermDate = contractItem.TermDate,
                                BookRebateRate = contractItem.BookRebateRateIntraday,
                                RebateRate = contractItem.RebateRate,
                                RebateRateId = contractItem.RebateRateId,
                                ProfitId = contractItem.ProfitId,
                                DividendRate = contractItem.DividendRate,
                                DividendCallable = contractItem.DividendCallable,
                                IncomeTracked = contractItem.IncomeTracked,
                                MarkParameterId = contractItem.MarkParameterId,
                                Mark = contractItem.Mark,
                                CurrencyCode = contractItem.CurrencyCode,
                                SecurityLoc = contractItem.SecurityLoc,
                                CashLoc = contractItem.CashLoc,
                                AltEntityId = contractItem.AltEntityId,
                                Comment = contractItem.Comment,
                                ContractFlag = contractItem.ContractFlag,
                                IncomeAmount = contractItem.IncomeAmount,
                                Spread = contractItem.Spread,
                                ActivityItemCount = contractItem.ActivityItemCount
                            } );
                        }
                    }

                    List<SL_ContractSummaryByContraEntityAndSecurityProjection> summaryListLoan = new List<SL_ContractSummaryByContraEntityAndSecurityProjection>();


                    if ( contractsList.Count( x => x.TradeType == TradeType.StockLoan ) > 0 )
                    {
                        foreach ( var contractItem in contractsList.Where( x => x.TradeType == TradeType.StockLoan ) )
                        {
                            summaryListLoan.Add( new SL_ContractSummaryByContraEntityAndSecurityProjection()
                            {
                                DateTimeId = contractItem.DateTimeId,
                                EffectiveDate = contractItem.EffectiveDate,
                                SLContract = contractItem.SLContract,
                                EntityId = contractItem.EntityId,
                                ContractNumber = contractItem.ContractNumber,
                                TradeType = contractItem.TradeType,
                                ContraEntity = contractItem.ContraEntity,
                                AccountName = contractItem.AccountName,
                                IssueId = contractItem.IssueId,
                                SecurityNumber = contractItem.SecurityNumber,
                                Ticker = contractItem.Ticker,
                                Classification = contractItem.Classification,
                                RecordDate = contractItem.RecordDate,
                                MktPrice = contractItem.MktPrice,
                                LoanQuantityOnRecall = contractItem.QuantityOnRecallOpen,
                                LoanQuantityStartOfDay = contractItem.QuantityStartOfDay,
                                LoanQuantityFullSettled = contractItem.QuantityFullSettled,
                                LoanQuantityDelta = contractItem.QuantityDelta,
                                LoanQuantity = contractItem.Quantity,
                                LoanQuantitySettled = contractItem.QuantitySettled,
                                LoanAmountStartOfDay = contractItem.AmountStartOfDay,
                                LoanAmountFullSettled = contractItem.AmountFullSettled,
                                LoanAmountDelta = contractItem.AmountDelta,
                                LoanAmount = contractItem.Amount,
                                LoanAmountSettled = contractItem.AmountSettled,
                                LoanDepositoryStatus = contractItem.DepositoryStatus,
                                BorrowQuantityOnRecall = 0,
                                BorrowQuantityStartOfDay = 0,
                                BorrowQuantityFullSettled = 0,
                                BorrowQuantityDelta = 0,
                                BorrowQuantity = 0,
                                BorrowQuantitySettled = 0,
                                BorrowAmountStartOfDay = 0,
                                BorrowAmountFullSettled = 0,
                                BorrowAmountDelta = 0,
                                BorrowAmount = 0,
                                BorrowAmountSettled = 0,
                                BorrowDepositoryStatus = SL_DtccItemDOPendMade.Unknown,
                                CollateralFlag = contractItem.CollateralFlag,
                                ValueDate = contractItem.CashSettleDate,
                                SettlementDate = contractItem.SecuritySettleDate,
                                TermDate = contractItem.TermDate,
                                BookRebateRate = contractItem.BookRebateRateEndOfDay,
                                RebateRate = contractItem.RebateRate,
                                RebateRateId = contractItem.RebateRateId,
                                ProfitId = contractItem.ProfitId,
                                DividendRate = contractItem.DividendRate,
                                DividendCallable = contractItem.DividendCallable,
                                IncomeTracked = contractItem.IncomeTracked,
                                MarkParameterId = contractItem.MarkParameterId,
                                Mark = contractItem.Mark,
                                CurrencyCode = contractItem.CurrencyCode,
                                SecurityLoc = contractItem.SecurityLoc,
                                CashLoc = contractItem.CashLoc,
                                AltEntityId = contractItem.AltEntityId,
                                Comment = contractItem.Comment,
                                ContractFlag = contractItem.ContractFlag,
                                IncomeAmount = contractItem.IncomeAmount,
                                Spread = contractItem.Spread,
                                ActivityItemCount = contractItem.ActivityItemCount
                            } );
                        }
                    }

                    summaryListBorrows.AddRange( summaryListLoan );

                    contractList = summaryListBorrows;
                }
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( contractList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ContractExtendedSearchSummary( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string contraEntityId, string issueCiteria, TradeType tradeType )
        {
            var contractList = new List<SL_ContractExtendedProjection>();

            try
            {
                if ( entityId != null )
                {
                    contractList = DataContracts.LoadContracts( effectiveDate, effectiveDate, entityId );

                    if ( ( issueCiteria != null ) && ( !issueCiteria.Equals( "" ) ) )
                    {
                        try
                        {
                            var issue = DataIssue.LoadIssue( issueCiteria );

                            contractList = contractList.Where( x => x.IssueId == issue.IssueId ).ToList();
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    if ( ( contraEntityId != null ) && ( !contraEntityId.Equals( "Empty List" ) ) )
                    {
                        contractList = contractList.Where( x => x.ContraEntity == contraEntityId ).ToList();
                    }

                    contractList = contractList.Where( x => x.TradeType == tradeType ).ToList();

                }
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( contractList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_SecurityProfileSummary([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SecurityProfileModel> addOnList = new List<SecurityProfileModel>();

            try
            {

                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    var contractList = DataContracts.LoadContracts(DateTime.Today, DateTime.Today, entityId);

                    var distinctList = contractList.Select(x => new { x.IssueId, x.SecurityNumber, x.Ticker }).Distinct().ToList();

                    decimal totalNotional = contractList.Where(x => x.TradeType == TradeType.StockBorrow && !x.ContraEntity.ToLower().StartsWith("h")).Sum(x => x.AmountFullSettled);

                    decimal totalHedgeNotional = contractList.Where(x => x.TradeType == TradeType.StockBorrow && x.ContraEntity.ToLower().StartsWith("h")) .Sum(x => x.AmountFullSettled);


                    foreach (var item in distinctList)
                    {
                        try
                        {
                            var model = new SecurityProfileModel();

                            model.EffectiveDate = DateTime.Today;
                            model.EntityId = entityId;
                            model.IssueId = item.IssueId.ToString();
                            model.SecurityNumber = item.SecurityNumber;
                            model.Ticker = item.Ticker;


                            decimal issueNotional = contractList.Where(x => x.IssueId == item.IssueId && x.TradeType == TradeType.StockBorrow && !x.ContraEntity.ToLower().StartsWith("h")).Sum(x => x.AmountFullSettled);
                            decimal issueHedgeNotional = contractList.Where(x => x.IssueId == item.IssueId && x.TradeType == TradeType.StockBorrow && x.ContraEntity.ToLower().StartsWith("h")).Sum(x => x.AmountFullSettled);

                            if ((issueNotional > 0) && (totalNotional > 0))
                            {
                                model.BookHypothication = (issueNotional / totalNotional) * 100;
                            }
                            else
                            {
                                model.BookHypothication = 0;
                            }

                            if ((issueHedgeNotional > 0) && (totalHedgeNotional > 0))
                            {
                                model.HedgeBookHypothication = (issueHedgeNotional / totalHedgeNotional) * 100;
                            }
                            else
                            {
                                model.HedgeBookHypothication = 0;
                            }

                            model.TotalIncome = contractList.Where(x => x.IssueId == item.IssueId).Sum(x => x.IncomeAmount);

                            model.BorrowAmount = contractList.Where(x => x.TradeType == TradeType.StockBorrow && x.IssueId == item.IssueId && !x.ContraEntity.ToLower().StartsWith("h") && x.AmountFullSettled > 0).Sum(x => x.AmountFullSettled);
                            model.HedgeBorrowAmount = contractList.Where(x => x.TradeType == TradeType.StockBorrow && x.IssueId == item.IssueId && x.ContraEntity.ToLower().StartsWith("h") && x.AmountFullSettled > 0).Sum(x => x.AmountFullSettled);
                            model.HedgeBorrowAddonRequirement = contractList.Where(x => x.IssueId == item.IssueId && x.TradeType == TradeType.StockBorrow && x.ContraEntity.ToLower().StartsWith("h") && x.AmountFullSettled > 0).Sum(x => ((x.AmountFullSettled / x.QuantityFullSettled) - x.MktPrice) * x.QuantityFullSettled) * -1;

                            model.LoanAmount = contractList.Where(x => x.TradeType == TradeType.StockLoan && x.IssueId == item.IssueId && !x.ContraEntity.ToLower().StartsWith("h") && x.AmountFullSettled > 0).Sum(x => x.AmountFullSettled);
                            model.HedgeLoanAmount = contractList.Where(x => x.TradeType == TradeType.StockLoan && x.IssueId == item.IssueId && x.ContraEntity.ToLower().StartsWith("h") && x.AmountFullSettled > 0).Sum(x => x.AmountFullSettled);
                            model.HedgeLoanAddonRequirement = contractList.Where(x => x.IssueId == item.IssueId && x.TradeType == TradeType.StockLoan && x.ContraEntity.ToLower().StartsWith("h") && x.AmountFullSettled > 0).Sum(x => ((x.AmountFullSettled / x.QuantityFullSettled) - x.MktPrice) * x.QuantityFullSettled);

                            model.TotalAddonRequirement = model.HedgeBorrowAddonRequirement + model.HedgeLoanAddonRequirement;

                            addOnList.Add(model);
                        }
                        catch (Exception error)
                        {
                            return ThrowJsonError("SecurityNumber : " + item.SecurityNumber + ", Error : " + error.Message);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(addOnList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}