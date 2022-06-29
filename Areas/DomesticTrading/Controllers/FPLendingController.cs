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
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class FPLendingController : BaseController
    {
        // GET: DomesticTrading/FPLending
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Read_FPLStockMovement([DataSourceRequest] DataSourceRequest request, DateTime EffectiveDate, string EntityId)
        {
            List<SL_FPLendingStockMovementProjection> fplStockMovementList = new List<SL_FPLendingStockMovementProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(EntityId))
                {
                    fplStockMovementList = DataFPLending.LoadFPLStockMovementExtendedByEntityId(EffectiveDate, EntityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(fplStockMovementList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_FPLBalances([DataSourceRequest] DataSourceRequest request, DateTime EffectiveDate, string EntityId)
        {
            List<SL_FPLendingBalanceProjection> fplStockMovementList = new List<SL_FPLendingBalanceProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(EntityId))
                {
                    fplStockMovementList = DataFPLending.LoadFPLStockBalanceExtendedByEntityId(EffectiveDate, EntityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(fplStockMovementList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_FPLCollateralSubstitutionConfig([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_FPLCollateralSubstitution> fplCollateralSubsitutionList = new List<SL_FPLCollateralSubstitution>();

            try
            {
                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    fplCollateralSubsitutionList = DataFPLending.LoadFPLAccountCollateralSubstitutionConfigByEntity(entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(fplCollateralSubsitutionList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        public JsonResult Read_FPLAccountConfig([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_FPLAccountConfigExtendedProjection> fplAccountConfig = new List<SL_FPLAccountConfigExtendedProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    fplAccountConfig = DataFPLending.LoadFPLAccountConfigExtendedByEntity(entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(fplAccountConfig.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_FPLPositions([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_FPLendingAvailableByContraProjection> fplAccountConfig = new List<SL_FPLendingAvailableByContraProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    fplAccountConfig = DataFPLending.LoadFPLendingAvailableByContraByEntity(effectiveDate, entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(fplAccountConfig.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadFPLAccountPayoutByDateRangeAndEntity([DataSourceRequest] DataSourceRequest request, DateTime startDate, DateTime stopDate, string entityId)
        {
            List<SL_FPLendingAccountAccrualProjection> fplAccountConfig = new List<SL_FPLendingAccountAccrualProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    fplAccountConfig = DataFPLending.LoadFPLAccountPayoutByDateRangeAndEntity(startDate, stopDate, int.Parse(entityId));
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(fplAccountConfig.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadFPLAccountPayoutSummaryByDateRangeAndEntity([DataSourceRequest] DataSourceRequest request, DateTime startDate, DateTime stopDate, string entityId)
        {
            List<SL_FPLendingAccountAccrualSummaryProjection> fplAccountConfig = new List<SL_FPLendingAccountAccrualSummaryProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    fplAccountConfig = DataFPLending.LoadFPLAccountPayoutSummaryByDateRangeAndEntity(startDate, stopDate, int.Parse(entityId));
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(fplAccountConfig.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update_FPLAccountConfig(SL_FPLAccountConfigExtendedProjection accountConfig)
        {
            SL_FPLAccountConfigExtendedProjection item = new SL_FPLAccountConfigExtendedProjection();

            try
            {
                var _AccountConfig = DataFPLending.LoadFPLAccountConfigByPK(accountConfig.SLFPLAccountConfig);

                var companyId = SessionService.UserFirms.Where(x => x.Custodian == accountConfig.CustodianToDisplay).First().CompanyId;

                _AccountConfig.IsOpted = accountConfig.IsOpted;
                _AccountConfig.AllowSendConfirm = accountConfig.AllowSendConfirm;
                _AccountConfig.DebitBalance = (decimal)accountConfig.DebitBalance;
                _AccountConfig.AccountRecordType = accountConfig.AccountRecordType;
                _AccountConfig.AccountType = accountConfig.AccountType;
                _AccountConfig.AccrualGroupId = accountConfig.AccrualGroupId;
                _AccountConfig.AccrualPayout = (int)accountConfig.AccrualPayout;
                _AccountConfig.Comment = accountConfig.Comment;
                _AccountConfig.EmailAddress = accountConfig.EmailAddress;
                _AccountConfig.EntityToDisplay = companyId.ToString();
                _AccountConfig.PercentLend = accountConfig.PercentLend;
                _AccountConfig.PercentBroker = accountConfig.PercentBroker;
                _AccountConfig.PercentMark = double.Parse(accountConfig.PercentMark.ToString());
                _AccountConfig.PercentPaid = accountConfig.PercentPaid;
                _AccountConfig.IsBookInternal = accountConfig.IsBookInternal;
                _AccountConfig.BookingContra = accountConfig.BookingContra;
                _AccountConfig.Title = string.IsNullOrWhiteSpace(accountConfig.Title) ? "" : accountConfig.Title;
                _AccountConfig.FirstName = string.IsNullOrWhiteSpace(accountConfig.FirstName) ? "" : accountConfig.FirstName;
                _AccountConfig.LastName = string.IsNullOrWhiteSpace(accountConfig.LastName) ? "" : accountConfig.LastName;
                _AccountConfig.ShortName = string.IsNullOrWhiteSpace(accountConfig.ShortName) ? "" : accountConfig.ShortName;


                DataFPLending.UpdateFPLAccountConfig(_AccountConfig);

                item = DataFPLending.LoadFPLAccountConfigExtendedByPK(_AccountConfig.EntityId, _AccountConfig.SLFPLAccountConfig);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(item, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete_FPLAccountConfig(SL_FPLAccountConfigExtendedProjection accountConfig)
        {
            try
            {
                var _AccountConfig = DataFPLending.LoadFPLAccountConfigByPK(accountConfig.SLFPLAccountConfig);

                DataFPLending.DeleteFPLAccountConfig(_AccountConfig);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(accountConfig, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ForceAllocation(string entityId)
        {
            try
            {
                DataFPLending.CalculateAccountMovementAndAccrual(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult Create_FPLAccountConfig(SL_FPLAccountConfigExtendedProjection item)
        {
            SL_FPLAccountConfig accountConfig = new SL_FPLAccountConfig();

            try
            {
                var company = DataEntity.LoadEntities().Where(x => x.Custodian == item.CustodianToDisplay).First();

                accountConfig = new SL_FPLAccountConfig()
                {
                    AccountNumber = item.AccountNumber,
                    AccountRecordType = item.AccountRecordType,
                    AccountType = item.AccountType,
                    AccrualGroupId = string.IsNullOrWhiteSpace(item.AccrualGroupId) ? "-1" : item.AccrualGroupId,
                    AccrualPayout = 0,
                    AllowSendConfirm = item.AllowSendConfirm,
                    BookingContra = item.BookingContra,
                    Comment = string.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment,
                    DebitBalance = 0.0m,
                    EmailAddress = string.IsNullOrWhiteSpace(item.EmailAddress) ? "" : item.EmailAddress,
                    EntityId = item.EntityId,
                    EntityToDisplay = company.CompanyId.ToString(),
                    FirstName = string.IsNullOrWhiteSpace(item.FirstName) ? "" : item.FirstName,
                    IsBookInternal = item.IsBookInternal,
                    IsOpted = item.IsOpted,
                    LastName = string.IsNullOrWhiteSpace(item.LastName) ? "" : item.LastName,
                    PercentBroker = item.PercentBroker,
                    PercentLend = item.PercentLend,
                    PercentMark = (double)item.PercentMark,
                    PercentPaid = item.PercentPaid,
                    ShortName = string.IsNullOrWhiteSpace(item.ShortName) ? "" : item.ShortName,
                    Title = string.IsNullOrWhiteSpace(item.Title) ? "" : item.Title,
                    LastAccrualPayout = item.LastAccrualPayout
                };

                DataFPLending.AddFPLAccountConfig(accountConfig);

                item = DataFPLending.LoadFPLAccountConfigExtendedByPK(accountConfig.EntityId, accountConfig.SLFPLAccountConfig);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(item, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Create_FPLCollateralSubstitutionConfig(SL_FPLCollateralSubstitution item)
        {
            try
            {
                item.AccountNumber = string.IsNullOrWhiteSpace(item.AccountNumber) ? "" : item.AccountNumber;
                item.AccountType = string.IsNullOrWhiteSpace(item.AccountType) ? "" : item.AccountType;
                item.AssociatedAccount = string.IsNullOrWhiteSpace(item.AssociatedAccount) ? "" : item.AssociatedAccount;
                item.AssociatedAccountType = string.IsNullOrWhiteSpace(item.AssociatedAccountType) ? "" : item.AssociatedAccountType;
                item.Comment = string.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment;
                item.CustomerCashAccount = string.IsNullOrWhiteSpace(item.CustomerCashAccount) ? "" : item.CustomerCashAccount;
                item.CustomerCashAccountType = string.IsNullOrWhiteSpace(item.CustomerCashAccountType) ? "" : item.CustomerCashAccountType;
                item.CustomerWashAccount = string.IsNullOrWhiteSpace(item.CustomerWashAccount) ? "" : item.CustomerWashAccount;
                item.CustomerWashAccountType = string.IsNullOrWhiteSpace(item.CustomerWashAccountType) ? "" : item.CustomerWashAccountType;
                item.BankControlAccount = string.IsNullOrWhiteSpace(item.BankControlAccount) ? "" : item.BankControlAccount;
                item.BankControlAccountType = string.IsNullOrWhiteSpace(item.BankControlAccountType) ? "" : item.BankControlAccountType;

                DataFPLending.AddFPLAccountCollateralSubstitutionConfig(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(item, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update_FPLCollateralSubstitutionConfig(SL_FPLCollateralSubstitution item)
        {
            SL_FPLAccountConfig accountConfig = new SL_FPLAccountConfig();

            try
            {
                var _item = DataFPLending.LoadFPLAccountCollateralSubstitutionConfigByPk(item.SLFPLCollateralSubstitution);

                _item.AccountNumber = item.AccountNumber;
                _item.AccountType = item.AccountType;
                _item.AssociatedAccount = item.AssociatedAccount;
                _item.AssociatedAccountType = item.AssociatedAccountType;
                _item.Comment = item.Comment;
                _item.CustomerCashAccount = item.CustomerCashAccount;
                _item.CustomerCashAccountType = item.CustomerCashAccountType;
                _item.CustomerWashAccount = item.CustomerWashAccount;
                _item.CustomerWashAccountType = item.CustomerWashAccountType;
                _item.BankControlAccount = item.BankControlAccount;
                _item.BankControlAccountType = item.BankControlAccountType;

                _item.ShortName = item.ShortName;

                DataFPLending.UpdateFPLAccountCollateralSubstitutionConfig(_item);

                item = _item;
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(item, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete_FPLCollateralSubstitutionConfig(SL_FPLCollateralSubstitution item)
        {
            SL_FPLAccountConfig accountConfig = new SL_FPLAccountConfig();

            try
            {
                var _item = DataFPLending.LoadFPLAccountCollateralSubstitutionConfigByPk(item.SLFPLCollateralSubstitution);


                DataFPLending.DeleteFPLAccountCollateralSubstitutionConfig(_item);

            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(item, JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult LoadFPLendingDefaultOptionsPartial()
        {
            return PartialView("~/Areas/DomesticTrading/Views/FPLending/_FPLendingDefaultOptions.cshtml");
        }
    }
}