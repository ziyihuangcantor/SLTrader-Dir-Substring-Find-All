using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SLTrader.Tools.Helpers;
using System.Web.SessionState;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Enums;
using SLTrader.Custom;
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class PortfolioController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult LoadAccounts([DataSourceRequest] DataSourceRequest request, string entityId, decimal portfolioId)
        {
            var accountList = new List<AccountModel>();
            int index = 0;

            try
            {
                var tempList = DataPortfolio.LoadPortfolioAvailableAccountItems(DateTime.Today, entityId, portfolioId);

                foreach (var acctModel in tempList.Select(acct => new AccountModel {AccountId = index, AccountNumber = acct}))
                {
                    accountList.Add(acctModel);

                    index++;
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);   
            }

            return Extended.JsonMax(accountList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadPortfolio([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var portfolioList = new List<SL_Portfolio>();

            try
            {
                if (!String.IsNullOrWhiteSpace(entityId))
                {

                    portfolioList = DataPortfolio.LoadPortfolio(entityId);
                }                
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(portfolioList.ToDataSourceResult(request));
        }

        public ActionResult DeletePortfolioItem([DataSourceRequest] DataSourceRequest request, SL_PortfolioItem portfolioItem)
        {
            bool success;

            try
            {
                DataPortfolio.DeletePortfolioItem(portfolioItem);
                success = true;
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(success);
        }

        public JsonResult LoadPortfolioMultiDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<PortfolioSummaryModel> portfolioList = new List<PortfolioSummaryModel>();

            try
            {
                if (!String.IsNullOrWhiteSpace(entityId))
                {
                    foreach (var _entityId in entityId.Replace("[", "").Replace("]", "").Split(new char[] { ',' }).ToList())
                    {
                        portfolioList.AddRange(DataPortfolio.LoadPortfolio(_entityId).Select(x => new PortfolioSummaryModel()
                        {
                            EntityId = _entityId.ToString(),
                            Custodian = SessionService.UserFirms.Where(q => q.CompanyId.ToString() == _entityId.ToString()).Select(q => q.Custodian).First(),
                            PortfolioName = x.Name
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(portfolioList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult LoadPortfolioDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var portfolioList = new List<SL_Portfolio>();

            try
            {
                if (!String.IsNullOrWhiteSpace(entityId))
                {

                    portfolioList = DataPortfolio.LoadPortfolio(entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(portfolioList, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadPortfolioItems([DataSourceRequest] DataSourceRequest request, decimal portfolioId)
        {
            List<SL_PortfolioItem> portfolioItems;

            try
            {
                portfolioItems = DataPortfolio.LoadPortfolioItems(portfolioId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(portfolioItems.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddPortfolio([DataSourceRequest] DataSourceRequest request, SL_Portfolio item)
        {
            try
            {
                item.Name = string.IsNullOrWhiteSpace(item.Name) ? "" : item.Name;

                DataPortfolio.AddPortfolio(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePortfolio([DataSourceRequest] DataSourceRequest request, SL_Portfolio item)
        {
            try
            {
                SL_Portfolio tempPortfolio = DataPortfolio.LoadPortfolio(item.SLPortfolio);
                tempPortfolio.TradedVsSettled = item.TradedVsSettled;
                tempPortfolio.ExcludeAccounts = item.ExcludeAccounts;
                tempPortfolio.IsActive = item.IsActive;
                tempPortfolio.Name = item.Name;
                tempPortfolio.PcFilter = item.PcFilter;
                
                DataPortfolio.UpdatePortfolio(tempPortfolio);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddPortfolioItem([DataSourceRequest] DataSourceRequest request, int portfolioId, string accountNumber)
        {
            var item = new SL_PortfolioItem {AccountNumber = accountNumber, PortfolioId = portfolioId};

            try
            {
                DataPortfolio.AddPortfolioItem(item);
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);        
        }

        public JsonResult AddPortfolioItems(int portfolioId, List<AccountModel> accounts)
        {
            var success = true;

            try
            {
                foreach (var item in accounts.Select(account => new SL_PortfolioItem {AccountNumber = account.AccountNumber, PortfolioId = portfolioId}))
                {
                    DataPortfolio.AddPortfolioItem(item);
                }
            }
            catch (Exception e)
            {
                success = true;
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(success, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeletePortfolioItems(List<SL_PortfolioItem> items)
        {
            var success = true;

            try
            {
                foreach (var item in items)
                {
                    DataPortfolio.DeletePortfolioItem(item);
                }
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(success, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadPortfolioPartial(string entityId)
        {
            return PartialView("~/Areas/DomesticTrading/Views/Portfolio/_PortfolioManagement.cshtml", entityId);
        }
    }
}
