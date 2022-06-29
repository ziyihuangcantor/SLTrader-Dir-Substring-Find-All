using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;

using BondFire.Core.Dates;

using BondFire.Entities;
using BondFire.Entities.Projections;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Custom;
using SLTrader.Enums;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ContraRateSwingController : BaseController
    {
        // GET: DomesticTrading/ContractRateSwing
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Read_ContraRateSwing([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<SL_ContraRateSwing>();

            if (entityId.Equals("")) return Json(response.ToDataSourceResult(request));

            try
            {
                response = DataContraRateSwing.LoadContraRateSwing(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(response.ToDataSourceResult(request));
        }


        public JsonResult Add_ContraRateSwing(SL_ContraRateSwing item)
        {            
            try
            {
                DataContraRateSwing.AddContraRateSwing(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(item, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Update_ContraRateSwing(SL_ContraRateSwing item)
        {         
            try
            {
                var _Item = DataContraRateSwing.LoadContraRateSwing(item.SLContraRateSwing);

                _Item.Enabled = item.Enabled;
                _Item.Spread = item.Spread;
                _Item.SwingEntity = item.SwingEntity;
                _Item.ContraEntity = item.ContraEntity;

                DataContraRateSwing.UpdateContraRateSwing(_Item);

                item = _Item;
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(item, JsonRequestBehavior.AllowGet);
        }
    }
}