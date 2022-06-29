using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Models;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class FeeTypeController : BaseController
    {
        public ActionResult Index()
        {
           
            return View();
        }

        public ActionResult Read_FeeTypes([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_FeeType> feeTypeList;
           
            try
            {
                feeTypeList = DataFeeTypes.LoadFeeTypes(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(feeTypeList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_FeeTypesDropdown(string entityId)
        {
            List<SL_FeeType> feeTypeList;

            try
            {
                feeTypeList = DataFeeTypes.LoadFeeTypes(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(feeTypeList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_FeeType(SL_FeeType feeType)
        {
            try
            {
                feeType.Description = string.IsNullOrWhiteSpace(feeType.Description) ? "" : feeType.Description;

                DataFeeTypes.AddFeeType(feeType);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { feeType }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_FeeType(SL_FeeType item)
        {
            SL_FeeType _item = new SL_FeeType();

            try
            {
                _item = DataFeeTypes.LoadFeeTypes(item.EntityId).First(x => x.SLFeeType == item.SLFeeType);

                _item.Fee = item.Fee;
                _item.Description = item.Description;

                DataFeeTypes.UpdateFeeType(_item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { _item }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_FxRate(FxRate item)
        {
            try
            {
                item.EffectiveDate = DateTime.Today;

                DataFXRate.AddFxRate(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_FxRate([DataSourceRequest] DataSourceRequest request, DateTime EffectiveDate)
        {
            List<FxRate> fxRateList = new List<FxRate>();
            try
            {
                fxRateList = DataFXRate.LoadFxRate(EffectiveDate);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(fxRateList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_IssuePriority([DataSourceRequest] DataSourceRequest request, DateTime EffectiveDate)
        {
            List<IssuePriorityModel> issuePriorityList = new List<IssuePriorityModel>();
            try
            {
                var list = DataIssue.LoadIntraDayLending(EffectiveDate);

                issuePriorityList = list.GroupBy(x => new { x.TradingDate, x.IssueId, x.Cusip, x.Ticker, x.ISIN }).Select(m => new IssuePriorityModel()
                {
                    EffectiveDate = m.Key.TradingDate,
                    Cusip = m.Key.Cusip,
                    Ticker = m.Key.Ticker,
                    SecNumber =  "",
                    IntradayRate = m.Where(b => b.Cusip == m.Key.Cusip).Select(s => s.LoanRateAvg).Average() ?? 0
                }).ToList();

                issuePriorityList.RemoveAll(x => x.IntradayRate >= 0);

            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(issuePriorityList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        

        public ActionResult Update_FxRate(FxRate item)
        {
            FxRate _item = new FxRate();

            try
            {
                _item = DataFXRate.LoadFxRateByPK(item.FxRateId);

                _item.Currencyfrom = item.Currencyfrom;
                _item.CurrencyTo = item.CurrencyTo;
                _item.FXRate = item.FXRate;
                

                DataFXRate.UpdateFxRate(_item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { _item }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_FxRate(FxRate item)
        {       
            try
            {
                FxRate _item = DataFXRate.LoadFxRateByPK(item.FxRateId);

                DataFXRate.DeleteFxRate(_item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }, JsonRequestBehavior.AllowGet);
        }
    }
}
