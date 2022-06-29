using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class QuickFilterController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_QuickFilter([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_QuickFilter> list;

            try
            {
                list = DataQuickFilter.LoadQuickFilter(entityId);
            }
            catch (Exception)
            {
                list = new List<SL_QuickFilter>();
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_QuickFilter(SL_QuickFilter quickFilter)
        {
            DataQuickFilter.AddQuickFilter(quickFilter);

            return Extended.JsonMax(new[] { quickFilter }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_QuickFilter(SL_QuickFilter quickFilter)
        {
            SL_QuickFilter tempQuickFilter =  DataQuickFilter.LoadQuickFilter(quickFilter.SLQuickFilter);

            tempQuickFilter.FilterName = quickFilter.FilterName;
            tempQuickFilter.Field = quickFilter.Field;
            tempQuickFilter.GridName = quickFilter.GridName;
            tempQuickFilter.Value = quickFilter.Value;
            tempQuickFilter.OperatorId = quickFilter.OperatorId;
            tempQuickFilter.Logic = quickFilter.Logic;
            DataQuickFilter.UpdateQuickFilter(tempQuickFilter);

            return Extended.JsonMax(new[] { quickFilter }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_QuickFilter(SL_QuickFilter quickFilter)
        {
            SL_QuickFilter tempQuickFilter = DataQuickFilter.LoadQuickFilter(quickFilter.SLQuickFilter);

            DataQuickFilter.DeleteQuickFilter(tempQuickFilter);

            return Extended.JsonMax(new[] { quickFilter }, JsonRequestBehavior.AllowGet);
        }
    }
}