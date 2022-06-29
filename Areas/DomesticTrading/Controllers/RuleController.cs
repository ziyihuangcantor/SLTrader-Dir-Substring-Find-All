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
    public class RuleController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRules([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_Rules> ruleList = DataRule.LoadRuleList(entityId);

            return Extended.JsonMax(ruleList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateRule([DataSourceRequest] DataSourceRequest request, SL_Rules rule)
        {
            try
            {
                DataRule.AddRule(rule);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { rule }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateRule([DataSourceRequest] DataSourceRequest request, SL_Rules rule)
        {
            try
            {
                var tempRule = DataRule.LoadRuleByPk(rule.SLRule);

                tempRule.RuleName = rule.RuleName;
                tempRule.ActivityType = rule.ActivityType;
                tempRule.ContraEntity = rule.ContraEntity;
                tempRule.Enabled = rule.Enabled;
                tempRule.Message = rule.Message;
                tempRule.Query = rule.Query;
                tempRule.ProcessType = rule.ProcessType;

                DataRule.UpdateRule(tempRule);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { rule }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteRule( [DataSourceRequest] DataSourceRequest request, SL_Rules rule )
        {
            try
            {
                var tempRule = DataRule.LoadRuleByPk( rule.SLRule );


                DataRule.DeleteRule( tempRule );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( new[] { rule }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
    }
}
