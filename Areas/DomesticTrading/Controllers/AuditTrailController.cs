using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Models;
using SLTrader.Custom;
using System.Data;
using System.Linq;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class AuditTrailController : BaseController
    {
        // GET: DomesticTrading/AutoLoanContraConfig
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_AuditTrail([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var auditTrailList = new List<AuditTrailModel>();

            if (string.IsNullOrWhiteSpace(entityId))
            {
                return Extended.JsonMax( auditTrailList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
            }

            var tradeList = DataTrades.LoadTradesExtended( effectiveDate, entityId );
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntity( effectiveDate, entityId );
            var autoBorrowList = DataAutoBorrow.LoadAutoBorrowByEntity( effectiveDate, entityId );
            var activityList = DataServerStatus.LoadActivityByEntity( effectiveDate, entityId );

            // add trades
            foreach ( var trade in tradeList.Where( x => x.TradeStatus == StatusDetail.Approved ).ToList() )
            {
                var tempAudit = new AuditTrailModel()
                {
                    EffectiveDate = (DateTime) trade.EffectiveDate,
                    EntityId = trade.EntityId,
                    ContraEntity = trade.ContraEntityId,
                    EntityType = SL_EntityType.Trade,
                    IssueId = (int) trade.IssueId,
                    SecurityNumber = trade.SecurityNumber,
                    Ticker = trade.Ticker,
                    TradeType = trade.TradeType,
                    Quantity = trade.Quantity,
                    Amount = trade.Amount,
                    RebateRate = Convert.ToDecimal( trade.RebateRate ),
                    ActionBy = activityList.Where( x => x.EntityType == SL_EntityType.Trade && x.TypeId == trade.TradeNumber ).OrderByDescending( x => x.SLActivity ).First().UserName
                };

                auditTrailList.Add( tempAudit );
            }

            // add autoborrow
            foreach ( var autoBorrow in autoBorrowList.Where( x => x.BorrowStatus == StatusMain.Settled ).ToList() )
            {
                var tempAudit = new AuditTrailModel()
                {
                    EffectiveDate = (DateTime)autoBorrow.EffectiveDate,
                    EntityId = autoBorrow.EntityId.ToString(),
                    ContraEntity = autoBorrow.ContraEntityId,
                    EntityType = SL_EntityType.AutoBorrow,
                    IssueId = (int)autoBorrow.IssueId,
                    SecurityNumber = autoBorrow.SecurityNumber,
                    Ticker = autoBorrow.Ticker,
                    TradeType = TradeType.StockBorrow,
                    Quantity = autoBorrow.Quantity,
                    Amount = autoBorrow.Quantity * autoBorrow.MaxPrice,
                    RebateRate = Convert.ToDecimal( autoBorrow.MinRebateRate ),
                    ActionBy = activityList.Where( x => x.EntityType == SL_EntityType.AutoBorrow && x.TypeId == autoBorrow.SLAutoBorrowOrder.ToString() ).OrderByDescending( x => x.SLActivity ).First().UserName
                };

                auditTrailList.Add( tempAudit );
            }

            // add autoLoan
            foreach ( var autoLoan in autoLoanList.Where( x => x.LoanStatusFlag == SL_AutoLoanOrderStatusFlag.Approved ).ToList() )
            {
                var tempAudit = new AuditTrailModel()
                {
                    EffectiveDate = (DateTime)autoLoan.EffectiveDate,
                    EntityId = autoLoan.EntityId.ToString(),
                    ContraEntity = autoLoan.ContraEntityId,
                    EntityType = SL_EntityType.AutoLoan,
                    IssueId = (int)autoLoan.IssueId,
                    SecurityNumber = autoLoan.SecurityNumber,
                    Ticker = autoLoan.Ticker,
                    TradeType = TradeType.StockLoan,
                    Quantity = (decimal) autoLoan.ApprovedQuantity,
                    Amount = (decimal) autoLoan.ApprovedAmount,
                    RebateRate = (decimal) autoLoan.ApprovedRebateRate,
                    ActionBy = activityList.Where( x => x.EntityType == SL_EntityType.AutoLoan && x.TypeId == autoLoan.SLAutoLoanOrder.ToString() ).OrderByDescending( x => x.SLActivity ).First().UserName
                };

                auditTrailList.Add( tempAudit );
            }

            return Extended.JsonMax( auditTrailList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
    }
}