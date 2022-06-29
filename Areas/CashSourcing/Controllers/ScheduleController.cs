using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Models;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;

namespace SLTrader.Areas.CashSourcing.Controllers
{
    public class ScheduleController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_ScheduleLists( [DataSourceRequest] DataSourceRequest request, string entityId )
        {
            List<Sl_ScheduleList> scheduleList;

            try
            {
                scheduleList = DataSchedule.LoadScheduleListByEntity( entityId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( scheduleList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_Schedules( [DataSourceRequest] DataSourceRequest request, string entityId, string name )
        {
            List<SL_Schedule> schedules;

            try
            {
                schedules = DataSchedule.LoadScheduleByEntityAndName( entityId, name );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( schedules.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ScheduleBoxOptimization( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            List<SL_BoxCalculationExtendedProjection> list = new List<SL_BoxCalculationExtendedProjection>();

            List<ScheduleBoxModel> boxList = new List<ScheduleBoxModel>();
            if ( entityId != "" )            
            {
                try
                {
                    list = DataBoxCalculation.LoadBoxCalculation( effectiveDate, entityId, 0, 0, false, false, false, false );
                }
                catch
                {
                    
                }

                foreach ( var item in list )
                {
                    try
                    {
                        if ( ( item.OtherBankLoanPositionSettledAmt + item.FirmBankLoanPositionSettledAmt ) > 0 )
                        {
                            Random randomList = new Random();

                            string schedule = schedules[ randomList.Next( 0, 2 ) ];
                            double scheduleMargin = schedulesMargin[ randomList.Next( 0, 2 ) ];

                            ScheduleBoxModel model = new ScheduleBoxModel();
                            model.Box = item;
                            model.OptimizedSchedule = schedule;
                            model.OptimizedAmount = ( ( item.OtherBankLoanPositionSettledAmt + item.FirmBankLoanPositionSettledAmt ) * Convert.ToDecimal( scheduleMargin ) );
                            model.OptimizedDelta = ( ( item.OtherBankLoanPositionSettledAmt + item.FirmBankLoanPositionSettledAmt ) * Convert.ToDecimal( scheduleMargin ) ) - ( item.OtherBankLoanPositionSettledAmt + item.FirmBankLoanPositionSettledAmt );

                            boxList.Add( model );
                        }
                    }
                    catch
                    {

                    }
                }
            }

            return Extended.JsonMax( boxList.AsQueryable().ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }


        private string[] schedules = new[] { "SCHEDULE A", "SCHEDULE B", "SCHEDULE C" };
        private double[] schedulesMargin = new[] { 1.35, .80, 1.00 };
    }
}
