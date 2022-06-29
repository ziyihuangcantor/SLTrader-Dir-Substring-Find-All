using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using System.Linq;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class FundController : BaseController
    {
        public ActionResult Index()
        {
           
            return View();
        }

        public JsonResult Read_CostOfFund([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_CostOfFund> feeTypeList;
           
            try
            {
                feeTypeList = DataFunding.LoadCostOfFund(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(feeTypeList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Create_CostOfFund(SL_CostOfFund costOfFund)
        {
            try
            {
                DataFunding.AddCostOfFund(costOfFund);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(costOfFund, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update_CostOfFund(SL_CostOfFund costOfFund)
        {
            SL_CostOfFund _CostOfFund = new SL_CostOfFund();

           try
            {
                _CostOfFund = DataFunding.LoadCostOfFund(costOfFund.EntityId).Where(x => x.SLCOSTOFFUND == costOfFund.SLCOSTOFFUND).First();

                _CostOfFund.EffectiveDate = costOfFund.EffectiveDate;                
                _CostOfFund.Currency = costOfFund.Currency;
                _CostOfFund.TradeType = costOfFund.TradeType;
                _CostOfFund.PcFilter = costOfFund.PcFilter;                
                _CostOfFund.Fund = costOfFund.Fund;

                DataFunding.UpdateCostOfFund(_CostOfFund);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(_CostOfFund, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Delete_CostOfFund(SL_CostOfFund costOfFund)
        {
            try
            {
                var _CostOfFund = DataFunding.LoadCostOfFund(costOfFund.EntityId).Where(x => x.SLCOSTOFFUND == costOfFund.SLCOSTOFFUND).First();

                DataFunding.DeleteCostOFFund(_CostOfFund);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(costOfFund, JsonRequestBehavior.AllowGet);
        }
    }
}
