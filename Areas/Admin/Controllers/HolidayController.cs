using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.Admin.Controllers
{
    [SessionState( SessionStateBehavior.ReadOnly )]
    public class HolidayController : BaseController
    {
        // GET: Admin/AccountCharter
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetHoliday( [DataSourceRequest] DataSourceRequest request )
        {
            var holidayList = new List<Holiday>();
            var holidayYear = DateTime.Today.Year;

            try
            {
                holidayList = DataHoliday.LoadHolidaysByMinYear( holidayYear );
            }
            catch
            {

            }

            return Extended.JsonMax( holidayList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult DeleteHolidayById( [DataSourceRequest] DataSourceRequest request, Holiday holiday )
        {
            try
            {
                DataHoliday.DeleteHolidayById( holiday );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( new[] { holiday }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult UpdateHoliday( [DataSourceRequest] DataSourceRequest request, Holiday holiday )
        {
            try
            {
                if ( holiday.HolidayId == 0 )
                {
                    DataHoliday.AddHoliday( holiday );
                }
                else
                {
                    var _item = DataHoliday.LoadHolidayById( holiday.HolidayId );

                    _item.Description = holiday.Description;
                    _item.HolidayDate = holiday.HolidayDate;

                    DataHoliday.UpdateHoliday( _item );
                }
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( new[] { holiday }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
    }
}