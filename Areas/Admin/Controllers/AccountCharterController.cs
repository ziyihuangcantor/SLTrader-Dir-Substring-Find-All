using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using System.Linq.Dynamic;


using BondFire.Core.Dates;
using BondFire.SunGard.Messages.Astec;
using BondFire.Entities.Projections;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Custom;
using SLTrader.Enums;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;
using SLTrader.Helpers.SessionHelper;
using System.Web.SessionState;

namespace SLTrader.Areas.Admin.Controllers
{
     [SessionState(SessionStateBehavior.ReadOnly)]
    public class AccountCharterController : BaseController
    {
        // GET: Admin/AccountCharter
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAccountCharter([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var accountList = new List<SL_AccountCharter>();

            try
            {
                if (!entityId.Equals(""))
                {
                    accountList = DataAccountCharter.LoadAccountCharterByEntityId(entityId);
                }
            }
            catch
            {
                
            }
            
            return Extended.JsonMax(accountList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DestroyAccountCharter([DataSourceRequest] DataSourceRequest request, SL_AccountCharter accountCharter)
        {
            try
            {
                DataAccountCharter.DeleteAccountCharter(accountCharter);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAccountCharter([DataSourceRequest] DataSourceRequest request, SL_AccountCharter accountCharter)
        {
            try
            {
                if (accountCharter.SLAccountCharter == -1)
                {
                    DataAccountCharter.AddAccountCharter(accountCharter);
                }
                else
                {
                    var _item = DataAccountCharter.LoadAccountCharterByPk(accountCharter.SLAccountCharter);

                    _item.AccountRecordType = accountCharter.AccountRecordType;
                    _item.AccountType = accountCharter.AccountType;
                    _item.ShortName = accountCharter.ShortName;
                    _item.AccountCategory = accountCharter.AccountCategory;

                    DataAccountCharter.UpdateAccountCharter(_item);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountCharterExtended([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var accountList = new List<SL_AccountCharterExtendedProjection>();

            try
            {
                if (!entityId.Equals(""))
                {
                    accountList = DataAccountCharter.LoadAccountCharterExtendedByEntityId(entityId);
                }
            }
            catch(Exception e)
            {

            }

            return Extended.JsonMax(accountList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DestroyAccountCharterExtended([DataSourceRequest] DataSourceRequest request, SL_AccountCharterExtendedProjection accountCharter)
        {
            try
            {
                var account = DataAccountCharter.LoadAccountCharterExtendedByPk(accountCharter.SLAccountCharterExtendedId);

                DataAccountCharter.DeleteAccountCharterExtended(account);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAccountCharterExtended([DataSourceRequest] DataSourceRequest request, SL_AccountCharterExtendedProjection accountCharter)
        {
            try
            {
                var account = DataAccountCharter.LoadAccountCharterExtendedByPk(accountCharter.SLAccountCharterExtendedId);

                account.AccountNumberHigh = accountCharter.AccountNumberHigh;
                account.AccountNumberLow = accountCharter.AccountNumberLow;
                account.SLAccountCharterExtendedId = (int)accountCharter.SLAccountCharterExtendedId;
                account.UserId = SessionService.User.UserId;
                
                DataAccountCharter.UpdateAccountCharterExtended(account);

                accountCharter.UserName = SessionService.User.UserName;
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddAccountCharterExtended([DataSourceRequest] DataSourceRequest request, SL_AccountCharterExtendedProjection accountCharter)
        {
            try
            {
                var account = new SL_AccountCharterExtended();

                var accountCodeList = DataAccountCharter.LoadAccountCodeCharterExtendedByEntityId(accountCharter.EntityId);

                account.EntityId = accountCharter.EntityId;
                account.AccountNumberHigh = accountCharter.AccountNumberHigh;
                account.AccountNumberLow = accountCharter.AccountNumberLow;

                account.SLAccountCharterExtendedId = (int)accountCodeList.Where(m => m.AccountCode == accountCharter.AccountCode).Select(x => x.SLAccountCodeCharterExtendedId).First();
                account.UserId = SessionService.User.UserId;


                accountCharter.AccountCode = accountCodeList.Where(m => m.AccountCode == accountCharter.AccountCode).Select(x => x.AccountCode).First();
                accountCharter.AccountCodeName = accountCodeList.Where(m => m.AccountCode == accountCharter.AccountCode).Select(x => x.AccountCodeName).First();
                accountCharter.UserName = SessionService.User.UserName;

                DataAccountCharter.AddAccountCharterExtended(account);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_AccountCodeExtendedDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<SL_AccountCodeCharterExtendedProjection>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                response = DataAccountCharter.LoadAccountCodeCharterExtendedByEntityId(entityId);
                //response = DataContraEntity.LoadContraEntity(entityId).Where(x => x.IsEnabled == true).ToList();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountCodeCharterExtended([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var accountList = new List<SL_AccountCodeCharterExtendedProjection>();

            try
            {
                if (!entityId.Equals(""))
                {
                    accountList = DataAccountCharter.LoadAccountCodeCharterExtendedByEntityId(entityId);
                }
            }
            catch
            {

            }

            return Extended.JsonMax(accountList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DestroyAccountCodeCharterExtended([DataSourceRequest] DataSourceRequest request, SL_AccountCodeCharterExtendedProjection accountCharter)
        {
            try
            {
                var account = DataAccountCharter.LoadAccountCodeCharterExtendedByPk(accountCharter.SLAccountCodeCharterExtendedId);

                DataAccountCharter.DeleteAccountCodeCharterExtended(account);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAccountCodeCharterExtended([DataSourceRequest] DataSourceRequest request, SL_AccountCodeCharterExtendedProjection accountCharter)
        {
            try
            {
                var account = DataAccountCharter.LoadAccountCodeCharterExtendedByPk(accountCharter.SLAccountCodeCharterExtendedId);

                account.AccountCodeName = string.IsNullOrWhiteSpace(accountCharter.AccountCodeName) ? "" : accountCharter.AccountCodeName;
                account.AccountNumberOffset = string.IsNullOrWhiteSpace(accountCharter.AccountNumberOffset) ? "" : accountCharter.AccountNumberOffset;
                account.CreditDebitId = string.IsNullOrWhiteSpace(accountCharter.CreditDebitId) ? "" : accountCharter.CreditDebitId;
                account.LongShortId = string.IsNullOrWhiteSpace(accountCharter.LongShortId) ? "" : accountCharter.LongShortId; 
                account.UserId = SessionService.User.UserId;

                DataAccountCharter.UpdateAccountCodeCharterExtended(account);

                accountCharter.UserName = SessionService.User.UserName;
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddAccountCodeCharterExtended([DataSourceRequest] DataSourceRequest request, SL_AccountCodeCharterExtendedProjection accountCharter)
        {
            try
            {
                var account = new SL_AccountCodeCharterExtended();

                account.EntityId = accountCharter.EntityId;
                account.AccountCode = accountCharter.AccountCode;
                account.AccountCodeName = string.IsNullOrWhiteSpace(accountCharter.AccountCodeName) ? "" : accountCharter.AccountCodeName;
                account.AccountNumberOffset = string.IsNullOrWhiteSpace(accountCharter.AccountNumberOffset) ? "" : accountCharter.AccountNumberOffset;
                account.CreditDebitId = string.IsNullOrWhiteSpace(accountCharter.CreditDebitId) ? "" : accountCharter.CreditDebitId;
                account.LongShortId = string.IsNullOrWhiteSpace(accountCharter.LongShortId) ? "" : accountCharter.LongShortId;
                account.UserId = SessionService.User.UserId;
                
                DataAccountCharter.AddAccountCodeCharterExtended(account);

                accountCharter.UserName = SessionService.User.UserName;
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { accountCharter }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public void AutoAssignAccounts()
         {
             DataAccountCharter.AutoAssignAccounts("42");
         }
    }
}