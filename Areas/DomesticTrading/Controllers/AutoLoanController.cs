using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Threading.Tasks;
using BondFire.Entities;
using BondFire.Calculators;
using BondFire.Entities.Projections;
using Helix.Core;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Models;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;
using SLTrader.Factory;
using SLTrader;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class AutoLoanController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult LoadPendingAutoLoanCount( )
        {
            var dataCount = DataAutoLoan.LoadAutoLoanByDateTime( DateTime.Today, DateTime.Today ).Where( x => x.LoanStatusFlag == SL_AutoLoanOrderStatusFlag.Pending ).Count();

            return Extended.JsonMax( dataCount, JsonRequestBehavior.AllowGet );
        }

        public PartialViewResult LoadAutoLoanDefaultOptionsPartial()
        {
            return PartialView( "~/Areas/DomesticTrading/Views/AutoLoan/_AutoLoanDefaultOptions.cshtml" );
        }

        public JsonResult Read_AutoLoanSummarySubset( DateTime effectiveDate, string entityId )
        {
            var loanSumaryList = new List<SL_AutoLoanOrderSummaryProjection>();

            ModelState.Clear();

            if ( entityId.Equals( "" ) )
                return Extended.JsonMax( loanSumaryList, JsonRequestBehavior.AllowGet );
            try
            {
                loanSumaryList = DataAutoLoan.LoadAutoLoanSummary( effectiveDate, entityId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e.Message );
            }

            return Extended.JsonMax( loanSumaryList , JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_AutoLoanSummary( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            var loanSumaryList = new List<SL_AutoLoanOrderSummaryProjection>();

            ModelState.Clear();

            if ( entityId.Equals( "" ) )
                return Extended.JsonMax( loanSumaryList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
            try
            {
                loanSumaryList = DataAutoLoan.LoadAutoLoanSummary( effectiveDate, entityId );
            }
            catch ( Exception e )
            {
                DataError.LogError( "", "Read_AutoLoanSummary", e.Message );
            }

            return Extended.JsonMax( loanSumaryList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_AutoLoan( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string contraEntity, decimal pullRecords, bool pullPending)
        {
            var loanOrderList = new List<SL_AutoLoanOrderProjection>();
            ModelState.Clear();

            if ( entityId.Equals( "" ) )
                return Extended.JsonMax( loanOrderList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );

            try
            {
                if ( pullPending )
                {
                    loanOrderList = AutoLoanAllocationFactory.PullPendingRecords( effectiveDate, entityId, contraEntity, pullRecords );
                }
                else
                {
                    loanOrderList = DataAutoLoan.LoadAutoLoanByEntityAndContraEntity( effectiveDate, entityId, contraEntity );
                }
            }
            catch ( Exception e )
            {
                DataError.LogError( "", "Read_AutoLoan", e.Message );
            }

            return Extended.JsonMax( loanOrderList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public void UpdateStatus( List<SL_AutoLoanOrderProjection> list, SL_AutoLoanOrderStatusFlag status )
        {
            foreach ( var temp in list.Select( item => DataAutoLoan.LoadAutoLoanByPk( item.SLAutoLoanOrder ) ) )
            {
                temp.LoanOrderStatusFlag = status;

                DataAutoLoan.UpdateLoanOrder( temp );
            }
        }

        public ActionResult UpdateAutoLoan( [DataSourceRequest] DataSourceRequest request, SL_AutoLoanOrderProjection item )
        {
            var temp = DataAutoLoan.LoadAutoLoanByPk( item.SLAutoLoanOrder );
            try
            {
                temp.Price = Convert.ToDouble( item.Price );
                temp.ApprovedQuantity = item.ApprovedQuantity;
                temp.ApprovedRebateRate = Convert.ToDouble( item.ApprovedRebateRate );
                temp.ApprovedRebateRateId = string.IsNullOrEmpty( item.ApprovedRebateRateId ) ? "" : item.ApprovedRebateRateId;
                temp.ApprovedAmount = item.ApprovedAmount;
                temp.Mark = Convert.ToDouble( item.Mark );
                temp.MarkParameterId = item.MarkParameterId;
                temp.BatchCode = item.BatchCode;
                temp.DeliverViaCode = item.DeliverViaCode;


                try
                {
                    temp.ApprovedAmount = SLTradeCalculator.CalculateMoney( Convert.ToDouble( temp.Price ), Convert.ToDecimal( temp.ApprovedQuantity ), Convert.ToDouble( temp.Mark ) );
                }
                catch { }

                DataAutoLoan.UpdateLoanOrder( temp );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            item = DataAutoLoan.LoadAutoLoanProjectionByPk( item.EffectiveDate, item.SLAutoLoanOrder );

            return Extended.JsonMax( new[] { item }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult BulkRateChangeAutoLoan( string entityId, string contraEntityId, double rebateRate, string rebateRateId )
        {
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityId( DateTime.Today, entityId ).Where( x => x.ContraEntity == contraEntityId ).ToList();
            var count = 0;


            try
            {
                SecurityContext tempContext = SessionService.SecurityContext.Clone();

                HostingEnvironment.QueueBackgroundWorkItem( q => RateChangeAutoLoan( autoLoanList, rebateRate, rebateRateId, tempContext ) );

                count = autoLoanList.Where( x => x.LoanOrderStatusFlag == SL_AutoLoanOrderStatusFlag.Pending ).Count();
            }
            catch
            {

            }

            return Extended.JsonMax( count, JsonRequestBehavior.AllowGet );
        }


        public JsonResult BulkRateChangeAutoLoanLocal( List<SL_AutoLoanOrderProjection> items, double rebateRate, string rebateRateId )
        {
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityId( DateTime.Today, items.Select(x => x.EntityId).First() ).Where( x => items.Any (q => q.SLAutoLoanOrder == x.SLAutoLoanOrder) ).ToList();            

            try
            {
                SecurityContext tempContext = SessionService.SecurityContext.Clone();

                var returnResults = RateChangeAutoLoan( autoLoanList, rebateRate, rebateRateId, tempContext );

                foreach ( var item in items )
                {
                    item.ApprovedRebateRate = (decimal)returnResults.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).Select( x => x.ApprovedRebateRate ).First();
                    item.ApprovedRebateRateId = returnResults.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).Select( x => x.ApprovedRebateRateId ).First();
                }
            }
            catch
            {

            }

            return Extended.JsonMax( items, JsonRequestBehavior.AllowGet );
        }

        public List<SL_AutoLoanOrder> RateChangeAutoLoan( List<SL_AutoLoanOrder> autoLoanList, double rebateRate, string rebateRateId, SecurityContext securityContext )
        {
            var autoLoanUpdates = new List<SL_AutoLoanOrder>();

            foreach ( var item in autoLoanList.Where( x => x.LoanOrderStatusFlag == SL_AutoLoanOrderStatusFlag.Pending ) )
            {
                try
                {
                    item.ApprovedRebateRate = rebateRate;

                    if ( rebateRate < 0 )
                    {
                        item.ApprovedRebateRateId = "N";
                    }
                    else
                    {
                        item.ApprovedRebateRateId = rebateRateId;
                    }                
                }
                catch ( Exception e )
                {
                    item.MemoInfo = e.Message;
                    item.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Error;
                }

                autoLoanUpdates.Add( item );               
            }

            HostingEnvironment.QueueBackgroundWorkItem( work =>
            {
                DataAutoLoan.UpdateLoanOrderRange( autoLoanUpdates, securityContext );
            } );

            return autoLoanUpdates;
        }

        public PartialViewResult LoadBulkRateChangeAutoLoanPartial(string entityId, string contraEntityId, decimal rebateRate, string rebateRateId  )
        {
            AutoLoanBulkRateChangeModel model = new AutoLoanBulkRateChangeModel();
            
            model.EntityId = entityId;
            model.ContraEntityId = contraEntityId;
            model.RebateRate = 0;
            model.RebateRateId = "";

            return PartialView( "~/Areas/DomesticTrading/Views/AutoLoan/_AutoLoanBulkRateChange.cshtml", model );
        }
        
        
        public JsonResult ExecuteAutoLoan( List<SL_AutoLoanOrderProjection> items )
        {
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityId( items.ToList().First().EffectiveDate, items.ToList().First().EntityId );
            var sent = 0;

            if (items.Any(x => x.ApprovedRebateRate == null || x.ApprovedAmount == null))
            {
                return ThrowJsonError( "Cannot process, records are missing rebate rates / contract amount" );
            }
            
            foreach ( var item in items )
            {
                var temp = autoLoanList.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).First();

                SecurityContext tempContext = SessionService.SecurityContext.Clone();

                HostingEnvironment.QueueBackgroundWorkItem( q => UpdateAutoLoanRecord( temp.SLAutoLoanOrder, tempContext ) );

                sent++;
            }            

            return Extended.JsonMax(sent, JsonRequestBehavior.AllowGet);
        }



     
        public JsonResult ExecuteAutoLoanLocal( List<SL_AutoLoanOrderProjection> items )
        {
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityId( items.ToList().First().EffectiveDate, items.ToList().First().EntityId );
            var sent = 0;

            if ( items.Any( x => x.ApprovedRebateRate == null || x.ApprovedAmount == null ) )
            {
                return ThrowJsonError( "Cannot process, records are missing rebate rates / contract amount" );
            }

            foreach ( var item in items )
            {
                if ( item.Price != 0 )
                {
                    var temp = autoLoanList.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).First();

                    SecurityContext tempContext = SessionService.SecurityContext.Clone();

                    HostingEnvironment.QueueBackgroundWorkItem( q => UpdateAutoLoanRecord( temp.SLAutoLoanOrder, tempContext ) );

                    item.LoanStatusFlag = SL_AutoLoanOrderStatusFlag.Approved;
                    sent++;
                }
            }

            return Extended.JsonMax( items, JsonRequestBehavior.AllowGet );
        }

        public JsonResult ExecuteSingleAutoLoan( SL_AutoLoanOrderProjection autoLoan )
        {
            var tempContext = SessionService.SecurityContext.Clone();

            HostingEnvironment.QueueBackgroundWorkItem( q => UpdateAutoLoanRecord( autoLoan.SLAutoLoanOrder, tempContext ) );

            autoLoan.LoanStatusFlag = SL_AutoLoanOrderStatusFlag.InProgress;
         
            return Extended.JsonMax( new[] { autoLoan }, JsonRequestBehavior.AllowGet );
        }

        private void UpdateAutoLoanRecord(decimal slAutoLoanOrder, SecurityContext securityContext )
        {
            SL_AutoLoanOrderProjection update = new SL_AutoLoanOrderProjection();
            SL_AutoLoanOrder item = DataAutoLoan.LoadAutoLoanByPk( slAutoLoanOrder, securityContext );

            if ( item.LoanOrderStatusFlag == SL_AutoLoanOrderStatusFlag.Pending )
            {

                try
                {
                    AutoLoanOrderValidationService.OnSend( item );

                    DataExternalOperations.AddLoanOrder( item, securityContext );

                    item.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.InProgress;
                }
                catch ( Exception e )
                {
                    item.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Pending;
                    item.MemoInfo = e.Message;
                }

                DataAutoLoan.UpdateLoanOrder( item, securityContext );                
            }
        }  

        public JsonResult CancelSingleAutoLoan( SL_AutoLoanOrderProjection autoLoan )
        {
            var autoLoanPrimary = DataAutoLoan.LoadAutoLoanByPk( autoLoan.SLAutoLoanOrder );
            var tempContext = SessionService.SecurityContext.Clone();

            if ( autoLoanPrimary.LoanOrderStatusFlag == SL_AutoLoanOrderStatusFlag.Pending )
            {
                try
                {
                    autoLoanPrimary.ApprovedQuantity = 0;
                    autoLoanPrimary.ApprovedAmount = 0;
                    autoLoanPrimary.ApprovedRebateRate = 0;
                    autoLoanPrimary.ApprovedRebateRateId = "";

                    DataExternalOperations.AddLoanOrder( autoLoanPrimary );

                    autoLoanPrimary.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Canceled;

                }
                catch ( Exception e )
                {
                    autoLoanPrimary.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Error;
                    autoLoanPrimary.MemoInfo = e.Message;
                }
            }

            HostingEnvironment.QueueBackgroundWorkItem( q =>
                {
                    var localCopy = DataAutoLoan.LoadAutoLoanByPk( autoLoanPrimary.SLAutoLoanOrder, tempContext );

                    if ( localCopy.LoanOrderStatusFlag == SL_AutoLoanOrderStatusFlag.Pending )
                    {
                        DataAutoLoan.UpdateLoanOrder( autoLoanPrimary, tempContext );
                    }
                } );

            autoLoan.ApprovedQuantity = 0;
            autoLoan.ApprovedAmount = 0;
            autoLoan.ApprovedRebateRate = 0;
            autoLoan.ApprovedRebateRateId = "";

            autoLoan.LoanStatusFlag = SL_AutoLoanOrderStatusFlag.Canceled;

            return Extended.JsonMax( new[] { autoLoan }, JsonRequestBehavior.AllowGet );
        }

        public JsonResult ExecuteAutoLoanByContraEntity( string entityId, string contraEntity )
        {
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityAndContraEntity( DateTime.Today, entityId, contraEntity );

            var sent = ExecuteAutoLoan( autoLoanList );

            return sent;
        }

        public JsonResult CloseAutoLoanByContraEntity( string entityId, string contraEntity )
        {
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityAndContraEntity( DateTime.Today, entityId, contraEntity );


            var sent = CloseAutoLoan( autoLoanList );

            return sent;
        }

        public JsonResult CloseAutoLoan( IEnumerable<SL_AutoLoanOrderProjection> items )
        {
            var returnList = new List<SL_AutoLoanOrderProjection>();
            if ( items == null ) return Extended.JsonMax( returnList, JsonRequestBehavior.AllowGet );

            try
            {
                var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityId( items.ToList().First().EffectiveDate, items.ToList().First().EntityId );
                var sent = 0;

                var autoLoanUpdates = new List<SL_AutoLoanOrder>();

                foreach ( var item in items )
                {
                    var temp = autoLoanList.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).First();

                    if ( temp.LoanOrderStatusFlag == SL_AutoLoanOrderStatusFlag.Pending )
                    {
                        try
                        {
                            temp.ApprovedQuantity = 0;
                            temp.ApprovedAmount = 0;
                            temp.ApprovedRebateRate = 0;
                            temp.ApprovedRebateRateId = "";
                            temp.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Canceled;

                            DataAutoLoan.UpdateLoanOrder( temp );
                            DataExternalOperations.AddLoanOrder( temp );
                            
                            sent++;
                        }
                        catch ( Exception e )
                        {
                            temp.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Error;
                            temp.MemoInfo = e.Message;
                        }

                        autoLoanUpdates.Add( temp );
                        item.LoanStatusFlag = SL_AutoLoanOrderStatusFlag.Canceled;

                        returnList.Add( item );
                    }
                }              
            }
            catch (Exception error)
            {
                var _e = error.Message;
            }

            return Extended.JsonMax( returnList, JsonRequestBehavior.AllowGet );
        }

        public JsonResult AutoFillAutoLoan( List<SL_AutoLoanOrderProjection> items )
        {
            var autoLoanList = DataAutoLoan.LoadAutoLoanByEntityId( items.First().EffectiveDate, items.First().EntityId );
            var autoLoanProjList = DataAutoLoan.LoadAutoLoanByEntity( items.First().EffectiveDate, items.First().EntityId );
            var sent = 0;

            foreach ( var item in items )
            {
                var temp = autoLoanList.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).First();

                if ( temp.LoanOrderStatusFlag == SL_AutoLoanOrderStatusFlag.Pending )
                {
                    try
                    {
                        temp.ApprovedQuantity = ( ( autoLoanProjList.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).First().HouseExcess ?? 0 ) > temp.RequestedQuantity ) ? temp.RequestedQuantity : ( autoLoanProjList.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).First().HouseExcess ?? 0 );
                        temp.ApprovedAmount = SLTradeCalculator.CalculateMoney( temp.Price, (decimal)temp.ApprovedQuantity, temp.Mark );
                        temp.ApprovedRebateRate = Convert.ToDouble( ( autoLoanProjList.Where( x => x.SLAutoLoanOrder == item.SLAutoLoanOrder ).First().HouseRebateRate ?? 0 ) );
                        temp.ApprovedRebateRateId = "";

                        AutoLoanOrderValidationService.OnSend( temp );

                        DataExternalOperations.AddLoanOrder( temp );

                        temp.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Canceled;

                        sent++;
                    }
                    catch ( Exception e )
                    {
                        temp.LoanOrderStatusFlag = SL_AutoLoanOrderStatusFlag.Error;
                        temp.MemoInfo = e.Message;
                    }

                    DataAutoLoan.UpdateLoanOrder( temp );
                }
            }

            return Extended.JsonMax( sent, JsonRequestBehavior.AllowGet ); 
        }

        public JsonResult ProcessDefaultsUpdateAutoLoanOrder(
                string batchCode,
                string profitId,
                decimal mark,
                string markId,
                string deliverViaCode,
                string addToLoanet,
                bool autoPopulate )
        {
            try
            {
                DataSystemValues.UpdateValueByName( "AutoLoanBatchCode", batchCode );

                DataSystemValues.UpdateValueByName( "AutoLoanProfitId", profitId );

                DataSystemValues.UpdateValueByName( "AutoLoanMark", mark.ToString( CultureInfo.InvariantCulture ) );

                DataSystemValues.UpdateValueByName( "AutoLoanMarkId", markId );

                DataSystemValues.UpdateValueByName( "AutoLoanAddToLoanet", addToLoanet );

                DataSystemValues.UpdateValueByName( "AutoLoanDeliverViaCode", deliverViaCode );

                DataSystemValues.UpdateValueByName( "AutoLoanAutoPopulate", autoPopulate.ToString() );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( true, JsonRequestBehavior.AllowGet );
        }
    }
}
