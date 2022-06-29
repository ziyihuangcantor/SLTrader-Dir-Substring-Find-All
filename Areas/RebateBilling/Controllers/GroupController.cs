using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.RebateBilling.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class GroupController : BaseController
    {      
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_Groups([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_RebateBillingGroup> groupList;

            try
            {
                groupList = DataRebateBilling.LoadRebateBillingGroups(entityId);
            }
            catch(Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(groupList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_GroupsDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_RebateBillingGroup> groupList;

            try
            {
                groupList = DataRebateBilling.LoadRebateBillingGroups(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(groupList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_Group([DataSourceRequest] DataSourceRequest request, SL_RebateBillingGroup rebateGroup)
        {
            SL_RebateBillingGroup _RebateGroup;

            try
            {
               _RebateGroup = DataRebateBilling.LoadRebateGroup(rebateGroup.SLRebateBillingGroup);

               _RebateGroup.GroupBillingCreditMarkup = rebateGroup.GroupBillingCreditMarkup;
               _RebateGroup.GroupBillingDebitMarkup = rebateGroup.GroupBillingDebitMarkup;
               _RebateGroup.GroupBillingType = rebateGroup.GroupBillingType;
               _RebateGroup.GroupEmailAddress = rebateGroup.GroupEmailAddress;
               _RebateGroup.GroupRebateType = rebateGroup.GroupRebateType;
               _RebateGroup.Name = rebateGroup.Name;
                _RebateGroup.UseCollateralPrice = rebateGroup.UseCollateralPrice;

               DataRebateBilling.UpdateRebateGroup(_RebateGroup);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { _RebateGroup }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create_Group([DataSourceRequest] DataSourceRequest request, SL_RebateBillingGroup rebateGroup)
        {
           try
            {
                DataRebateBilling.AddRebateGroup(rebateGroup);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

           return Json(new[] { rebateGroup }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Delete_Group([DataSourceRequest] DataSourceRequest request, SL_RebateBillingGroup rebateGroup)
        {
           try
            {
                //DataRebateBilling.AddRebateGroup(rebateGroup);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

           return Json(new[] { rebateGroup }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Read_GroupAccounts([DataSourceRequest] DataSourceRequest request, string entityId, int rebatebillingGroup)
        {
            List<SL_RebateBillingGroupAccount> groupAccountList;

            try
            {
                groupAccountList = DataRebateBilling.LoadRebateGroupAccounts(entityId, rebatebillingGroup.ToString());                
            }
            catch (Exception e)
            {
                return ThrowJsonError(e.Message);
            }

            return Extended.JsonMax(groupAccountList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_GroupAccount([DataSourceRequest] DataSourceRequest request, SL_RebateBillingGroupAccount rebateGroupAccount)
        {
            SL_RebateBillingGroupAccount _RebateGroup;
            SL_RebateBillingGroupAccountProjection projection;

            try
            {
                _RebateGroup = DataRebateBilling.LoadRebateGroupAccount(rebateGroupAccount);
                
                _RebateGroup.AccountBillingType = rebateGroupAccount.AccountBillingType;
                _RebateGroup.AccountCreditMarkup = rebateGroupAccount.AccountCreditMarkup;
                _RebateGroup.AccountDebitMarkup = rebateGroupAccount.AccountDebitMarkup;
                _RebateGroup.AccountEmailAddress = rebateGroupAccount.AccountEmailAddress;
                _RebateGroup.AccountName = rebateGroupAccount.AccountName;
                _RebateGroup.AccountRebateType = rebateGroupAccount.AccountRebateType;
                _RebateGroup.IsEnabled = rebateGroupAccount.IsEnabled;
                _RebateGroup.UseCollateralPrice = rebateGroupAccount.UseCollateralPrice;
                _RebateGroup.StartDate = rebateGroupAccount.StartDate;
                _RebateGroup.StopDate = rebateGroupAccount.StopDate;
                DataRebateBilling.UpdateRebateGroupAccount(_RebateGroup);

                projection = DataRebateBilling.LoadRebateGroupAccountProjection(rebateGroupAccount);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { projection }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create_GroupAccount([DataSourceRequest] DataSourceRequest request, SL_RebateBillingGroupAccount rebateGroupAccount)
        {
            try
            {
                DataRebateBilling.AddRebateGroupAccount(rebateGroupAccount);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { rebateGroupAccount }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_GroupAccount([DataSourceRequest] DataSourceRequest request, SL_RebateBillingGroupAccountProjection rebateGroupAccount)
        {
            try
            {
                //DataRebateBilling.AddRebateGroup(rebateGroup);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { rebateGroupAccount }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult UpdateBulkAccountEditor(string groupCode, SL_RebateType? rebateType, SL_RebateBillingType? billingType, decimal? debitMarkup, decimal? creditMarkup, IEnumerable<SL_RebateBillingGroupAccountProjection> list)
        {
            
            if (list.Any())
            {
                foreach(SL_RebateBillingGroupAccountProjection item in list)
                {
                    List<SL_RebateBillingGroup> groupList = DataRebateBilling.LoadRebateBillingGroups(item.EntityId);

                    if (!String.IsNullOrWhiteSpace(groupCode))
                    {
                                             
                        item.RebateBillingGroup = Convert.ToInt32(groupCode);
                        item.RebateBillingGroupCode = groupList.Where(x => x.SLRebateBillingGroup == Convert.ToDecimal(groupCode)).Select(x => x.GroupCode).Single();

                    }

                    if (rebateType != null)
                    {
                        item.AccountRebateType = rebateType;
                    }

                    if (billingType!= null)
                    {
                        item.AccountBillingType = billingType;
                    }

                    if (debitMarkup != null)
                    {
                        item.AccountDebitMarkup = Convert.ToDouble(debitMarkup);
                    }

                    if (creditMarkup != null)
                    {
                        item.AccountCreditMarkup = Convert.ToDouble(creditMarkup);
                    }               
                }
            }

            return Json(list);
        }

        public JsonResult SaveBulkAccountEditor(IEnumerable<SL_RebateBillingGroupAccountProjection> list)
        {
            var itemList = new List<SL_RebateBillingGroupAccountProjection>();

            var slRebateBillingGroupAccountProjections = list as SL_RebateBillingGroupAccountProjection[] ?? list.ToArray();

            if (!slRebateBillingGroupAccountProjections.Any()) return Json(itemList);
            
            foreach (var item in slRebateBillingGroupAccountProjections)
            {
                var tempAccount = new SL_RebateBillingGroupAccount
                {
                    SLRebateBillingGroupAccount = item.SLRebateBillingGroupAccount,
                    EntityId = item.EntityId
                };

                tempAccount = DataRebateBilling.LoadRebateGroupAccount(tempAccount);

                tempAccount.AccountBillingType = (SL_RebateBillingType)item.AccountBillingType;
                tempAccount.AccountCreditMarkup = item.AccountCreditMarkup;
                tempAccount.AccountDebitMarkup = item.AccountDebitMarkup;
                tempAccount.AccountEmailAddress = item.AccountEmailAddress;
                tempAccount.AccountRebateType = (SL_RebateType)item.AccountRebateType;
                tempAccount.RebateBillingGroup = item.RebateBillingGroup;

                DataRebateBilling.UpdateRebateGroupAccount(tempAccount);

                itemList.Add(item);
            }

            return Json(itemList);
        }
    }
}