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

namespace SLTrader.Areas.Locates.Controllers
{
    public class LocatesController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadLocateShort([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_LocateShortSummaryProjection> locateList = DataLocate.LoadLocateByShorts(effectiveDate, entityId);

            return Extended.JsonMax(locateList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
 
        public ActionResult LoadLocates([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_LocateProjection> locateList = DataLocate.LoadLocate(effectiveDate, entityId);

            if (SessionSecurityService.IsEditable(ManagerTask.ViewSLLocateClientRestricted) && SessionSecurityService.IsEditable(ManagerTask.ViewSLLocateByUser))
            {
                locateList = locateList.Where(x => x.RequestedBy.ToLower().Equals(SessionService.User.UserName.ToLower())).ToList();
            }

            return Extended.JsonMax(locateList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadPendingLocatesCount(string entityId)
        {
            List<SL_LocateProjection> locateList = DataLocate.LoadLocate(DateCalculator.Default.Today, entityId);

            var pendingItems = 0;

            try
            {
                pendingItems = locateList.Count(x => ((x.LocateStatus == SL_LocateStatus.Pend) && (x.StatusMain == StatusMain.Settled)));
            }
            catch
            {
                pendingItems = 0;
            }

            return Extended.JsonMax(pendingItems, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadLocateTotal([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_LocateProjection> locateList = DataLocate.LoadLocate(effectiveDate, entityId).Where(x => x.IssueId == issueId).ToList();

            List<LocateTotalModel> totalList = locateList.GroupBy(l => new
                                                {
                                                    l.EffectiveDate,
                                                    l.EntityId,
                                                    l.Clientid,
                                                    l.IssueId
                                                })
                                                .Select(s => new LocateTotalModel()
                                                {
                                                    ClientId = s.Key.Clientid,
                                                    IssueId = s.Key.IssueId,
                                                    Located = (s.Sum(c => c.AllocatedQuantity ?? 0)),
                                                    Requested = (s.Sum(c => c.RequestQuantity))
                                                }).ToList();

            return Extended.JsonMax(totalList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadInventoryByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {            
            List<SL_InventoryProjection> list = DataInventory.LoadInventoryByIssue(effectiveDate, entityId, issueId);

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LocateLocateClient([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_LocateClient> locateClientList = DataLocate.LoadLocateClient(entityId);

            return Extended.JsonMax(locateClientList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadLocateClientDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_LocateClient> locateClientList = DataLocate.LoadLocateClient(entityId);

            return Extended.JsonMax(locateClientList, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadLocateList(string entityId)
        {
            return PartialView("~/Areas/Locates/Views/Locates/_LocateList.cshtml", entityId);
        }

        public PartialViewResult LoadSingleLocate(string entityId)
        {
            LocateCustom model = new LocateCustom();
            model.EntityId = entityId;

            return PartialView("~/Areas/Locates/Views/Shared/Templates/_LocateCustom.cshtml", model);
        }

        public JsonResult AddSingleLocateList(List<LocateCustom> modelList)
        {
            foreach (var item in modelList)
            {
                try
                {
                    SL_Locate _locate = new SL_Locate();
                    var issue = DataIssue.LoadIssue(item.Ticker);
                    var issuePrice = DataIssue.LoadIssuePrice(item.EntityId, issue.IssueId);

                    _locate.IssueId = issue.IssueId;
                    _locate.Price = issuePrice.CurrentCashPrice ?? 0;
                    _locate.EffectiveDate = DateTime.Today;
                    _locate.EntityId = item.EntityId;
                    _locate.RequestedQty = item.RequestedQuantity;
                    _locate.RequestedTime = DateTime.Now;
                    _locate.AllocBy = SessionService.SecurityContext.UserName;
                    _locate.AllocQty = item.AllocatedQuantity;
                    _locate.AllocTime = DateTime.Now;
                    _locate.ClientComment = "";
                    _locate.ClientId = item.Client;
                    _locate.Comment = string.IsNullOrWhiteSpace(item.Comment) ? "manually entered" : item.Comment + "-manually entered";
                    _locate.DateTimeId = DateTime.Now;
                    _locate.ModifiedBy = SessionService.SecurityContext.UserName;
                    _locate.RequestedTime = DateTime.Now;
                    _locate.TradeXref = string.IsNullOrWhiteSpace(item.TradeXref) ? "" : item.TradeXref;
                    _locate.RebateRate = 0;
                    _locate.StatusMain = StatusMain.Settled;
                    _locate.RequestedBy = SessionService.SecurityContext.UserName;

                    if (item.AllocatedQuantity == 0)
                    {
                        _locate.StatusId = SL_LocateStatus.None;
                    }
                    else if (item.RequestedQuantity > item.AllocatedQuantity)
                    {
                        _locate.StatusId = SL_LocateStatus.Part;
                    }
                    else if (item.RequestedQuantity == item.AllocatedQuantity)
                    {
                        _locate.StatusId = SL_LocateStatus.Full;
                    }

                    SL_Inventory inventory = new SL_Inventory();

                    if (item.AllocatedQuantity > 0)
                    {
                        inventory.EffectiveDate = DateTime.Today;
                        inventory.EntityId = item.EntityId;
                        inventory.IssueId = issue.IssueId;
                        inventory.Quantity = item.AllocatedQuantity;
                        inventory.RebateRate = null;
                        inventory.Source = item.Source;
                    }
                    else
                    {
                        inventory = null;
                    }

                    SL_LocateInventory locateInventory = new SL_LocateInventory();

                    if (item.AllocatedQuantity > 0)
                    {
                        locateInventory.EffectiveDate = DateTime.Today;
                        locateInventory.EntityId = item.EntityId;
                        locateInventory.IssueId = issue.IssueId;
                        locateInventory.Client = item.Client;
                        locateInventory.AllocQty = item.AllocatedQuantity;
                        locateInventory.Source = item.Source;
                    }
                    else
                    {
                        locateInventory = null;
                    }

                    DataLocate.LocateCustom(_locate, inventory, locateInventory);
                }
                catch (Exception e)
                {
                    return ThrowJsonError(e);
                }
            }

            return Extended.JsonMax("Success!", JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddSingleLocate(LocateCustom model)
        {
            try
            {
                SL_Locate _locate = new SL_Locate();
                var issue = DataIssue.LoadIssue(model.Ticker);
                var issuePrice = DataIssue.LoadIssuePrice(model.EntityId, issue.IssueId);

                _locate.IssueId = issue.IssueId;
                _locate.Price = issuePrice.CurrentCashPrice ?? 0;
                _locate.EffectiveDate = DateTime.Today;
                _locate.EntityId = model.EntityId;
                _locate.RequestedQty = model.RequestedQuantity;
                _locate.RequestedTime = DateTime.Now;
                _locate.AllocBy = SessionService.SecurityContext.UserName;
                _locate.AllocQty = model.AllocatedQuantity;
                _locate.AllocTime = DateTime.Now;
                _locate.ClientComment = "";
                _locate.ClientId = model.Client;
                _locate.Comment = string.IsNullOrWhiteSpace(model.Comment) ? "manually entered" : model.Comment + "-manually entered";
                _locate.DateTimeId = DateTime.Now;
                _locate.ModifiedBy = SessionService.SecurityContext.UserName;
                _locate.RequestedTime = DateTime.Now;
                _locate.TradeXref = string.IsNullOrWhiteSpace(model.TradeXref) ? "" : model.TradeXref;
                _locate.RebateRate = 0;
                _locate.StatusMain = StatusMain.Settled;
                _locate.RequestedBy = SessionService.SecurityContext.UserName;

                if (model.AllocatedQuantity == 0)
                {
                    _locate.StatusId = SL_LocateStatus.None;
                }
                else if (model.RequestedQuantity > model.AllocatedQuantity)
                {
                    _locate.StatusId = SL_LocateStatus.Part;
                }
                else if (model.RequestedQuantity == model.AllocatedQuantity)
                {
                    _locate.StatusId = SL_LocateStatus.Full;
                }

                SL_Inventory inventory = new SL_Inventory();

                if (model.AllocatedQuantity > 0)
                {
                    inventory.EffectiveDate = DateTime.Today;
                    inventory.EntityId = model.EntityId;
                    inventory.IssueId = issue.IssueId;
                    inventory.Quantity = model.AllocatedQuantity;
                    inventory.RebateRate = null;
                    inventory.Source = model.Source;
                }
                else
                {
                    inventory = null;
                }

                SL_LocateInventory locateInventory = new SL_LocateInventory();

                if (model.AllocatedQuantity > 0)
                {
                    locateInventory.EffectiveDate = DateTime.Today;
                    locateInventory.EntityId = model.EntityId;
                    locateInventory.IssueId = issue.IssueId;
                    locateInventory.Client = model.Client;
                    locateInventory.AllocQty = model.AllocatedQuantity;
                    locateInventory.Source = model.Source;
                }
                else
                {
                    locateInventory = null;
                }

                DataLocate.LocateCustom(_locate, inventory, locateInventory);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax("Success!", JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadInventoryList(string entityId)
        {
            return PartialView("~/Areas/Locates/Views/Locates/_InventoryList.cshtml", entityId);
        }

        public PartialViewResult LoadNewLocateClient(string entityId)
        {
            SL_LocateClient client = new SL_LocateClient();

            client.SLLocateClient = -1;
            client.EntityId = entityId;

            return PartialView("~/Areas/Locates/Views/Shared/Templates/_LocateClient.cshtml", client);
        }

        public PartialViewResult LoadUpdateLocateClient(string entityId, string client)
        {
            SL_LocateClient locateClient = DataLocate.LoadLocateClient(entityId).Where(x => x.Client.Equals(client)).Single();

            return PartialView("~/Areas/Locates/Views/Shared/Templates/_LocateClient.cshtml", locateClient);
        }

        public PartialViewResult LoadCancelLocatePartial(List<SL_LocateProjection> list)
        {
            return PartialView("~/Areas/Locates/Views/Locates/_CancelLocate.cshtml", list);
        }


        public bool AddLocateClient(SL_LocateClient client)
        {

            try
            {
                ModelState.Clear();

                DataLocate.AddLocateClient(client);
            }
            catch (Exception e)
            {
                ThrowJsonError(e);

                return false;
            }

            return true;
        }

        public ActionResult Update_LocateClient([DataSourceRequest] DataSourceRequest request, SL_LocateClient client)
        {
            SL_LocateClient locateClient = new SL_LocateClient();

            try
            {
                locateClient = DataLocate.LoadLocateClient(client.SLLocateClient);

                locateClient.AllowEasy = client.AllowEasy;
                locateClient.AllowNoLend = client.AllowNoLend;
                locateClient.AllowRestricted = client.AllowRestricted;
                locateClient.AllowThreshold = client.AllowThreshold;
                locateClient.AutoApprovalQty = client.AutoApprovalQty;
                locateClient.ClientTypeId = client.ClientTypeId;
                locateClient.MinPrice = client.MinPrice;
                locateClient.MinQty = client.MinQty;
                locateClient.InventoryPercentage = client.InventoryPercentage;

                DataLocate.UpdateLocateClient(locateClient);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { locateClient }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadLocateList(string entityId, string clientId, string locateList, string tradeXref,string clientComment)
        {
            var count = 0;
            var displayMsg = "";
            List<ParsedListModel> parsedList = new List<ParsedListModel>();

            try
            {
                parsedList = ListParsingService.GenerateList(entityId, locateList, "");

                foreach (var item in parsedList.Where(x => x.IssueId == -1).ToList())
                {
                    var message = item.EntityId + item.SecurityNumber + item.Quantity.ToString();
                }



                List<SL_Locate> list = ListParsingService.GenerateLocates(parsedList, clientId);

                list.RemoveAll(x => x.IssueId == -1);

                foreach (SL_Locate item in list)
                {
                    try
                    {
                        if (item.IssueId != -1)
                        {
                            item.TradeXref = string.IsNullOrWhiteSpace(tradeXref) ? "" : tradeXref;
                            item.ClientComment = clientComment;
                            item.RequestedBy = SessionService.SecurityContext.UserName;

                            DataLocate.AddLocate(item);
                            count = count + 1;
                        }
                    }
                    catch (Exception e)
                    {
                        return ThrowJsonError(e);
                    }
                }

                displayMsg = string.Format("Processed {0} items out of {1} items, with {2} errors", count, parsedList.Count(), parsedList.Count(x => x.IssueId == -1));
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new { Message = displayMsg, RejectedItems = parsedList.Where(x => x.IssueId == -1).ToList() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateLocate([DataSourceRequest] DataSourceRequest request, SL_LocateProjection locate)
        {
            try
            {
                if (locate.AllocatedQuantity > locate.RequestQuantity)
                {
                    throw new Exception("Allocated Qty cannot be greater than Requested Qty");
                }
                else
                {
                    SL_Locate locateItem = DataLocate.LoadLocate(locate.LocateId);

                    locateItem.AllocQty = locate.AllocatedQuantity;
                    locateItem.StatusMain = StatusMain.Ready;
                    locateItem.RebateRate = Convert.ToDouble(locate.RebateRate);

                    locate.AllocatedQuantity = locate.AllocatedQuantity;
                    locate.AllocatedBy = SessionService.SecurityContext.UserName;
                    locate.StatusMain = StatusMain.Ready;

                    DataLocate.UpdateLocate(locateItem);
                }
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { locate }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadInventoryList(string entityId, string source, string inventoryList)
        {
            long count = 0;
            string displayMsg = "";

            List<ParsedListModel> parsedList = new List<ParsedListModel>();


            SL_ClientEmail email = new SL_ClientEmail();

            try
            {
                email = DataNotification.LoadClientEmail(entityId).Where(x => x.Source.ToLower().Equals(source.ToLower())).First();
            }
            catch
            {
                email = new SL_ClientEmail()
                {
                    AllowInventory = false,
                    AllowLocate = false,
                    ClientId = -1,
                    EmailAddress = "temp@temp.com",
                    EntityId = entityId,
                    SLFileDownloadConfigId = Convert.ToDecimal(DataSystemValues.LoadSystemValue("ClientEmailDefaultConfig", "35")),
                    Source = source
                };

                DataNotification.AddClientEmail(email);
            }

            SL_ClientEmailAction action = new SL_ClientEmailAction()
            {
                EntityId = entityId,
                FileType = SL_FileType.INVENTORY,
                ItemProcessed = count,
                Subject = "Manual User Input By " + SessionService.SecurityContext.UserName,
                ClientEmail = Convert.ToInt32(email.SLClientEmail.ToString()),
                Source = source
            };


            DataNotification.AddClientEmailAction(action);

            try
            {
                parsedList = ListParsingService.GenerateList(entityId, inventoryList, "");

                List<SL_Inventory> list = ListParsingService.GenerateInventory(parsedList, source);

                list.RemoveAll(x => x.IssueId == -1);

                var _previousInventoryList = DataInventory.LoadInventoryBySourceItem(DateCalculator.Default.Today, entityId, source).ToList();

                var _previousAddInvewntory = new List<SL_Inventory>();
                var _previousUpdateInvewntory = new List<SL_Inventory>();

                try
                {
                    foreach (var item in list)
                    {
                        if (_previousInventoryList.Where(x => x.IssueId == item.IssueId).Any())
                        {
                            var _inventoryItem = _previousInventoryList.Where(x => x.IssueId == item.IssueId).First();

                            if (item.Quantity >= _inventoryItem.Quantity)
                            {
                                _inventoryItem.Quantity = item.Quantity;
                                _inventoryItem.SLClientEmailActionId = action.SLClientEmailAction;

                                _previousUpdateInvewntory.Add(_inventoryItem);
                            }
                        }
                        else
                        {
                            item.SLClientEmailActionId = action.SLClientEmailAction;

                            _previousAddInvewntory.Add(item);
                        }
                    }

                    if (_previousUpdateInvewntory.Count > 0)
                    {
                        DataInventory.UpdateInventory(_previousUpdateInvewntory);
                    }

                    if (_previousAddInvewntory.Count > 0)
                    {
                        DataInventory.AddInventory(_previousAddInvewntory);
                    }

                    count = list.Count();
                }
                catch
                {
                    foreach (SL_Inventory item in list)
                    {
                        try
                        {
                            if (item.IssueId != -1)
                            {
                                item.SLClientEmailActionId = action.SLClientEmailAction;
                                DataInventory.AddInventory(item);
                                count = count + 1;
                            }
                        }
                        catch
                        {

                        }
                    }
                }

                foreach (var item in parsedList.Where(x => x.IssueId == -1).ToList())
                {
                    var message = item.EntityId + item.SecurityNumber + item.Quantity.ToString();
                }

                displayMsg = string.Format("Processed {0} items out of {1} items, with {2} errors", count, parsedList.Count(), parsedList.Count(x => x.IssueId == -1));

                action.ItemProcessed = count;

                DataNotification.UpdateClientEmailAction(action);

                if (count > 0)
                {
                    SL_Activity activity = new SL_Activity()
                    {
                        EffectiveDate = DateCalculator.Default.Today,
                        EntityId = entityId,
                        Activity = string.Format("Uploaded {0} items, from Source: {1}", count, source),
                        ActivityError = "",
                        ActivityFlag = SL_ActivityFlag.Completed,
                        ActivityRequest = "",
                        ActivityResponse = "",
                        ActivitySubType = SL_ActivitySubType.None,
                        ActivityType = SL_ActivityType.Activity,
                        ExecutionSystemType = SL_ExecutionSystemType.LOCAL,
                        ContraEntity = "",
                        EntityType = SL_EntityType.InventoryUpload,
                        Amount = 0,
                        Quantity = 0,
                        Exception = "",
                        IssueId = list.Select(x => x.IssueId).First(),
                        TradeType = TradeType.StockBorrow,
                        TypeId = "",
                        UserName = SessionService.SecurityContext.UserName
                    };

                    DataActivity.AddActivity(activity);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new { Message = displayMsg, RejectedItems = parsedList.Where(x => x.IssueId == -1).ToList() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadInventoryRateList(string entityId, string inventoryRateList)
        {
            long count = 0;
            long counter = 0;
            string displayMsg = "";

            Company comapny = DataEntity.LoadEntity(entityId);
            
            try
            {          
                List<SL_PendingIntradayLending> _pendingIntradayLendingList = new List<SL_PendingIntradayLending>();

                try
                {
                    foreach (var item in inventoryRateList.Split(new char[] { '\n' }))
                    {
                        try
                        {
                            var lineItem = item.Split(new char[] { ' ', '\t' }).ToList();

                            if (!string.IsNullOrWhiteSpace(lineItem[0]))
                            {
                                SL_PendingIntradayLending pendingIntraDayItem = new SL_PendingIntradayLending();

                                pendingIntraDayItem.EffectiveDate = DateTime.Today;
                                pendingIntraDayItem.StatusMain = StatusMain.Ready;
                                pendingIntraDayItem.AstecId = counter.ToString("000000");
                                pendingIntraDayItem.LoanRateMax = lineItem[1];
                                pendingIntraDayItem.LoanRateAvg = lineItem[1];
                                pendingIntraDayItem.LoanRateMin = lineItem[1];
                                pendingIntraDayItem.LoanRateStdDev = "0";
                                pendingIntraDayItem.MarketValueUSD = "0";
                                
                                pendingIntraDayItem.Ticker = "";
                                pendingIntraDayItem.LoanStage = LoanStageId.ToLoanStageId(SL_LoanStage.Outstanding);
                                pendingIntraDayItem.CollateralCurrencyId = "USD";
                                pendingIntraDayItem.Cusip = lineItem[0];
                                pendingIntraDayItem.CollateralType = CollateralTypeId.ToCollateralTypeId(SL_CollateralFlag.C);
                                pendingIntraDayItem.ContractType = ContractTypeId.ToContractType(SL_ContractType.OpenEndedOvernight);
                                pendingIntraDayItem.Age = "0";
                                pendingIntraDayItem.TradingDate = DateTime.Today.ToString("yyyy-MM-dd");
                                pendingIntraDayItem.Units = "1";
                                pendingIntraDayItem.Tickets = "0";
                                pendingIntraDayItem.ISIN = lineItem[0];
                                pendingIntraDayItem.Data = "";
                                pendingIntraDayItem.MemoInfo = "";

                                counter = counter + 1;
                                _pendingIntradayLendingList.Add(pendingIntraDayItem);
                            }
                        }
                        catch
                        {

                        }

                    }


                    while (_pendingIntradayLendingList.Any())
                    {
                        var tempList = _pendingIntradayLendingList.Take(500).ToList();

                        DataInventory.AddBulkPendingIntraDayLending(tempList);
                        _pendingIntradayLendingList.RemoveRange(0, tempList.Count());

                        count = count + tempList.Count();
                    }
                }
                catch (Exception e)
                {
                    ThrowJsonError(e);
                }

                displayMsg = string.Format("Processed {0} items out of {1} items, with {2} errors", counter, inventoryRateList.Split(new char[] { '\n' }).Count(), 0);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new { Message = displayMsg, RejectedItems = new string[] { } }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadLocateHistory([DataSourceRequest] DataSourceRequest request,DateTime startDate, DateTime stopDate, string entityId, string clientId, string criteria)
        {
            List<SL_LocateProjection> locateList = new List<SL_LocateProjection>();
            int? issueId = null;

            try
            {
                if (!criteria.Equals(""))
                {
                    try
                    {
                        issueId = DataIssue.LoadIssue(criteria).IssueId;
                    }
                    catch
                    {
                        issueId = null;
                    }
                }

                if (clientId.Equals(LabelHelper.Text("OptionLabel")))
                {
                    clientId = "";
                }

                locateList = DataLocate.LoadLocateHistory(startDate, stopDate, entityId, clientId, issueId);
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(locateList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public JsonResult RerunLocates(IEnumerable<SL_LocateProjection> locateList)
        {
            var rerunList = new List<SL_Locate>();

            foreach (var item in locateList)
            {
                var originalItem = DataLocate.LoadLocate(item.LocateId);

                if (originalItem.StatusMain == StatusMain.Settled)
                {
                    SL_Locate rerunItem = new SL_Locate();
                    rerunItem.EffectiveDate = originalItem.EffectiveDate;
                    rerunItem.EntityId = originalItem.EntityId;
                    rerunItem.IssueId = originalItem.IssueId;
                    rerunItem.ClientId = originalItem.ClientId;
                    rerunItem.TradeXref = originalItem.TradeXref;
                    rerunItem.ParentLocateId = originalItem.ParentLocateId;
                    rerunItem.Price = originalItem.Price;
                    rerunItem.ClientComment = originalItem.ClientComment;
                    rerunItem.RequestedQty = originalItem.RequestedQty;
                    rerunItem.RequestedBy = originalItem.RequestedBy;
                    rerunItem.AllocQty = null;
                    rerunItem.StatusId = SL_LocateStatus.Pend;


                    DataLocate.AddLocate(rerunItem);

                    rerunList.Add(rerunItem);
                }
            }

            return Extended.JsonMax(rerunList , JsonRequestBehavior.AllowGet);
        }

        public JsonResult CancelLocates(IEnumerable<SL_LocateProjection> locateList)
        {
            var cancelList= new List<SL_Locate>();
            
            foreach (var item in locateList.Where(x => x.LocateStatus != SL_LocateStatus.Reject))
            {
                var locateItem = DataLocate.LoadLocate(item.LocateId);
                DataLocate.DeleteLocate(locateItem);

                item.AllocatedQuantity = 0;
                item.Source = "";
                item.ModifiedBy = SessionService.SecurityContext.UserName;

                cancelList.Add(locateItem);
            }

            RealTime.Broadcast.Locate(locateList.ToList());

            return Extended.JsonMax(cancelList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadLocateInventoryByLocate([DataSourceRequest] DataSourceRequest request, decimal locateId)
        {
            List<SL_LocateInventory> locateList = new List<SL_LocateInventory>();

            try
            {
                locateList = DataLocate.LoadInventoryByLocate(locateId);
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(locateList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public const string Alphabet =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string GenerateString(int size)
        {
            Random rand = new Random();

            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = Alphabet[rand.Next(Alphabet.Length)];
            }
            return new string(chars);
        }
    }
}
