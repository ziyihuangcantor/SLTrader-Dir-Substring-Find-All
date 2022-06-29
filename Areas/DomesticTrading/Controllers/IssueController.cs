using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Core.Dates;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Helix.Core.Exceptions;
using BondFire.Managers;
using Kendo.Mvc.UI;
using SLTrader.Helpers;
using SLTrader.Helpers.SessionHelper;
using SLTrader.Custom;
using SLTrader.Enums;
using SLTrader.Models;
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class IssueController : BaseController
    {
        public static List<IssueLookupModel> issueLookUpList = new List<IssueLookupModel>();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Read_IssuePrice(string entityId, int issueId)
        {           
            var issuePrice = DataIssue.LoadIssuePrice(entityId, issueId);

            return Json((issuePrice.CurrentCashPrice ?? 0), JsonRequestBehavior.AllowGet);
        }



        public void AddIssueColor(IssueColorType colorType, int issueId)
        {
            try
            {
                var issueColor = DataIssue.LoadIssueColor((decimal)issueId);


                issueColor.IssueColorType = colorType;

                DataIssue.UpdateIssueColor(issueColor);
            }
            catch
            {
                DataIssue.AddIssueColor(new IssueColor()
                {
                    IssueId = (decimal)issueId,
                    IssueColorType = colorType
                });
            }
        }

        public JsonResult GetIssueEnumAvailable([DataSourceRequest] DataSourceRequest request, SecurityBrowserPanelEnum pane1, SecurityBrowserPanelEnum pane2, SecurityBrowserPanelEnum pane3, SecurityBrowserPanelEnum pane4)
        {
            var enumList = EnumExtensions.GetEnumSelectList<SecurityBrowserPanelEnum>().ToList();

            if (pane1 != SecurityBrowserPanelEnum.None)
            {
                enumList = enumList.Where(x => x.Text != pane1.ToString()).ToList();
            }

            if (pane2 != SecurityBrowserPanelEnum.None)
            {
                enumList = enumList.Where(x => x.Text != pane2.ToString()).ToList();
            }
            if (pane3 != SecurityBrowserPanelEnum.None)
            {
                enumList = enumList.Where(x => x.Text != pane3.ToString()).ToList();
            }
            if (pane4 != SecurityBrowserPanelEnum.None)
            {
                enumList = enumList.Where(x => x.Text != pane4.ToString()).ToList();
            }

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GetIssueInformationPartial(SecurityBrowserPanelEnum selectedItem)
        {
            PartialViewResult partialView = new PartialViewResult();

            switch(selectedItem)
            {
                case SecurityBrowserPanelEnum.Activity:
                    partialView =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Issue/__IssueActivity.cshtml");
                    break;

                case SecurityBrowserPanelEnum.AutoBorrow:
                    partialView =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Issue/__IssueAutoBorrow.cshtml");
                    break;

                case SecurityBrowserPanelEnum.Contracts:
                    partialView =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Issue/__IssueContracts.cshtml");
                    break;

                case SecurityBrowserPanelEnum.Inventory:
                    partialView =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Issue/__IssueInventory.cshtml");
                    break;

                case SecurityBrowserPanelEnum.Recalls:
                    partialView =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Issue/__IssueRecalls.cshtml");
                    break;

                case SecurityBrowserPanelEnum.Returns:
                    partialView =
                            PartialView(
                                "~/Areas/DomesticTrading/Views/Issue/__IssueReturnActions.cshtml");
                    break;

                case SecurityBrowserPanelEnum.None:
                    partialView = PartialView("~/Areas/DomesticTrading/Views/Issue/__IssueNone.cshtml");
                    break;
            }

            return partialView;
        }

        public ActionResult Read_IssueSearchWithMatchingIssues(string entityId, string criteria)
        {
            var item = new SecurityPanelModel();


            if (!string.IsNullOrWhiteSpace(criteria))
            {
                criteria = criteria.Trim();
            }

            try
            {
                if (StaticDataCache.SecurityPanelExistsByCriteria(criteria))
                {
                    item = StaticDataCache.SecurityPanelStaticByCriteriaGet(DateTime.Today, criteria);
                }
                else
                {
                    item.Issue = DataIssue.LoadIssue(criteria);

                    try
                    {
                        item.IssuePrice = DataIssue.LoadIssuePrice(entityId, item.Issue.IssueId);

                        try
                        {
                            IssueCategoryProjection projection = DataIssue.LoadIssueCategory(item.Issue.IssueCategoryId, item.Issue.IssueCalcType);
                            item.SecurityTypeDescription = projection.IssueSubType1Desc.Trim() + " | " + projection.IssueSubType2Desc.Trim();


                        }
                        catch
                        {
                            item.SecurityTypeDescription = "";
                        }

                        if (item.IssuePrice.IssueId != -1)
                        {
                            if (bool.Parse(DataSystemValues.LoadSystemValue("UseIssueMargin", true.ToString())))
                            {
                                double? margin = Convert.ToDouble(DataSystemValues.LoadSystemValue("IssueMargin", "102"));
                                margin = margin / 100;

                                item.IssuePrice.CurrentCashPrice = Math.Ceiling(((item.IssuePrice.CurrentCashPrice * margin) ?? 0));
                            }
                            else
                            {
                                item.IssuePrice.CurrentCashPrice = item.IssuePrice.CurrentCashPrice ?? 0;
                            }

                            item.IssuePrice.CurrentCashPrice = item.IssuePrice.CurrentCashPrice ?? 0;
                        }
                        else
                        {
                            item.IssuePrice.CurrentCashPrice = null;
                        }
                    }
                    catch (Exception)
                    { }

                    try
                    {
                        item.IssueDividendInfo = DataIssue.LoadIssueDividendInfo(item.Issue.IssueId);
                    }
                    catch
                    {
                        item.IssueDividendInfo = new IssueDividendInfo();
                    }

                    try
                    {
                        item.IssueColor = DataIssue.LoadIssueColor(item.Issue.IssueId);
                    }
                    catch
                    {
                        item.IssueColor = new IssueColor();
                    }

                    if (item.Issue != null)
                    {

                        var list = DataRegulatoryList.LoadRegulatoryListByIssue(DateTime.Today, entityId, item.Issue.IssueId.ToString());

                        item.IssueList = new IssueListModel();

                        foreach (var listItem in list)
                        {
                            switch (listItem.RegulatoryType)
                            {
                                case SLRegulatoryType.EASYBORROW:
                                    item.IssueList.Easy = true;

                                    break;

                                case SLRegulatoryType.PENALTYBOX:
                                    item.IssueList.PenaltyBox = true;

                                    break;

                                case SLRegulatoryType.RESTRICTED:
                                    item.IssueList.Restricted = true;

                                    break;

                                case SLRegulatoryType.PREMIUM:
                                    item.IssueList.Premium = true;

                                    break;

                                case SLRegulatoryType.THRESHOLD:
                                    item.IssueList.Threshold = true;

                                    break;

                                case SLRegulatoryType.OCCELIGIBLE:
                                    item.IssueList.OccEligible = true;
                                    break;

                                case SLRegulatoryType.ALLOCATION:
                                    item.IssueList.AllocationEligible = true;
                                    break;

                                default: break;
                            }
                        }

                        item.IssueList.RestrictedEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLRestricted);
                        item.IssueList.EasyEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLEasyBorrow);
                        item.IssueList.PenaltyBoxEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLPenaltyBox);
                        item.IssueList.PremiumEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLPremium);
                        item.IssueList.ThresholdEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLThreshold);
                        item.IssueList.OccEligibleEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLOCCEligible);
                        item.IssueList.AllocationEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLRestricted);

                        item.IntraDayList = StaticDataCache.IntradayLendingStaticGet(DateTime.Today, item.Issue.IssueId);

                        var issueComment = DataIssue.LoadIssueCOmmebtByEntityAndIssueId(entityId, item.Issue.IssueId);

                        if (issueComment.Any())
                        {
                            item.Comment = ((DateTime)issueComment.First().DateTimeId).ToString("yyyy-MM-dd") + " - " + issueComment.First().UserName + issueComment.First().Comment;
                        }
                        else
                        {
                            item.Comment = "";
                        }

                        if (!string.IsNullOrWhiteSpace(item.Issue.Quick) ||
                            !string.IsNullOrWhiteSpace(item.Issue.Cusip) ||
                            !string.IsNullOrWhiteSpace(item.Issue.Ticker)  ||
                            !string.IsNullOrWhiteSpace(item.Issue.ISIN) ||
                              !string.IsNullOrWhiteSpace(item.Issue.SEDOL))
                        {
                            if (SessionService.UserPreference.ShowMatchingIssues)
                            {
                                item.PossibleMatchingIssueList = DataIssue.LoadIssueByCriteria(item.Issue.Cusip, item.Issue.Ticker, item.Issue.ISIN, item.Issue.SEDOL, item.Issue.Quick);
                            }
                        }
                    }


                    ModelState.Clear();

                    try
                    {
                        issueLookUpList.Add(new IssueLookupModel()
                        {
                            Cusip = item.Issue.Cusip,
                            DateTimeLookup = DateTime.Now,
                            Description = item.Issue.Description_1,
                            ISIN = item.Issue.ISIN,
                            IssueId = item.Issue.IssueId,
                            Ticker = item.Issue.Ticker,
                            SecNumber = item.Issue.SecNumber,
                            Sedol = item.Issue.SEDOL,
                            EntityId = item.EntityId,
                            IntradayRate = item.IntraDayList.Average(x => x.LoanRateMin),
                            UserId = SessionService.SecurityContext.UserId
                        });
                    }
                    catch (Exception error)
                    {

                    }

                    StaticDataCache.SecurityPanelAdd(item);
                }

                ViewData.Model = item;
            }
            catch
            {
                var exceptionPath = "";

                if (SessionService.UserPreference.SLSecurityLayoutTypeId == SL_SecurityLayoutType.TOP)
                {
                    exceptionPath = @"~/Areas/DomesticTrading/Views/Issue/IndexTop.cshtml";
                }
                else
                {
                    exceptionPath = @"~/Areas/DomesticTrading/Views/Issue/Index.cshtml";
                }
                return PartialView(exceptionPath, new SecurityPanelModel());
            }

            string path = "";

            if (SessionService.UserPreference.SLSecurityLayoutTypeId == SL_SecurityLayoutType.TOP)
            {
                path = @"~/Areas/DomesticTrading/Views/Issue/IndexTop.cshtml";
            }
            else
            {
                path = @"~/Areas/DomesticTrading/Views/Issue/Index.cshtml";
            }


            return PartialView(path, ViewData.Model);
        }


        public ActionResult Read_IssueSearch(string entityId, string criteria)
        {
            var item = new SecurityPanelModel();

            if (!string.IsNullOrWhiteSpace(criteria))
            {
                criteria = criteria.Trim();


                try
                {
                    if (StaticDataCache.SecurityPanelExistsByCriteria(criteria))
                    {
                        item = StaticDataCache.SecurityPanelStaticByCriteriaGet(DateTime.Today, criteria);
                    }
                    else
                    {
                        item.Issue = DataIssue.LoadIssue(criteria);

                        try
                        {
                            item.IssuePrice = DataIssue.LoadIssuePrice(entityId, item.Issue.IssueId);

                            try
                            {
                                IssueCategoryProjection projection = DataIssue.LoadIssueCategory(item.Issue.IssueCategoryId, item.Issue.IssueCalcType);
                                item.SecurityTypeDescription = projection.IssueSubType1Desc.Trim() + " | " + projection.IssueSubType2Desc.Trim();


                            }
                            catch
                            {
                                item.SecurityTypeDescription = "";
                            }

                            if (item.IssuePrice.IssueId != -1)
                            {
                                if (bool.Parse(DataSystemValues.LoadSystemValue("UseIssueMargin", true.ToString())))
                                {
                                    double? margin = Convert.ToDouble(DataSystemValues.LoadSystemValue("IssueMargin", "102"));
                                    margin = margin / 100;

                                    item.IssuePrice.CurrentCashPrice = Math.Ceiling(((item.IssuePrice.CurrentCashPrice * margin) ?? 0));
                                }
                                else
                                {
                                    item.IssuePrice.CurrentCashPrice = item.IssuePrice.CurrentCashPrice ?? 0;
                                }

                                item.IssuePrice.CurrentCashPrice = item.IssuePrice.CurrentCashPrice ?? 0;
                            }
                            else
                            {
                                item.IssuePrice.CurrentCashPrice = null;
                            }
                        }
                        catch (Exception)
                        { }

                        try
                        {
                            item.IssueDividendInfo = DataIssue.LoadIssueDividendInfo(item.Issue.IssueId);
                        }
                        catch
                        {
                            item.IssueDividendInfo = new IssueDividendInfo();
                        }

                        try
                        {
                            item.IssueColor = DataIssue.LoadIssueColor(item.Issue.IssueId);
                        }
                        catch
                        {
                            item.IssueColor = new IssueColor();
                        }


                        if (item.Issue != null)
                        {

                            var list = DataRegulatoryList.LoadRegulatoryListByIssue(DateTime.Today, entityId, item.Issue.IssueId.ToString());

                            item.IssueList = new IssueListModel();

                            foreach (var listItem in list)
                            {
                                switch (listItem.RegulatoryType)
                                {
                                    case SLRegulatoryType.EASYBORROW:
                                        item.IssueList.Easy = true;

                                        break;

                                    case SLRegulatoryType.PENALTYBOX:
                                        item.IssueList.PenaltyBox = true;

                                        break;

                                    case SLRegulatoryType.RESTRICTED:
                                        item.IssueList.Restricted = true;

                                        break;

                                    case SLRegulatoryType.PREMIUM:
                                        item.IssueList.Premium = true;

                                        break;

                                    case SLRegulatoryType.THRESHOLD:
                                        item.IssueList.Threshold = true;

                                        break;

                                    case SLRegulatoryType.OCCELIGIBLE:
                                        item.IssueList.OccEligible = true;

                                        break;
                                    default: break;
                                }
                            }

                            item.IssueList.RestrictedEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLRestricted);
                            item.IssueList.EasyEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLEasyBorrow);
                            item.IssueList.PenaltyBoxEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLPenaltyBox);
                            item.IssueList.PremiumEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLPremium);
                            item.IssueList.ThresholdEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLThreshold);
                            item.IssueList.OccEligibleEditable = SessionSecurityService.IsEditable(ManagerTask.EditSLOCCEligible);

                            item.IntraDayList = StaticDataCache.IntradayLendingStaticGet(DateTime.Today, item.Issue.IssueId);

                            var issueComment = DataIssue.LoadIssueCOmmebtByEntityAndIssueId(entityId, item.Issue.IssueId);

                            if (issueComment.Any())
                            {
                                item.Comment = ((DateTime)issueComment.First().DateTimeId).ToString("yyyy-MM-dd") + " - " + issueComment.First().UserName + issueComment.First().Comment;
                            }
                            else
                            {
                                item.Comment = "";
                            }

                            if (!string.IsNullOrWhiteSpace(item.Issue.ISIN))
                            {
                                item.PossibleMatchingIssueList = DataIssue.LoadIssueList(item.Issue.ISIN);
                            }
                        }


                        ModelState.Clear();

                        try
                        {
                            issueLookUpList.Add(new IssueLookupModel()
                            {
                                Cusip = item.Issue.Cusip,
                                DateTimeLookup = DateTime.Now,
                                Description = item.Issue.Description_1,
                                ISIN = item.Issue.ISIN,
                                IssueId = item.Issue.IssueId,
                                Ticker = item.Issue.Ticker,
                                SecNumber = item.Issue.SecNumber,
                                Sedol = item.Issue.SEDOL,
                                EntityId = item.EntityId,
                                IntradayRate = item.IntraDayList.Average(x => x.LoanRateMin),
                                UserId = SessionService.SecurityContext.UserId
                            });
                        }
                        catch (Exception error)
                        {

                        }

                        StaticDataCache.SecurityPanelAdd(item);
                    }

                    ViewData.Model = item;
                }
                catch
                {
                    var exceptionPath = "";

                    if (SessionService.UserPreference.SLSecurityLayoutTypeId == SL_SecurityLayoutType.TOP)
                    {
                        exceptionPath = @"~/Areas/DomesticTrading/Views/Issue/IndexTop.cshtml";
                    }
                    else
                    {
                        exceptionPath = @"~/Areas/DomesticTrading/Views/Issue/Index.cshtml";
                    }
                    return PartialView(exceptionPath, new Issue());
                }
            }
            else
            {
                item.Comment = "";
                item.Boxcalc = new SL_BoxCalculationExtendedProjection();
                item.EffectiveDate = DateTime.Today;
                item.EntityId = entityId;
                item.IntraDayList = new List<SL_IntradayLending>();
                item.Issue = new Issue();
                item.IssueColor = new IssueColor();
                item.IssueDividendInfo = new IssueDividendInfo();
                item.IssueList = new IssueListModel();
                item.IssuePrice = new IssuePrice();
                item.MktPrice = null;
                item.PossibleMatchingIssueList = new List<Issue>();
                item.SecurityTypeDescription = "";

                ViewData.Model = item;
            }

            string path = "";

            if (SessionService.UserPreference.SLSecurityLayoutTypeId == SL_SecurityLayoutType.TOP)
            {
                path = @"~/Areas/DomesticTrading/Views/Issue/IndexTop.cshtml";
            }
            else
            {
                path = @"~/Areas/DomesticTrading/Views/Issue/Index.cshtml";
            }


            return PartialView(path, ViewData.Model);
        }

        public ActionResult Read_IssueHistoryLookup()
        {
            var itemList = issueLookUpList.Where(x => x.UserId == SessionService.SecurityContext.UserId).OrderByDescending(x => x.DateTimeLookup).Take(20);

            return PartialView("~/Areas/DomesticTrading/Views/Issue/IssueUserLookup.cshtml", itemList);
        }
       public ActionResult Read_IssueBundle(string entityId, string criteria)
       {
           var item = new IssueBundle();

           try
           {
               item.EntityId = entityId;
               item.Issue = DataIssue.LoadIssue(criteria);
               item.IssuePrice = DataIssue.LoadIssuePrice(entityId, item.Issue.IssueId);
           }
           catch
           {
                item.EntityId = entityId;
                item.Issue = DataIssue.LoadIssueById(int.Parse(criteria));
                item.IssuePrice = DataIssue.LoadIssuePrice(entityId, item.Issue.IssueId);
            }

           return PartialView("~/Areas/DomesticTrading/Views/Issue/_UpdateIssue.cshtml", item);
       }

        public ActionResult Read_IssueBundleByIssueId(string entityId, string criteria)
        {
            var item = new IssueBundle();

            try
            {
                item.EntityId = entityId;
                item.Issue = DataIssue.LoadIssueById(int.Parse(criteria));
                item.IssuePrice = DataIssue.LoadIssuePrice(item.EntityId, item.Issue.IssueId);
            }
            catch
            {
             
            }

            return PartialView("~/Areas/DomesticTrading/Views/Issue/_UpdateIssue.cshtml", item);
        }

        public ActionResult LoadIssueCommentAdd(string entityId, string criteria)
        {
            var item = new SL_IssueComment();

            try
            {
                item.EntityId = entityId;
                item.IssueId = DataIssue.LoadIssueById(int.Parse(criteria)).IssueId;
                
                item.Comment = "";
                item.UserId = SessionService.SecurityContext.UserId;
            }
            catch
            {

            }

            return PartialView("~/Areas/DomesticTrading/Views/Issue/IssueComment.cshtml", item);
        }

        public ActionResult LoadIssueRateAdd(string entityId)
        {            
            return PartialView("~/Areas/DomesticTrading/Views/Issue/IssueRate.cshtml", entityId);
        }

        public JsonResult AddIssueComment(SL_IssueComment form)
        {
            try
            {
                form.UserId = SessionService.SecurityContext.UserId;

                DataIssue.AddIssueCOmment(form);
            }
            catch (Exception error)
            {
                ThrowJsonError(error);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_IssueSnapShot(string entityId, string criteria)
       {
           var item = new InformationSnapshotModel();

           return PartialView("~/Areas/DomesticTrading/Views/Issue/_IssueInformation.cshtml", item);
       }

        public ActionResult Read_IssueList(string cusip, string ticker, string sedol, string ISIN, string quick)
        {
            var issueList = new List<Issue>();

            issueList = DataIssue.LoadIssueByCriteria(cusip, ticker, ISIN, sedol, quick);

            return PartialView("~/Areas/DomesticTrading/Views/Issue/IssueSearchResults.cshtml", issueList);
        }

        public JsonResult UpdateIssueBundle(IssueBundle issueBundle)
        {
            var issueModelItemHelper  = new ModelItemHelper();
            var issuePriceModelItemHelper = new ModelItemHelper();

            var _item = new IssueBundle();
            var activityCount = 0;

            try
            {
                _item.Issue = DataIssue.LoadIssueById(issueBundle.Issue.IssueId);
                _item.IssuePrice = DataIssue.LoadIssuePrice(issueBundle.EntityId, _item.Issue.IssueId);

                _item.Issue = issueModelItemHelper.UpdateIncludeProp(_item.Issue, issueBundle.Issue, new string[] {"Cusip","Quick","Ticker", "Description_1","ISIN", "SEDOL", "CountryIssued", "DtccEligible" });
                _item.IssuePrice = issuePriceModelItemHelper.UpdateIncludeProp(_item.IssuePrice, issueBundle.IssuePrice, new string[] { "CurrentCashPrice" });

                if (issueModelItemHelper.DiffList.Count > 0)
                {
                    DataIssue.UpdateIssue(_item.Issue);
                    DataActivity.AddIssueActivity(issueBundle.EntityId, _item.Issue.IssueId, issueModelItemHelper.DiffList, SL_ActivityFlag.Completed);
                }

                if (issuePriceModelItemHelper.DiffList.Count > 0)
                {
                    DataIssue.UpdateIssuePrice(_item.IssuePrice);
                    DataActivity.AddIssueActivity(issueBundle.EntityId, _item.Issue.IssueId, issuePriceModelItemHelper.DiffList, SL_ActivityFlag.Completed);
                }

                StaticDataCache.SecurityPanelUpdateIssue( _item.Issue, _item.IssuePrice );

                activityCount = issueModelItemHelper.DiffList.Count + issuePriceModelItemHelper.DiffList.Count;
            }
            catch (BusinessException exception)
            {
                if (issueModelItemHelper.DiffList.Count > 0)
                {
                    DataActivity.AddIssueActivity(issueBundle.EntityId, _item.Issue.IssueId, issueModelItemHelper.DiffList, SL_ActivityFlag.Failed);
                }

                if (issuePriceModelItemHelper.DiffList.Count > 0)
                {
                    DataActivity.AddIssueActivity(issueBundle.EntityId, _item.Issue.IssueId, issuePriceModelItemHelper.DiffList, SL_ActivityFlag.Failed);
                }
             
                return ThrowJsonError(ModelHelpers.ProcessBusinessException(exception));
            }
            catch (Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax(activityCount, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Load_IssueHistory([DataSourceRequest] DataSourceRequest request)
        {
            return PartialView("~/Areas/DomesticTrading/Views/Shared/_SecurityMasterHistory.cshtml");
        }

        public JsonResult LoadIssueSnapshotLookup(DateTime effectiveDate, List<string> entityIdList, string criteria, int dayCount)
        {
            IssueSnapshotModel model = new IssueSnapshotModel();

            model.Issue = DataIssue.LoadIssue(criteria);


            foreach (var item in entityIdList)
            {
                try
                {
                    model.issueContractLookupList.AddRange(DataContracts.LoadContractsByIssue(effectiveDate, effectiveDate, item, model.Issue.IssueId.ToString()));
                }
                catch (Exception e)
                {

                }

                try
                {
                    DateTime currentBizDate = effectiveDate;

                    for (int index = 0; index < dayCount; index++)
                    {                     
                        var boxItem = DataBoxCalculation.LoadBoxCalculationByIssue(currentBizDate, item, criteria);

                        if ((boxItem.EffectiveDate != null) && (boxItem.ClearingId != null))
                        {
                            model.issueBoxCalcLookupList.Add(boxItem);
                        }

                        currentBizDate = currentBizDate.AddDays(-1);
                    }
                }
                catch (Exception e)
                {

                }
            }

            try
            {
                model.issueInventoryLookupList.AddRange(DataInventory.LoadInventoryHistoryByIssue(effectiveDate, entityIdList[0], model.Issue.IssueId, dayCount*-1));

                model.issueInventoryLookupChartList = model.issueInventoryLookupList.Where(x => x.EffectiveDate == effectiveDate).ToList();
            }
            catch (Exception e)
            {

            }

            try
            {
                DateTime currentBizDate = effectiveDate;

                for (int index = 0; index < dayCount; index++)
                {
                    model.issueIntradayLendingLookupList.AddRange(DataInventory.LoadIntraDayLendingByIssue(currentBizDate, model.Issue.IssueId).GroupBy(x => new { x.TradingDate, x.CollateralType, x.CollateralCurrencyId }).Select(q => new SL_IntradayLending()
                    {
                        TradingDate = q.Key.TradingDate,
                        CollateralType = q.Key.CollateralType,
                        CollateralCurrencyId = q.Key.CollateralCurrencyId,
                        ContractType = SL_ContractType.Any,
                        LoanRateMin = q.Max(x => x.LoanRateMin),
                        LoanRateMax = q.Max(x => x.LoanRateMax),
                        LoanRateAvg = q.Max(x => x.LoanRateAvg)
                    }));

                    currentBizDate = currentBizDate.AddDays(-1);
                }
            }
            catch (Exception e)
            {

            }

            return Extended.JsonMax(new[] { model }, JsonRequestBehavior.AllowGet);
        }
    }
}
