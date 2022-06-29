using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using SLTrader.Enums;
using SLTrader.Custom;
using SLTrader.Models;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;
using SLTrader.Helpers.ExportHelper;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class SmartRouteController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        /*** SmartRoute Functionality ***/
        public ActionResult Read_SmartRouteLists([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_SmartRouteList> smartRouteList;

            try
            {
                smartRouteList = DataSmartRoute.LoadSmartRouteLists(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(smartRouteList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SmartRouteListsDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var newSmartRouteList = new List<SL_SmartRouteList>();
            try
            {
                var smartRouteList = DataSmartRoute.LoadSmartRouteLists(entityId);

                newSmartRouteList.AddRange(smartRouteList.Where(item => DataSmartRoute.LoadSmartRoutes(item.EntityId, Convert.ToInt32(item.SLSmartRouteList)).Any()));
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(newSmartRouteList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_SmartRouteList([DataSourceRequest] DataSourceRequest request, SL_SmartRouteList smartRouteList)
        {

            try
            {
                DataSmartRoute.AddSmartRouteList(smartRouteList);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { smartRouteList }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Update_SmartRouteList([DataSourceRequest] DataSourceRequest request, SL_SmartRouteList smartRouteList)
        {
            try
            {
                DataSmartRoute.UpdateSmartRouteList(smartRouteList);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { smartRouteList }.ToDataSourceResult(request, ModelState));            
        }

        public ActionResult Delete_SmartRouteList([DataSourceRequest] DataSourceRequest request, SL_SmartRouteList smartRouteList)
        {
            try
            {
                DataSmartRoute.DeleteSmartRouteList(smartRouteList);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { smartRouteList }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Create_SmartRoute([DataSourceRequest] DataSourceRequest request, SL_SmartRoute smartRoute)
        {
            try
            {
                DataSmartRoute.AddSmartRoute(smartRoute);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { smartRoute }.ToDataSourceResult(request, ModelState));
        }

        public ActionResult Update_SmartRoute([DataSourceRequest] DataSourceRequest request, SL_SmartRoute smartRoute)
        {
            SL_SmartRoute returnRecord = new SL_SmartRoute();

            try
            {
                SL_SmartRoute tempRoute = DataSmartRoute.LoadSmartRoute(smartRoute.EntityId, smartRoute.SLSmartRoute);

                if ( smartRoute.MinRebateRate < -99 )
                {
                    smartRoute.MinRebateRate = -99;
                }

                tempRoute.AddToLoanetIndicator = smartRoute.AddToLoanetIndicator;
                tempRoute.BatchCode = smartRoute.BatchCode;
                tempRoute.ContraEntity = smartRoute.ContraEntity;
                tempRoute.DividendRate = smartRoute.DividendRate;
                tempRoute.ExecuteOrder = smartRoute.ExecuteOrder;
                tempRoute.IncomeTracked = smartRoute.IncomeTracked;
                tempRoute.Mark = smartRoute.Mark;
                tempRoute.MarkParameterId = smartRoute.MarkParameterId;
                tempRoute.MinRebateRate = smartRoute.MinRebateRate;
                tempRoute.MinRebateRateId = smartRoute.MinRebateRate < 0 ? "N" : smartRoute.MinRebateRateId.ToUpper();
                tempRoute.ProfitId = string.IsNullOrWhiteSpace( smartRoute.ProfitId ) ? "" : smartRoute.ProfitId;
                tempRoute.TimeOut = smartRoute.TimeOut;

                DataSmartRoute.UpdateSmartRoute(tempRoute);

                returnRecord = DataSmartRoute.LoadSmartRoute( smartRoute.EntityId, smartRoute.SLSmartRoute );
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json( new[] { returnRecord }.ToDataSourceResult( request, ModelState ) );            
        }

        public ActionResult Read_SmartRoutes([DataSourceRequest] DataSourceRequest request, string entityId, int smartRouteList)
        {
            List<SL_SmartRoute> smartRoutesList = new List<SL_SmartRoute>();

            try
            {
                if (!entityId.Equals("") && (smartRouteList != -1))
                {
                    smartRoutesList = DataSmartRoute.LoadSmartRoutes(entityId, smartRouteList);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(smartRoutesList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_SmartRoute([DataSourceRequest] DataSourceRequest request, SL_SmartRoute smartRoute)
        {
            try
            {
                SL_SmartRoute tempSmartRoute = DataSmartRoute.LoadSmartRoute(smartRoute.EntityId, smartRoute.SLSmartRoute);

                DataSmartRoute.DeleteSmartRoute(tempSmartRoute);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { smartRoute }.ToDataSourceResult(request, ModelState));
        }

        /***Auto Borrow Functionality ***/
        public ActionResult Read_AutoBorrowSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var borrowOrderList = new List<SL_AutoBorrowOrderSummaryProjection>();
           
            ModelState.Clear();

            if (entityId.Equals(""))
                return Extended.JsonMax(borrowOrderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            try
            {
                borrowOrderList = DataSmartRoute.LoadAutoBorrowsSummary(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(borrowOrderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);         
        }

        public JsonResult Send_AutoBorrowList([DataSourceRequest] DataSourceRequest request, string entityId, string listName)
        {
            var borrowOrderList = new List<SL_AutoBorrowOrderProjection>();
            var removeCancelled = bool.Parse(DataSystemValues.LoadSystemValue("RemoveCancelled", "false"));

            ModelState.Clear();

            if (entityId.Equals("")) return Extended.JsonMax(borrowOrderList, JsonRequestBehavior.AllowGet);

            borrowOrderList = DataAutoBorrow.LoadAutoBorrows( DateTime.Today, entityId, listName );

            borrowOrderList.RemoveAll( x => x.BorrowStatus != StatusMain.Pending );

            try
            {
                foreach ( SL_AutoBorrowOrderProjection item in borrowOrderList )
                {
                    try
                    {
                        SL_AutoBorrowOrder order = DataSmartRoute.LoadAutoBorrowOrder( item.SLAutoBorrowOrder );

                        if (order.AutoBorrowOrderSystem == SL_ExecutionSystemType.LOANET)
                        {
                            AutoBorrowOrderValidationService.OnSend(order);

                            DataExternalOperations.AddBorrowOrder(order);
                        }

                        order.BorrowOrderStatus = StatusMain.Ready;

                        DataSmartRoute.UpdateAutoBorrowOrder( order );
                    }
                    catch
                    {

                    }
                }
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax(borrowOrderList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Cancel_AutoBorrowList([DataSourceRequest] DataSourceRequest request, string entityId, string listName)
        {
            var borrowOrderList = new List<SL_AutoBorrowOrderProjection>();
           ModelState.Clear();

            if (entityId.Equals("")) return Extended.JsonMax(borrowOrderList, JsonRequestBehavior.AllowGet);
            try
            {
                borrowOrderList = DataAutoBorrow.LoadAutoBorrows(DateTime.Today, entityId, listName);

                borrowOrderList.RemoveAll(x => x.BorrowStatus != StatusMain.Pending);

                foreach (SL_AutoBorrowOrderProjection item in borrowOrderList)
                {
                    SL_AutoBorrowOrder order = DataSmartRoute.LoadAutoBorrowOrder(item.SLAutoBorrowOrder);

                    order.BorrowOrderStatus = StatusMain.Cancelled;
            
                    DataSmartRoute.UpdateAutoBorrowOrder(order);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(borrowOrderList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_AutoBorrow([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string listName)
        {
            var borrowOrderList = new List<SL_AutoBorrowOrderProjection>();
            var removeCancelled = bool.Parse(DataSystemValues.LoadSystemValue("RemoveCancelled", "false"));

            ModelState.Clear();

            if (entityId.Equals(""))
                return Extended.JsonMax(borrowOrderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            try
            {
                borrowOrderList = DataAutoBorrow.LoadAutoBorrows(effectiveDate, entityId, listName);

                if (removeCancelled)
                {
                    borrowOrderList.RemoveAll(x => x.BorrowStatus == StatusMain.Cancelled);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(borrowOrderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_AutoBorrowByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string criteria)
        {
            var borrowOrderList = new List<SL_AutoBorrowOrderProjection>();
        
        
            if (entityId.Equals(""))
                return Extended.JsonMax(borrowOrderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        
            try
            {
                Issue issue = DataIssue.LoadIssue(criteria);
                borrowOrderList = DataAutoBorrow.LoadAutoBorrowByEntity(effectiveDate, entityId).Where(x => x.IssueId == issue.IssueId).ToList();          
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(borrowOrderList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAutoBorrow([DataSourceRequest] DataSourceRequest request, SL_AutoBorrowOrderProjection item)
        {
            try
            {
                var orderTemp = DataSmartRoute.LoadAutoBorrowOrder(item.SLAutoBorrowOrder);

                if (!String.IsNullOrWhiteSpace(item.SmartRouteName))
                {
                    var item1 = item;
                    var smartRouteList = DataSmartRoute.LoadSmartRouteLists(item.EntityId.ToString(CultureInfo.InvariantCulture)).Where(x => x.Name.ToUpper().Equals(item1.SmartRouteName.ToUpper())).Take(1).Single();
                    var smartRoutes = DataSmartRoute.LoadSmartRoutes(smartRouteList.EntityId, (int)smartRouteList.SLSmartRouteList).OrderBy(x => x.ExecuteOrder).ToList();

                    orderTemp.SmartRouteList = int.Parse(smartRouteList.SLSmartRouteList.ToString(CultureInfo.InvariantCulture));

                    orderTemp.ContraEntity = smartRoutes.Take(1).Single().ContraEntity;
                    orderTemp.MinRebateRate = smartRoutes.Take(1).Single().MinRebateRate;
                    orderTemp.MinRebateRateId = smartRoutes.Take(1).Single().MinRebateRateId;
                    orderTemp.Mark = smartRoutes.Take(1).Single().Mark;
                    orderTemp.TimeOut = smartRoutes.Take(1).Single().TimeOut;
                }
                else if ( !String.IsNullOrWhiteSpace( item.ContraEntityId ) )
                {
                    orderTemp.SmartRouteList = -1;
                    orderTemp.ContraEntity = item.ContraEntityId;
                    orderTemp.Mark = Convert.ToDouble( item.Mark );
                    orderTemp.MarkParameterId = String.IsNullOrWhiteSpace( item.MarkParameterId ) ? "" : item.MarkParameterId;
                    orderTemp.MinRebateRate = ( item.MinRebateRate < -99 ) ? -99.0 : Convert.ToDouble( item.MinRebateRate );
                    orderTemp.MinRebateRateId = orderTemp.MinRebateRate < 0 ? "N" : item.MinRebateRateId.ToUpper();
                }

                orderTemp.AddToLoanetIndicator = String.IsNullOrWhiteSpace(item.AddToLoanetId) ? "" : item.AddToLoanetId;
                orderTemp.BatchCode = String.IsNullOrWhiteSpace(item.BatchCode) ? "" : item.BatchCode;
                orderTemp.BorrowOrderStatus = item.BorrowStatus;
                orderTemp.CollateralFlag = item.CollateralFlag;
                orderTemp.Comments = String.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment;
                orderTemp.DividendRate = Convert.ToDouble(item.DividendRate);
                orderTemp.EffectiveDate = DateTime.Today;
                orderTemp.EntityId = item.EntityId.ToString(CultureInfo.InvariantCulture);
                orderTemp.IncomeTracked = item.IncomeTracked;
                orderTemp.IssueId = item.IssueId;
                orderTemp.MaxPrice = Convert.ToDouble(item.MaxPrice);
                orderTemp.MinPartialQuantity = item.MinQuantity;
                orderTemp.MinRebateRate = ( item.MinRebateRate < -99 ) ? -99.0 : Convert.ToDouble( item.MinRebateRate );
                orderTemp.MinRebateRateId = orderTemp.MinRebateRate < 0 ? "N" : item.MinRebateRateId.ToUpper();
                orderTemp.ProfitId = String.IsNullOrWhiteSpace(item.ProfitId) ? "" : item.ProfitId;
                orderTemp.Quantity = item.Quantity;
                orderTemp.SLAutoBorrowOrder = item.SLAutoBorrowOrder;
                orderTemp.SubmissionType = String.IsNullOrWhiteSpace(item.SubmissionType) ? "" : item.SubmissionType;
                orderTemp.TimeOut = String.IsNullOrWhiteSpace(item.TimeOut) ? "" : item.TimeOut;

                orderTemp = AutoBorrowOrderValidationService.SetDefaults(orderTemp);

                DataSmartRoute.UpdateAutoBorrowOrder(orderTemp);

                item = DataAutoBorrow.LoadBorrowOrder(DateTime.Today, item.EntityId.ToString(CultureInfo.InvariantCulture), orderTemp.SLAutoBorrowOrder);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAutoBorrowHelper([DataSourceRequest] DataSourceRequest request, AutoBorrowOrderSuggestionModel item)
        {
            if (String.IsNullOrWhiteSpace(item.SmartRouteName))
            {
                if (item.RebateRate < -99)
                {
                    item.RebateRate = -99;
                }
                return Json(new[] { item }.ToDataSourceResult(request, ModelState));
            }

            var smartRouteList = DataSmartRoute.LoadSmartRouteLists(item.EntityId).Single(x => x.Name.Equals(item.SmartRouteName));
            var smartRoutes = DataSmartRoute.LoadSmartRoutes(item.EntityId, (int)smartRouteList.SLSmartRouteList).OrderBy(x => x.ExecuteOrder).ToList();
                
            item.SmartRoute = Convert.ToInt32(smartRouteList.SLSmartRouteList);
            item.SmartRouteName = smartRouteList.Name;
                
            item.ContraEntityId = smartRoutes.Take(1).Single().ContraEntity;

            item.RebateRate = smartRoutes.Take(1).Single().MinRebateRate;
            item.Mark = Convert.ToDecimal(smartRoutes.Take(1).Single().Mark);

            return Json(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveLcorTrades( IEnumerable<TradeChangeModel> list )
        {
            if ( list == null ) return Json( list.Where( x => x.SubmissionType != StatusDetail.Approved ) );

            var tradeChangeModels = list as TradeChangeModel[] ?? list.ToArray();

            foreach ( var item in tradeChangeModels.Where( item => item.Enabled ) )
            {
                try
                {
                    var order = new SL_AutoBorrowOrder
                    {
                        EffectiveDate = DateTime.Today,
                        SmartRouteList = -1,
                        ListName = "",
                        AddToLoanetIndicator = DataSystemValues.LoadSystemValue( "AutoBorrowAddToLoanet", "Y" ),
                        BatchCode = DataSystemValues.LoadSystemValue( "AutoBorrowBatchCode", "" ),
                        BorrowOrderRequest = "",
                        BorrowOrderResponse = "",
                        AutoBorrowOrderSystem = 0,
                        BorrowOrderStatus = StatusMain.Pending,
                        BorrowOrderStatusFlag = SL_AutoBorrowOrderStatusFlag.Empty,
                        CollateralFlag = SL_CollateralFlag.C,
                        Comments = "",
                        ContraEntity = String.IsNullOrWhiteSpace( item.ContraEntityId ) ? "" : item.ContraEntityId,
                        DividendRate = Convert.ToDouble( DataSystemValues.LoadSystemValue( "AutoBorrowDivRate", "100" ) ),
                        EntityId = item.EntityId,
                        IncomeTracked =
                            Convert.ToBoolean( DataSystemValues.LoadSystemValue( "AutoBorrowIncomeTracked", "False" ) ),
                        IssueId = item.IssueId,
                        Mark = Convert.ToDouble( DataSystemValues.LoadSystemValue( "AutoBorrowMark", "1.02" ) ),
                        MarkParameterId = DataSystemValues.LoadSystemValue( "AutoBorrowMarkId", "%" ),
                        MaxPrice = Convert.ToDouble( item.Price ),
                        Quantity = item.Quantity
                    };

                    order.MinPartialQuantity = bool.Parse( DataSystemValues.LoadSystemValue( "AutoBorrowMatchMin", "false" ) ) ? order.Quantity : Convert.ToDecimal( DataSystemValues.LoadSystemValue( "AutoBorrowMinPartialQuantity", "100" ) );

                    order.MinRebateRate = Convert.ToDouble( item.RebateRate );
                    if ( !String.IsNullOrWhiteSpace( item.ContraEntityId ) )
                    {

                        try
                        {
                            DataContraEntity.LoadContraEntity( item.EntityId, item.ContraEntityId );

                            order.MinRebateRateId = item.RebateRate >= 0 ? DataSystemValues.LoadSystemValue( "AutoBorrowRoutingTableRateId", "" ) : "N";
                        }
                        catch
                        {
                            order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                        }
                    }
                    else
                    {
                        order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                    }

                    order.ProfitId = DataSystemValues.LoadSystemValue( "AutoBorrowProfitId", "" );
                    order.SubmissionType = "";
                    order.TimeOut = DataSystemValues.LoadSystemValue( "AutoBorrowTimeOut", "10" );

                    DataSmartRoute.AddAutoBorrowOrder( order );

                    item.SubmissionType = StatusDetail.Approved;
                }
                catch ( Exception e )
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                }
            }

            return Extended.JsonMax( tradeChangeModels.Where( x => x.SubmissionType != StatusDetail.Approved ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult HelperSave(IEnumerable<AutoBorrowOrderSuggestionModel> list)
        {
            if (list == null) return Json(list.Where(x => x.SubmissionType != StatusDetail.Approved));

            var autoBorrowOrderSuggestionModels = list as AutoBorrowOrderSuggestionModel[] ?? list.ToArray();
            foreach (var item in autoBorrowOrderSuggestionModels.Where(item => item.Enabled))
            {
                try
                {
                    var order = new SL_AutoBorrowOrder
                    {
                        EffectiveDate = DateTime.Today,
                        AutoBorrowOrderSystem = item.AutoBorrowOrderSystem,
                        AddToLoanetIndicator = DataSystemValues.LoadSystemValue("AutoBorrowAddToLoanet", "Y"),
                        BatchCode = DataSystemValues.LoadSystemValue("AutoBorrowBatchCode", ""),
                        BorrowOrderRequest = "",
                        BorrowOrderResponse = "",
                        BorrowOrderStatus = StatusMain.Pending,
                        CollateralFlag = SL_CollateralFlag.C,
                        Comments = "",
                        ContraEntity = String.IsNullOrWhiteSpace(item.ContraEntityId) ? "" : item.ContraEntityId,
                        DividendRate = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowDivRate", "100")),
                        EntityId = item.EntityId,
                        IncomeTracked =
                            Convert.ToBoolean(DataSystemValues.LoadSystemValue("AutoBorrowIncomeTracked", "False")),
                        IssueId = item.IssueId,
                        Mark = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowMark", "1.02")),
                        MarkParameterId = DataSystemValues.LoadSystemValue("AutoBorrowMarkId", "%"),
                        MaxPrice = Convert.ToDouble(item.MaxPrice),
                        Quantity = item.Quantity
                    };

                    order.MinPartialQuantity = bool.Parse(DataSystemValues.LoadSystemValue("AutoBorrowMatchMin", "false")) ? order.Quantity : Convert.ToDecimal(DataSystemValues.LoadSystemValue("AutoBorrowMinPartialQuantity", "100"));

                    order.MinRebateRate = Convert.ToDouble(item.RebateRate);

                    if (!String.IsNullOrWhiteSpace(item.ContraEntityId))
                    {

                        try
                        {
                            DataContraEntity.LoadContraEntity(item.EntityId, item.ContraEntityId);

                            order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                        }
                        catch
                        {
                            order.MinRebateRateId = item.RebateRate >= 0 ? DataSystemValues.LoadSystemValue("AutoBorrowRoutingTableRateId", "") : "N";
                        }
                    }

                    order.ProfitId = DataSystemValues.LoadSystemValue("AutoBorrowProfitId", "");
                    order.SubmissionType = "";
                    order.TimeOut = DataSystemValues.LoadSystemValue("AutoBorrowTimeOut", "10");

                    DataSmartRoute.AddAutoBorrowOrder(order);

                    item.SubmissionType = StatusDetail.Approved;
                }
                catch (Exception e)
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                }
            }

            return Extended.JsonMax(autoBorrowOrderSuggestionModels.Where(x => x.SubmissionType != StatusDetail.Approved), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Send( List<SL_AutoBorrowOrderProjection> list )
        {
            if ( list != null )
            {
                var autoBorrowList = DataSmartRoute.LoadAutoBorrowsByEntityId( list.First().EffectiveDate, list.First().EntityId.ToString() );

                foreach ( SL_AutoBorrowOrderProjection item in list )
                {
                    if ( item.BorrowStatus == StatusMain.Pending )
                    {
                        try
                        {
                            SL_AutoBorrowOrder order = autoBorrowList.Where( x => x.SLAutoBorrowOrder == item.SLAutoBorrowOrder ).First();

                            if (order.AutoBorrowOrderSystem == SL_ExecutionSystemType.LOANET)
                            {
                                AutoBorrowOrderValidationService.OnSend(order);

                                DataExternalOperations.AddBorrowOrder(order);
                            }

                            order.BorrowOrderStatus = StatusMain.Ready;

                            DataSmartRoute.UpdateAutoBorrowOrder( order );
                        }
                        catch ( Exception e )
                        {
                            return ThrowJsonError( e );
                        }
                    }
                }
            }

            return Extended.JsonMax( list, JsonRequestBehavior.AllowGet );
        }

        public JsonResult Cancel(List<SL_AutoBorrowOrderProjection> list)
        {
            if (list != null)
            {
                var autoBorrowList = DataSmartRoute.LoadAutoBorrowsByEntityId( list.First().EffectiveDate, list.First().EntityId.ToString() );

                foreach (SL_AutoBorrowOrderProjection item in list)
                {
                    if (item.BorrowStatus == StatusMain.Pending)
                    {
                        try
                        {
                            SL_AutoBorrowOrder order = autoBorrowList.Where( x => x.SLAutoBorrowOrder == item.SLAutoBorrowOrder ).First();

                            order.BorrowOrderStatus = StatusMain.Cancelled;
                           
                            DataSmartRoute.UpdateAutoBorrowOrder(order);
                        }
                        catch (Exception e)
                        {
                            return ThrowJsonError(e);
                        }
                    }
                }
            }

            return Extended.JsonMax( list, JsonRequestBehavior.AllowGet );
        }

        public JsonResult Mirror(IEnumerable<SL_AutoBorrowOrderProjection> list)
        {
            if (list == null) return Json(list);

            foreach (var item in list)
            {
                try
                {
                    var orderTemp = new SL_AutoBorrowOrder
                    {
                        ListName = String.IsNullOrWhiteSpace(item.ListName) ? "" : item.ListName,                        
                        AddToLoanetIndicator = String.IsNullOrWhiteSpace(item.AddToLoanetId) ? "" : item.AddToLoanetId,
                        BatchCode = String.IsNullOrWhiteSpace(item.BatchCode) ? "" : item.BatchCode,
                        BorrowOrderRequest = "",
                        BorrowOrderResponse = "",
                        BorrowOrderStatus = StatusMain.Pending,
                        BorrowOrderStatusFlag = SL_AutoBorrowOrderStatusFlag.Empty,
                        AutoBorrowOrderSystem = item.AutoBorrowOrderSystem,
                        CollateralFlag = item.CollateralFlag,
                        Comments = String.IsNullOrWhiteSpace(item.Comment) ? "" : item.Comment,
                        ContraEntity = "",
                        DateTimeId = item.DateTimeId,
                        DividendRate = Convert.ToDouble(item.DividendRate),
                        EffectiveDate = DateTime.Today,
                        EntityId = item.EntityId.ToString(CultureInfo.InvariantCulture),
                        IncomeTracked = item.IncomeTracked,
                        IssueId = item.IssueId,
                        Mark = Convert.ToDouble(item.Mark),
                        MarkParameterId = String.IsNullOrWhiteSpace(item.MarkParameterId) ? "" : item.MarkParameterId,
                        MaxPrice = Convert.ToDouble(item.MaxPrice),
                        MinPartialQuantity = item.MinQuantity,
                        MinRebateRate = Convert.ToDouble(item.MinRebateRate)
                    };

                    orderTemp.MinRebateRateId = orderTemp.MinRebateRate < 0 ? "N" : "S";

                    orderTemp.ProfitId = String.IsNullOrWhiteSpace(item.ProfitId) ? "" : item.ProfitId;
                    orderTemp.Quantity = item.Quantity;
                    orderTemp.SubmissionType = String.IsNullOrWhiteSpace(item.SubmissionType) ? "" : item.SubmissionType;
                    orderTemp.TimeOut = String.IsNullOrWhiteSpace(item.TimeOut) ? "" : item.TimeOut;

                    DataAutoBorrow.AddBorrowOrder(orderTemp);
                }
                catch (Exception e)
                {
                    return ThrowJsonError(e);
                }
            }

            return Extended.JsonMax( list, JsonRequestBehavior.AllowGet );
        }

        public JsonResult ProcessAutoBorrowBulkOptions(IEnumerable<SL_AutoBorrowOrderProjection> list,
                                string contraEntityId,
                                string smartRoute,
                                string listName,
                                decimal? rebateRate,
                                string rebateRateId,
                                decimal? mark,
                                string profitId,
                                string batchCode,
                                string timeOut)
        {
            if (list == null) return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
            
            var smartRouteList = new SL_SmartRouteList();
            var smartRoutes = new List<SL_SmartRoute>();

            var slAutoBorrowOrderProjections = list as SL_AutoBorrowOrderProjection[] ?? list.ToArray();
            if (!String.IsNullOrWhiteSpace(smartRoute))
            {
                smartRouteList = DataSmartRoute.LoadSmartRouteLists(slAutoBorrowOrderProjections.Select(x => x.EntityId).Distinct().Single().ToString(CultureInfo.InvariantCulture)).Where(x => x.Name.ToUpper().Equals(smartRoute.ToUpper())).Take(1).Single();
                smartRoutes = DataSmartRoute.LoadSmartRoutes(smartRouteList.EntityId, (int)smartRouteList.SLSmartRouteList).OrderBy(x => x.ExecuteOrder).ToList();
            }

            foreach (var item in slAutoBorrowOrderProjections)
            {
                try
                {
                    if (item.BorrowStatus != StatusMain.Pending) continue;
                    item.ContraEntityId = String.IsNullOrWhiteSpace(contraEntityId) ? "" : contraEntityId;
                    item.MinRebateRate = ( rebateRate == null ) ? item.MinRebateRate : ( rebateRate < -99.0m ) ? -99.0m : Convert.ToDecimal( rebateRate );
                    item.Mark = (mark == null) ? item.Mark : (decimal)mark;
                    item.TimeOut = String.IsNullOrWhiteSpace(timeOut) ? item.TimeOut : timeOut;
                    item.BatchCode = String.IsNullOrWhiteSpace(batchCode) ? item.BatchCode : batchCode;
                    item.ListName = String.IsNullOrWhiteSpace(listName) ? item.ListName: listName;
                    item.ProfitId = String.IsNullOrWhiteSpace(profitId) ? item.ProfitId : profitId;

                    if (item.MinRebateRate < 0)
                    {
                        item.MinRebateRateId = "N";
                    }
                    else
                    {
                        item.MinRebateRateId = rebateRateId;
                    }

                    if (!String.IsNullOrWhiteSpace(smartRoute))
                    {
                        item.SmartRouteId = int.Parse(smartRouteList.SLSmartRouteList.ToString(CultureInfo.InvariantCulture));
                        item.SmartRouteName = smartRouteList.Name;

                        if (smartRoutes.Count > 0)
                        {
                            item.AutoBorrowOrderSystem = smartRoutes.Take(1).Single().ExecutionSystemType;
                            item.ContraEntityId = smartRoutes.Take(1).Single().ContraEntity;
                            item.MinRebateRate = Convert.ToDecimal(smartRoutes.Take(1).Single().MinRebateRate);
                            item.Mark = Convert.ToDecimal(smartRoutes.Take(1).Single().Mark);
                            item.TimeOut = smartRoutes.Take(1).Single().TimeOut;
                        }
                        else
                        {
                            item.SmartRouteId = -1;
                            item.SmartRouteName = "";
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(contraEntityId))
                    {
                        item.SmartRouteId = -1;
                        item.SmartRouteName = "";
                        item.ContraEntityId = contraEntityId;
                    }
                }
                catch (Exception e)
                {
                    return ThrowJsonError(e);
                }
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveBulkAutoBorrowsEditor( IEnumerable<SL_AutoBorrowOrderProjection> list )
        {
            var itemList = new List<SL_AutoBorrowOrderProjection>();
            var updateList = new List<SL_AutoBorrowOrder>();

            var slAutoBorrowOrderProjections = list as SL_AutoBorrowOrderProjection[] ?? list.ToArray();
            if ( !slAutoBorrowOrderProjections.Any() ) return Json( list );

            foreach ( var item in slAutoBorrowOrderProjections )
            {
                var newItem = DataAutoBorrow.LoadBorrowOrder( item.SLAutoBorrowOrder );

                newItem.AddToLoanetIndicator = String.IsNullOrWhiteSpace( item.AddToLoanetId ) ? "" : item.AddToLoanetId;
                newItem.AutoBorrowOrderSystem = item.AutoBorrowOrderSystem;
                newItem.BatchCode = String.IsNullOrWhiteSpace( item.BatchCode ) ? "" : item.BatchCode;
                newItem.BorrowOrderStatusFlag = item.BorrowStatusFlag;
                newItem.CollateralFlag = item.CollateralFlag;
                newItem.ContraEntity = item.ContraEntityId;
                newItem.DividendRate = Convert.ToDouble( item.DividendRate );
                newItem.EntityId = item.EntityId.ToString( CultureInfo.InvariantCulture );
                newItem.IncomeTracked = item.IncomeTracked;
                newItem.IssueId = item.IssueId;
                newItem.ListName = String.IsNullOrWhiteSpace( item.ListName ) ? "" : item.ListName;
                newItem.Mark = Convert.ToDouble( item.Mark );
                newItem.MarkParameterId = String.IsNullOrWhiteSpace( item.MarkParameterId ) ? "" : item.MarkParameterId;
                newItem.MaxPrice = Convert.ToDouble( item.MaxPrice );
                newItem.MinPartialQuantity = item.MinQuantity;
                newItem.MinRebateRate = ( item.MinRebateRate < -99 ) ? -99.0 : Convert.ToDouble( item.MinRebateRate );
                newItem.MinRebateRateId = String.IsNullOrWhiteSpace( item.MinRebateRateId ) ? "" : item.MinRebateRateId;
                newItem.ProfitId = String.IsNullOrWhiteSpace( item.ProfitId ) ? "" : item.ProfitId;
                newItem.Quantity = item.Quantity;
                newItem.SLAutoBorrowOrder = item.SLAutoBorrowOrder;
                newItem.SmartRouteList = item.SmartRouteId;
                newItem.TimeOut = item.TimeOut;
                newItem.DateTimeId = DateTime.Now;
                
                updateList.Add( newItem );
            }

            var index = 0;

            while(true)
            {
                var chunk = updateList.Skip( index * 10 ).Take( 10 ).ToList();

                if ( chunk.Count == 0 ) break;

                DataAutoBorrow.UpdateBulkBorrowOrder( chunk );

                index++;
            }


            return Extended.JsonMax( list, JsonRequestBehavior.AllowGet );
        }

        /*** AutoBorrow Helper ***/
        public JsonResult ProcessHelperLcorOptions(IEnumerable<AutoBorrowOrderSuggestionModel> list,
                                string smartRoute,
                                string listName,
                                string contraEntityId,
                                double? rebateRate,
                                decimal? mark,
                                SL_ExecutionSystemType executingSystem)
        {
            if (list == null) return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);

            var smartRouteList = new SL_SmartRouteList();
            var smartRoutes = new List<SL_SmartRoute>();

            if (!String.IsNullOrWhiteSpace(smartRoute))
            {
                smartRouteList = DataSmartRoute.LoadSmartRouteLists(list.Select(x => x.EntityId).Distinct().Single()).Where(x => x.Name.ToUpper().Equals(smartRoute.ToUpper())).Take(1).Single();
                smartRoutes = DataSmartRoute.LoadSmartRoutes(smartRouteList.EntityId, (int)smartRouteList.SLSmartRouteList).OrderBy(x => x.ExecuteOrder).ToList();                   
            }

            foreach (var item in list.Where(item => item.Enabled))
            {
                try
                {
                    item.MemoInfo = "";
                    item.AutoBorrowOrderSystem = executingSystem;
                    item.ContraEntityId = String.IsNullOrWhiteSpace(contraEntityId) ? "" : contraEntityId;
                    item.RebateRate = ( rebateRate == null ) ? item.RebateRate : ( rebateRate < -99 ) ? -99.0 : Convert.ToDouble( rebateRate );
                    item.Mark = (mark == null) ? item.Mark : Convert.ToDecimal(mark);
                    item.ListName = String.IsNullOrWhiteSpace(listName) ? item.ListName : listName;

                    if (!String.IsNullOrWhiteSpace(smartRoute))
                    {
                        item.SmartRouteName = smartRouteList.Name;
                        item.SmartRoute = int.Parse(smartRouteList.SLSmartRouteList.ToString(CultureInfo.InvariantCulture));

                        if (smartRoutes.Count > 0)
                        {
                            item.AutoBorrowOrderSystem = smartRoutes.Take(1).Single().ExecutionSystemType;
                            item.ContraEntityId = smartRoutes.Take(1).Single().ContraEntity;
                            item.RebateRate = smartRoutes.Take(1).Single().MinRebateRate;
                            item.Mark = Convert.ToDecimal(smartRoutes.Take(1).Single().Mark);                                    
                        }
                        else
                        {
                            item.MemoInfo = "Smart route doesnt contain any items.";
                            item.SmartRoute = null;
                            item.SmartRouteName = "";
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(contraEntityId))
                    {
                        item.SmartRoute = null;
                        item.SmartRouteName = "";
                        item.ContraEntityId = contraEntityId;
                    }

                    if (item.SubmissionType != StatusDetail.Rejected)
                    {
                        item.SubmissionType = StatusDetail.Pending;
                    }
                }
                catch (Exception e)
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                    DataError.LogError("", "ProcessLcorOptions", e.Message);
                }
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessHelperSelectAll(IEnumerable<AutoBorrowOrderSuggestionModel> list)
        {
            foreach (var item in list)
            {
                item.Enabled = true;
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ProcessRemoveFakeSecurities(IEnumerable<AutoBorrowOrderSuggestionModel> list)
        {
            return Extended.JsonMax( list.Where( x => Char.IsDigit(x.SecurityNumber.ToUpper(), 0)).ToList(), JsonRequestBehavior.AllowGet );
        }

        public JsonResult ProcessHelperSaveBorrowOrder(IEnumerable<AutoBorrowOrderSuggestionModel> list)
        {
            var defaultAddToLoanetIndicator = DataSystemValues.LoadSystemValue("AutoBorrowAddToLoanet", "Y");
            var defaultBatchCode = DataSystemValues.LoadSystemValue("AutoBorrowBatchCode", "");
            var defaultDividendRate = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowDivRate", "100"));
            var defaultIncomeTracked = Convert.ToBoolean(DataSystemValues.LoadSystemValue("AutoBorrowIncomeTracked", "False"));
            var defaultMarkId = DataSystemValues.LoadSystemValue("AutoBorrowMarkId", "%");
            var matchQuantity = bool.Parse(DataSystemValues.LoadSystemValue("AutoBorrowMatchMin", "false"));
            var defaultProfitId = DataSystemValues.LoadSystemValue("AutoBorrowProfitId", "");
            var defaultTimeOut = DataSystemValues.LoadSystemValue("AutoBorrowTimeOut", "10");
            var defaultMinPartialQuantity = Convert.ToDecimal(DataSystemValues.LoadSystemValue("AutoBorrowMinPartialQuantity", "100"));
            var defaultMinRebateTableId = DataSystemValues.LoadSystemValue("AutoBorrowRoutingTableRateId", "");

            var autoBorrowOrderSuggestionModels = list as AutoBorrowOrderSuggestionModel[] ?? list.ToArray();

            var entityId = autoBorrowOrderSuggestionModels.Select(x => x.EntityId).Distinct().Take(1).Single();

            var smartRouteList = DataSmartRoute.LoadSmartRouteLists(entityId);

            var smartRoutes = new List<SL_SmartRoute>();

            foreach (var item in smartRouteList)
            {
                smartRoutes.AddRange(DataSmartRoute.LoadSmartRoutes(item.EntityId, Convert.ToInt32(item.SLSmartRouteList)));
            }

            var newItems = new List<SL_AutoBorrowOrder>();

            foreach (var item in autoBorrowOrderSuggestionModels.Where(item => item.Enabled))
            {
                try
                {
                    var order = new SL_AutoBorrowOrder
                    {
                        EffectiveDate = DateTime.Today,
                        AddToLoanetIndicator = defaultAddToLoanetIndicator,
                        BatchCode = defaultBatchCode,
                        BorrowOrderRequest = "",
                        BorrowOrderResponse = "",
                        AutoBorrowOrderSystem = item.AutoBorrowOrderSystem,
                        BorrowOrderStatus = StatusMain.Pending,
                        BorrowOrderStatusFlag = SL_AutoBorrowOrderStatusFlag.Empty,
                        CollateralFlag = SL_CollateralFlag.C,
                        Comments = "",
                        ContraEntity = String.IsNullOrWhiteSpace(item.ContraEntityId) ? "" : item.ContraEntityId,
                        DividendRate = defaultDividendRate,
                        EntityId = item.EntityId,
                        IncomeTracked = defaultIncomeTracked,
                        IssueId = item.IssueId,
                        Mark = Convert.ToDouble(item.Mark),
                        MarkParameterId = defaultMarkId,
                        MaxPrice = Convert.ToDouble(item.MaxPrice),
                        Quantity = item.Quantity,
                        ListName = String.IsNullOrWhiteSpace(item.ListName) ? "" : item.ListName,
                        SmartRouteList = (item.SmartRoute == null) ? -1 : (int) item.SmartRoute
                    };

                    order.MinPartialQuantity = matchQuantity ? order.Quantity : defaultMinPartialQuantity;


                    AutoBorrowOrderSuggestionModel item1;
                    if (!String.IsNullOrWhiteSpace(item.SmartRouteName))
                    {

                        try
                        {
                            item1 = item;
                            var item2 = item1;
                            order.MinRebateRateId = item.RebateRate >= 0 ? smartRoutes.Where(x => item2.SmartRoute != null && (x.EntityId.Equals(entityId) && x.SmartRouteList == (int)item2.SmartRoute)).Select(x => x.MinRebateRateId).Take(1).Single() : "N";
                        }
                        catch
                        {
                            order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                        }
                    }
                    else
                    {
                        order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                    }

                    order.MinRebateRate = Convert.ToDouble(item.RebateRate);
                    order.ProfitId = defaultProfitId;
                    order.SubmissionType = "";

                    item1 = item;
                    order.TimeOut = order.SmartRouteList != -1 ? smartRoutes.Where(x => item1.SmartRoute != null && (x.EntityId.Equals(entityId) && x.SmartRouteList == (int)item1.SmartRoute)).Select(x => x.TimeOut).Take(1).Single() : defaultTimeOut;

                    newItems.Add(order);

                    item.SubmissionType = StatusDetail.Approved;
                }
                catch (Exception e)
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                }
            }

            DataAutoBorrow.AddBulkBorrowOrder(newItems.AsEnumerable());

            return Json(autoBorrowOrderSuggestionModels.Where(x => x.SubmissionType != StatusDetail.Approved));
        }

        public JsonResult ProcessHelperConfirmBorrowOrder(IEnumerable<AutoBorrowOrderSuggestionModel> list)
        {
            var defaultAddToLoanetIndicator = DataSystemValues.LoadSystemValue("AutoBorrowAddToLoanet", "Y");
            var defaultBatchCode = DataSystemValues.LoadSystemValue("AutoBorrowBatchCode", "");
            var defaultDividendRate = Convert.ToDouble(DataSystemValues.LoadSystemValue("AutoBorrowDivRate", "100"));
            var defaultIncomeTracked = Convert.ToBoolean(DataSystemValues.LoadSystemValue("AutoBorrowIncomeTracked", "False"));
            var defaultMarkId = DataSystemValues.LoadSystemValue("AutoBorrowMarkId", "%");
            var matchQuantity = bool.Parse(DataSystemValues.LoadSystemValue("AutoBorrowMatchMin", "false"));
            var defaultProfitId = DataSystemValues.LoadSystemValue("AutoBorrowProfitId", "");
            var defaultTimeOut = DataSystemValues.LoadSystemValue("AutoBorrowTimeOut", "10");
            var defaultMinPartialQuantity = Convert.ToDecimal(DataSystemValues.LoadSystemValue("AutoBorrowMinPartialQuantity", "100"));
            var defaultMinRebateTableId = DataSystemValues.LoadSystemValue("AutoBorrowRoutingTableRateId", "");

            var autoBorrowOrderSuggestionModels = list as AutoBorrowOrderSuggestionModel[] ?? list.ToArray();

            var entityId = autoBorrowOrderSuggestionModels.Select(x => x.EntityId).Distinct().Take(1).Single();

            var smartRouteList = DataSmartRoute.LoadSmartRouteLists(entityId);

            var smartRoutes = new List<SL_SmartRoute>();

            foreach (var item in smartRouteList)
            {
                smartRoutes.AddRange(DataSmartRoute.LoadSmartRoutes(item.EntityId, Convert.ToInt32(item.SLSmartRouteList)));
            }

            var newItems = new List<SL_AutoBorrowOrder>();

            foreach (var item in autoBorrowOrderSuggestionModels.Where(item => item.Enabled))
            {
                try
                {
                    var order = new SL_AutoBorrowOrder
                    {
                        EffectiveDate = DateTime.Today,
                        AddToLoanetIndicator = defaultAddToLoanetIndicator,
                        BatchCode = defaultBatchCode,
                        BorrowOrderRequest = "",
                        BorrowOrderResponse = "",
                        AutoBorrowOrderSystem = item.AutoBorrowOrderSystem,
                        BorrowOrderStatus = StatusMain.Pending,
                        BorrowOrderStatusFlag = SL_AutoBorrowOrderStatusFlag.Empty,
                        CollateralFlag = SL_CollateralFlag.C,
                        Comments = "",
                        ContraEntity = String.IsNullOrWhiteSpace(item.ContraEntityId) ? "" : item.ContraEntityId,
                        DividendRate = defaultDividendRate,
                        EntityId = item.EntityId,
                        IncomeTracked = defaultIncomeTracked,
                        IssueId = item.IssueId,
                        Mark = Convert.ToDouble(item.Mark),
                        MarkParameterId = defaultMarkId,
                        MaxPrice = Convert.ToDouble(item.MaxPrice),
                        Quantity = item.Quantity,
                        ListName = String.IsNullOrWhiteSpace(item.ListName) ? "" : item.ListName,
                        SmartRouteList = (item.SmartRoute == null) ? -1 : (int)item.SmartRoute
                    };

                    order.MinPartialQuantity = matchQuantity ? order.Quantity : defaultMinPartialQuantity;


                    AutoBorrowOrderSuggestionModel item1;
                    if (!String.IsNullOrWhiteSpace(item.SmartRouteName))
                    {

                        try
                        {
                            item1 = item;
                            var item2 = item1;
                            order.MinRebateRateId = item.RebateRate >= 0 ? smartRoutes.Where(x => item2.SmartRoute != null && (x.EntityId.Equals(entityId) && x.SmartRouteList == (int)item2.SmartRoute)).Select(x => x.MinRebateRateId).Take(1).Single() : "N";
                        }
                        catch
                        {
                            order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                        }
                    }
                    else
                    {
                        order.MinRebateRateId = item.RebateRate >= 0 ? "S" : "N";
                    }

                    order.MinRebateRate = Convert.ToDouble(item.RebateRate);
                    order.ProfitId = defaultProfitId;
                    order.SubmissionType = "";

                    item1 = item;
                    order.TimeOut = order.SmartRouteList != -1 ? smartRoutes.Where(x => item1.SmartRoute != null && (x.EntityId.Equals(entityId) && x.SmartRouteList == (int)item1.SmartRoute)).Select(x => x.TimeOut).Take(1).Single() : defaultTimeOut;

                    newItems.Add(order);

                    item.SubmissionType = StatusDetail.Approved;
                }
                catch (Exception e)
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                }
            }

            var listNames = newItems.Select(x => x.ListName).Distinct();

            DataAutoBorrow.AddBulkBorrowOrder(newItems.AsEnumerable());

            foreach (var itemListName in listNames)
            {
                Send_AutoBorrowList(null, entityId, itemListName);
            }

            return Json(autoBorrowOrderSuggestionModels.Where(x => x.SubmissionType != StatusDetail.Approved));
        }

        public JsonResult LoadSingleBorrowOrder(SL_BoxCalculationExtendedProjection item, decimal modelId)
        {
            var order = new AutoBorrowOrderSuggestionModel();

            if (item.ExcessPositionSettled == 0) return Json(order, JsonRequestBehavior.AllowGet);
            order.ContraEntityId = "";
            order.Enabled = true;
            order.EntityId = item.EntityId;
            if (item.IssueId != null)
            {
                order.IssueId = (int)item.IssueId;
                order.Mark = 1.02m;
                order.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, (int)item.IssueId).CurrentCashPrice), null);
            }
            order.ModelId = modelId;
            order.Quantity = Math.Abs(item.SuggestionBorrowSettled);
            order.RebateRate = 0;
            order.SecurityNumber = item.SecurityNumber;
            order.Ticker = item.Ticker;
            order.MemoInfo = "";
            order.SubmissionType = StatusDetail.Pending;

            order = AutoBorrowOrderValidationService.SetDefaults(order);

            return Json(order, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadBulkBorrowOrder(IEnumerable<SL_BoxCalculationExtendedProjection> items)
        {
            var list = new List<AutoBorrowOrderSuggestionModel>();
            long modelCount = 0;

            if (items == null) return Json(list, JsonRequestBehavior.AllowGet);

            foreach (var item in items.Where(item => item.ExcessPositionSettled != 0))
            {
                if (item.IssueId != null)
                {
                    var order = new AutoBorrowOrderSuggestionModel
                    {
                        ContraEntityId = "",
                        Enabled = true,
                        EntityId = item.EntityId,
                        IssueId = (int) item.IssueId,
                        AutoBorrowOrderSystem = SL_ExecutionSystemType.LOANET,
                        Mark = 1.02m,
                        MaxPrice =
                            SLTradeCalculator.CalculatePrice(item.EntityId,
                                Convert.ToDecimal(DataIssue.LoadIssuePrice(item.EntityId, (int) item.IssueId).CurrentCashPrice),
                                null),
                        ModelId = modelCount,
                        Quantity = Math.Abs(item.SuggestionBorrowSettled),
                        RebateRate = 0,
                        SecurityNumber = item.SecurityNumber,
                        Ticker = item.Ticker,
                        MemoInfo = "",
                        SubmissionType = StatusDetail.Pending
                    };

                    order = AutoBorrowOrderValidationService.SetDefaults(order);

                    list.Add(order);
                }
                modelCount++;
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateBulkAutoBorrow([DataSourceRequest] DataSourceRequest request, SL_AutoBorrowOrderProjection item)
        {
            if (!item.SmartRouteName.Equals(""))
            {
                var smartRouteList = DataSmartRoute.LoadSmartRouteLists(item.EntityId.ToString(CultureInfo.InvariantCulture)).Where(x => x.Name.ToUpper().Equals(item.SmartRouteName.ToUpper())).Take(1).Single();
                var smartRoutes = DataSmartRoute.LoadSmartRoutes(smartRouteList.EntityId, (int)smartRouteList.SLSmartRouteList).OrderBy(x => x.ExecuteOrder).ToList();

                item.SmartRouteName = smartRouteList.Name;
                item.SmartRouteId = int.Parse(smartRouteList.SLSmartRouteList.ToString());


                item.ContraEntityId = smartRoutes.Take(1).Single().ContraEntity;
                item.MinRebateRate = Convert.ToDecimal(smartRoutes.Take(1).Single().MinRebateRate);
                item.MinRebateRateId = smartRoutes.Take(1).Single().MinRebateRateId;
                item.Mark = Convert.ToDecimal(smartRoutes.Take(1).Single().Mark);
            }
            else
            {
                item.SmartRouteId = -1;
                item.SmartRouteName = "";
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }

        /*** AutoBorrow partials ***/
        public PartialViewResult LoadAutoBorrowBoxProjectionPartial(IEnumerable<AutoBorrowOrderSuggestionModel> items)
        {
            var list = new List<AutoBorrowOrderSuggestionModel>();

            var useIssueMarkup = bool.Parse(DataSystemValues.LoadSystemValue("UseIssueMarginBoxMgmt", "false"));

            long modelCount = 0;
            var listName = DateTime.Now.ToString("HHmmss") + "-" + SessionService.SecurityContext.UserName;

            if (items == null)
                return list.Count == 0
                    ? PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowErrorHelper.cshtml")
                    : PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowHelper.cshtml", list);

            foreach (var item in items.Where(item => item.Quantity != 0))
            {
                item.ContraEntityId = "";
                item.Enabled = true;
                item.EntityId = item.EntityId;
                item.IssueId = item.IssueId;
                item.Mark = 1.02m;
                item.ModelId = modelCount;
                item.SmartRoute = null;
                item.SmartRouteName = "";
                item.AutoBorrowOrderSystem = SL_ExecutionSystemType.LOANET;
                item.ListName = listName;

                if (!useIssueMarkup)
                {
                    item.MaxPrice = SLTradeCalculator.CalculatePrice(item.EntityId, item.MaxPrice, null);
                }

                item.RebateRate = 0;
                item.SecurityNumber = item.SecurityNumber;
                item.Ticker = item.Ticker;
                item.MemoInfo = "";                       
                item.SubmissionType = StatusDetail.Pending;

                var tempItem = AutoBorrowOrderValidationService.SetDefaults(item);
                list.Add(tempItem);
                modelCount++;
            }

            return list.Count == 0 ? PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowErrorHelper.cshtml") : PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowHelper.cshtml", list);
        }

        public PartialViewResult LoadAutoBorrowDefaultOptionsPartial()
        {
            return PartialView("~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowDefaultOptions.cshtml");
        }

        public JsonResult ProcessDefaultsUpdateBorrowOrder(
            string batchCode,
            decimal divRate,
            bool incomeTracked,
            bool matchMin,
            string profitId,
            decimal mark,
            string markId,
            decimal minQuantity,
            string timeOut,
            string addToLoanet,
            string routeTableRateId)
        {
            try
            {
                DataSystemValues.UpdateValueByName("AutoBorrowBatchCode", batchCode);

                DataSystemValues.UpdateValueByName("AutoBorrowDivRate", divRate.ToString(CultureInfo.InvariantCulture));

                DataSystemValues.UpdateValueByName("AutoBorrowIncomeTracked", incomeTracked.ToString());

                DataSystemValues.UpdateValueByName("AutoBorrowProfitId", profitId);

                DataSystemValues.UpdateValueByName("AutoBorrowMark", mark.ToString(CultureInfo.InvariantCulture));

                DataSystemValues.UpdateValueByName("AutoBorrowMarkId", markId);

                DataSystemValues.UpdateValueByName("AutoBorrowMinPartialQuantity", minQuantity.ToString(CultureInfo.InvariantCulture));

                DataSystemValues.UpdateValueByName("AutoBorrowTimeOut", timeOut);

                DataSystemValues.UpdateValueByName("AutoBorrowAddToLoanet", addToLoanet);

                DataSystemValues.UpdateValueByName("AutoBorrowMatchMin", matchMin.ToString());

                DataSystemValues.UpdateValueByName("AutoBorrowRoutingTableRateId", routeTableRateId);
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadBulkUpdatePartial( List<SL_AutoBorrowOrderProjection> items )
        {
            var _items = items.Where( x => x.BorrowStatus == StatusMain.Pending ).ToList();

            if ( _items.Count == 0 )
            {
                return PartialView( "~/Areas/DomesticTrading/Views/AutoAction/_AutoBorrowErrorHelper.cshtml" );
            }

            return PartialView( "~/Areas/DomesticTrading/Views/AutoBorrow/_UpdateAutoBorrows.cshtml", _items );
        }
    
    
        /*** AutoBorrow ScratchPad ***/

        public JsonResult ProcessScratchPadLcorOptions(IEnumerable<AutoBorrowOrderSuggestionModel> list,
                               string smartRoute,
                               string listName,
                               string contraEntityId,
                               double? rebateRate,
                               decimal? mark,
                                SL_ExecutionSystemType executingSystem)
        {
            if (list == null) return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
            var smartRouteList = new SL_SmartRouteList();
            var smartRoutes = new List<SL_SmartRoute>();

            var autoBorrowOrderSuggestionModels = list as AutoBorrowOrderSuggestionModel[] ?? list.ToArray();
            if (!String.IsNullOrWhiteSpace(smartRoute))
            {
                smartRouteList = DataSmartRoute.LoadSmartRouteLists(autoBorrowOrderSuggestionModels.Select(x => x.EntityId).Distinct().Single()).Where(x => x.Name.ToUpper().Equals(smartRoute.ToUpper())).Take(1).Single();
                smartRoutes = DataSmartRoute.LoadSmartRoutes(smartRouteList.EntityId, (int)smartRouteList.SLSmartRouteList).OrderBy(x => x.ExecuteOrder).ToList();
            }

            foreach (var item in autoBorrowOrderSuggestionModels.Where(item => (item.Enabled) && (item.SubmissionType != StatusDetail.Rejected)))
            {
                try
                {
                    item.MemoInfo = "";
                    item.ContraEntityId = String.IsNullOrWhiteSpace(contraEntityId) ? "" : contraEntityId;
                    item.RebateRate = ( rebateRate == null ) ? item.RebateRate : ( rebateRate < -99 ) ? -99.0 : Convert.ToDouble( rebateRate );
                    item.Mark = (mark == null) ? item.Mark : Convert.ToDecimal(mark);
                    item.ListName = String.IsNullOrWhiteSpace(listName) ? item.ListName : listName;
                    item.AutoBorrowOrderSystem = executingSystem;

                    if (!String.IsNullOrWhiteSpace(smartRoute))
                    {
                        item.SmartRouteName = smartRouteList.Name;
                        item.SmartRoute = int.Parse(smartRouteList.SLSmartRouteList.ToString());

                        if (smartRoutes.Count > 0)
                        {
                            item.AutoBorrowOrderSystem = smartRoutes.Take(1).Single().ExecutionSystemType;
                            item.ContraEntityId = smartRoutes.Take(1).Single().ContraEntity;
                            item.RebateRate = smartRoutes.Take(1).Single().MinRebateRate;
                            item.Mark = Convert.ToDecimal(smartRoutes.Take(1).Single().Mark);
                        }
                        else
                        {
                            item.MemoInfo = "Smart route doesnt contain any items.";
                            item.SmartRoute = null;
                            item.SmartRouteName = "";
                        }
                    }
                    else if (!String.IsNullOrWhiteSpace(contraEntityId))
                    {
                        item.SmartRoute = null;
                        item.SmartRouteName = "";
                        item.ContraEntityId = contraEntityId;                       
                    }

                    if (item.SubmissionType != StatusDetail.Rejected)
                    {
                        item.SubmissionType = StatusDetail.Pending;
                    }
                }
                catch (Exception e)
                {
                    item.MemoInfo = e.Message;
                    item.SubmissionType = StatusDetail.Rejected;
                    DataError.LogError("", "ProcessLcorOptions", e.Message);
                }
            }

            return Extended.JsonMax(list, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadScratchpad()
        {
            return PartialView("~/Areas/DomesticTrading/Views/AutoBorrow/_AutoBorrowScratchpad.cshtml", new List<AutoBorrowOrderSuggestionModel>());
        }

        public JsonResult LoadList(string entityId,
                        string list,
                        string listName,
                        string smartRoute,
                        string contraEntityId,
                        double? rebateRate,
                        double? mark,
                        SL_ExecutionSystemType executingSystem)
        {

            var listFormat = DataSystemValues.LoadSystemValue("AutoBorrowListParseFormat", "TICKER;QUANTITY;");

            var parsedList = ListParsingService.GenerateList(entityId, list, listFormat);
            List<AutoBorrowOrderSuggestionModel> autoborrowList = ListParsingService.GenerateAutoBorrowSuggestions(listName, smartRoute, contraEntityId, rebateRate, mark, parsedList) ;

            return Json(autoborrowList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAutoBorrowScratchPad([DataSourceRequest] DataSourceRequest request, AutoBorrowOrderSuggestionModel item)
        {
            if (String.IsNullOrWhiteSpace(item.SmartRouteName))
            {
                return Json(new[] { item }.ToDataSourceResult(request, ModelState));
            }
            else
            {
                var smartRouteList = DataSmartRoute.LoadSmartRouteLists(item.EntityId).Single(x => x.Name.Equals(item.SmartRouteName));
                var smartRoutes = DataSmartRoute.LoadSmartRoutes(item.EntityId, (int)smartRouteList.SLSmartRouteList).OrderBy(x => x.ExecuteOrder).ToList();

                item.SmartRoute = Convert.ToInt32(smartRouteList.SLSmartRouteList);
                item.SmartRouteName = smartRouteList.Name;

                item.ContraEntityId = smartRoutes.Take(1).Single().ContraEntity;

                item.RebateRate = smartRoutes.Take(1).Single().MinRebateRate;
                item.Mark = Convert.ToDecimal(smartRoutes.Take(1).Single().Mark);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        public ActionResult ExportAutoBorrowToExcel(DateTime effectiveDate, string entityId, AutoActionReportTypeEnums reportType)
        {
            var reportDetails = new List<SL_AutoBorrowOrderProjection>();
            var autoBorrowList = DataAutoBorrow.LoadAutoBorrowByEntity( effectiveDate, entityId );

            switch (reportType)
            {
                case AutoActionReportTypeEnums.Filled:
                    reportDetails = autoBorrowList.Where( x => x.BorrowStatus == StatusMain.Settled ).ToList();
                    break;
                case AutoActionReportTypeEnums.NotFilled:
                    reportDetails = autoBorrowList.Where( x => x.BorrowStatus == StatusMain.Error ).ToList();
                    break;
                case AutoActionReportTypeEnums.Pending:
                    reportDetails = autoBorrowList.Where( x => x.BorrowStatus == StatusMain.Pending ).ToList();
                    break;
                case AutoActionReportTypeEnums.All:
                    reportDetails = autoBorrowList;
                    break;
            }
            
            

            var fName = string.Format( "autoBorrowReport-{0}.xlsx", DateTime.Now.ToString( "yyyy-MM-dd hh:mm:ss" ) );

            return File( ExcelHelper.ExportExcel( reportDetails, "autoBorrowReport", new List<string>() { "SmartRouteName", "ContraEntityId", "SecurityNumber", "Ticker", "Quantity", "MinRebateRate", "BorrowStatus", "BorrowStatusFlag" }, true ), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fName );
        }
    }
}
