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
    public class AutoLoanContraConfigController : BaseController
    {
        // GET: DomesticTrading/AutoLoanContraConfig
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_AutoLoanContraConfig([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var contraConfigList = new List<SL_AutoLoanContraConfig>();

            if (!entityId.Equals(""))
            {
                contraConfigList = DataAutoLoanContraConfig.LoadAutoLoanContraConfig(entityId);
            }

            return Extended.JsonMax(contraConfigList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult CreateAutoLoanContraConfig([DataSourceRequest] DataSourceRequest request, SL_AutoLoanContraConfig item)
        {
            try
            {
                item.ContraEntity = "";

                DataAutoLoanContraConfig.AddAutoLoanContraConfig( item );
            }
            catch 
            {
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }

        public ActionResult DestroyAutoLoanContraConfig( [DataSourceRequest] DataSourceRequest request, SL_AutoLoanContraConfig item )
        {
            SL_AutoLoanContraConfig temp = DataAutoLoanContraConfig.LoadAutoLoanContraConfigByPk( item.SLAutoLoanContraConfig );

            DataAutoLoanContraConfig.DeleteAutoLoanContraConfig( temp );

            return Extended.JsonMax( new[] { temp }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult UpdateAutoLoanContraConfig( [DataSourceRequest] DataSourceRequest request, SL_AutoLoanContraConfig item )
        {
            SL_AutoLoanContraConfig temp = DataAutoLoanContraConfig.LoadAutoLoanContraConfigByPk( item.SLAutoLoanContraConfig );

            temp.ContraEntity = item.ContraEntity;
            temp.AutoApprovalWithExcess = item.AutoApprovalWithExcess;
            temp.AutoRejectPriceBelow = item.AutoRejectPriceBelow;
            temp.AutoApprovalMinAmount = item.AutoApprovalMinAmount;
            temp.AutoApprovalMinQty = item.AutoApprovalMinQty;
            temp.DefaultRebateRate = item.DefaultRebateRate;
            temp.DefaultRebateRateId = ( item.DefaultRebateRate < 0 ) ? "N" : item.DefaultRebateRateId;

            DataAutoLoanContraConfig.UpdateAutoLoanContraConfig( temp );

            return Extended.JsonMax( new[] { temp }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public PartialViewResult UpdateAutoLoanContraConfigPartial(string entityId)
        {
            return PartialView("~/Areas/DomesticTrading/Views/AutoLoanContraConfig/_AutoLoanContraConfig.cshtml", entityId);
        }
    }
}