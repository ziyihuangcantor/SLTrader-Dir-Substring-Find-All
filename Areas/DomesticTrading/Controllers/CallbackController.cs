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
using SLTrader.Models.ContractRelatedModels;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class CallbackController : BaseController
    {
        // GET: DomesticTrading/Callback
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_BorrowExcess([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var list = new List<SL_BorrowExcessProjection>();

            if (String.IsNullOrWhiteSpace(entityId))
            {
                return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet); ;
            }

            try
            {
                list = DataCallback.LoadBorrowExcess(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_BorrowCallbackByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            var contracts = DataContracts.LoadContractsByIssue(effectiveDate, effectiveDate, entityId, issueId.ToString());

            var list = new List<SL_Callback>();
            var _list = new List<BorrowCallbackWithContractModel>();

            /*try
            {
                list = DataCallback.LoadCallback( effectiveDate, entityId ).Where( x => x.IssueId == issueId ).ToList();

                _list = contracts.Select( x => new BorrowCallbackWithContractModel()
                    {
                        Contract = x,
                        CallBack = ( list.Where( q => q.ContraEntity == x.ContraEntity && q.ContractNumber == x.ContractNumber && q.TradeType == x.TradeType && q.IssueId == x.IssueId ).FirstOrDefault() )
                    } ).ToList();
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }*/

            return Extended.JsonMax(_list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_LoanCallbackByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            var list = new List<SL_Callback>();
            var contractList = DataContracts.LoadContracts(effectiveDate, effectiveDate, entityId).Where(x => x.TradeType == TradeType.StockLoan).ToList();

            try
            {
                list = DataCallback.LoadCallback(effectiveDate, entityId).Where(x => x.IssueId == issueId && x.TradeType == TradeType.StockLoan).ToList();

                foreach (var item in list)
                {
                    item.RebateRate = Convert.ToDouble(SLTradeCalculator.CalculateAvgWeightedRate(item.EntityId, item.IssueId, contractList));
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_LoanCallbackByContraEntity([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var list = new List<SL_ContractCallbackByContraEntityProjection>();

            try
            {
                list = DataCallback.LoadCallbackByContraEntity(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult BorrowAllocatedReturn(SL_CallBackExtendedProjection item)
        {
            try
            {
                SL_Callback callback = DataCallback.LoadCallbackByPK(item.SLCallback);

                DataExternalOperations.AddNewReturn(
                    callback.EntityId,
                    callback.ContraEntity,
                    callback.ContractNumber,
                    callback.TradeType,
                    callback.IssueId,
                    callback.ReturnQuantity,
                    callback.ReturnAmount,
                    SL_ActivitySubType.ReturnPartial,
                    "E",
                    "L",
                    SL_DeliveryCode.CCF);

                callback.ReturnStatusId = StatusMain.Settled;
                item.ReturnStatusId = StatusMain.Settled;
                DataCallback.UpdateCallBack(callback);

            }
            catch (Exception e)
            {
                item.ReturnStatusId = StatusMain.Error;
            }

            return Extended.JsonMax(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PendingActionAllocated(PendingActionModel item)
        {
            try
            {
                SL_Callback callBackItem = new SL_Callback();
                callBackItem.ContractNumber = item.TypeId;
                callBackItem.ContraEntity = item.ContraEntity;
                callBackItem.CurrentAmount = item.Amount;
                callBackItem.CurrentQuantity = item.Quantity;
                callBackItem.DeliveryDate = DateTime.Today;
                callBackItem.EffectiveDate = DateTime.Today;
                callBackItem.EntityId = item.EntityId;
                callBackItem.IssueId = item.IssueId;
                callBackItem.MadeIndicatorId = SL_MadeIndicator.PENDING;
                callBackItem.MemoInfo = "";
                callBackItem.RebateRate = (double)item.Rate;
                callBackItem.ReturnQuantity = item.PendQuantity;
                callBackItem.ReturnAmount = item.PendAmount;
                callBackItem.ReturnStatusId = StatusMain.Pending;
                callBackItem.TradeType = item.TradeType;
                

                DataCallback.AddCallBack(callBackItem);

                DataExternalOperations.AddNewBorrowCallback(callBackItem);

                item.SubmissionType = StatusDetail.Settled;

                item.PendQuantity = 0;
                item.PendAmount = 0;
            }
            catch (Exception e)
            {
                item.SubmissionType = StatusDetail.Rejected;
            }

            return Extended.JsonMax(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Destroy_BorrowAllocation([DataSourceRequest] DataSourceRequest request, PendingActionModel callBackItem)
        {
            return Extended.JsonMax(new[] { callBackItem }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
                            
       
        public ActionResult Update_PendingAction([DataSourceRequest] DataSourceRequest request, PendingActionModel item)
        {

            item.PendAmount = item.PendQuantity * item.Price;

            return Extended.JsonMax(item, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Update_BorrowCallBack( [DataSourceRequest] DataSourceRequest request, BorrowCallbackWithContractModel item )
        {
            var callback = DataCallback.LoadCallbackByPK( item.CallBack );

            var price = callback.ReturnAmount / callback.ReturnQuantity;

            callback.ReturnQuantity = item.CallBack.ReturnQuantity;
            callback.ReturnAmount = item.CallBack.ReturnQuantity * price;

            DataCallback.UpdateCallBack( callback );

            return Extended.JsonMax( callback, JsonRequestBehavior.AllowGet );
        }


        public ActionResult Load_BorrowAllocation([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<Models.ContractRelatedModels.PendingActionModel> pendingActionList = new List<Models.ContractRelatedModels.PendingActionModel>();

            var contractList = DataContracts.LoadContractsByIssue(effectiveDate, effectiveDate, entityId, issueId.ToString());

            int count = 0;

            foreach (var item in contractList.Where(x => x.TradeType == TradeType.StockBorrow))
            {
                Models.ContractRelatedModels.PendingActionModel pendingActionModel = new Models.ContractRelatedModels.PendingActionModel();

                if ((item.Quantity - item.QuantityCallback) > 0)
                {
                    pendingActionModel.Enabled = true;
                    pendingActionModel.EntityId = item.EntityId;
                    pendingActionModel.ContraEntity = item.ContraEntity;
                    pendingActionModel.IssueId = item.IssueId;
                    pendingActionModel.TypeId = item.ContractNumber;
                    pendingActionModel.MemoInfo = "";
                    pendingActionModel.PortionType = Enums.PortionType.None;
                    pendingActionModel.Price = item.Amount / item.Quantity;
                    pendingActionModel.Quantity = (item.Quantity - item.QuantityCallback);
                    pendingActionModel.Amount = (item.Quantity - item.QuantityCallback) * pendingActionModel.Price;
                    pendingActionModel.SecurityNumber = item.SecurityNumber;
                    pendingActionModel.Ticker = item.Ticker;
                    pendingActionModel.TradeType = item.TradeType;
                    pendingActionModel.RecallQuantity = item.QuantityOnRecallOpen;
                    pendingActionModel.RecallQuantity = item.QuantityOnRecallOpen * pendingActionModel.Price;
                    pendingActionModel.PendQuantity = 0;
                    pendingActionModel.PendAmount = 0;
                    pendingActionModel.Rate = item.RebateRate;
                    pendingActionModel.ModelId = count;

                    pendingActionList.Add(pendingActionModel);

                    count = count + 1;
                }
            }

            return Extended.JsonMax(pendingActionList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult CallbackByEntityAndQuantity(DateTime effectiveDate, string entityId, int issueId, decimal quantity)
        {
            var contractList = DataContracts.LoadContractsByIssue( effectiveDate, effectiveDate, entityId, issueId.ToString() ).Where(x => x.TradeType == TradeType.StockBorrow && x.Quantity > 0 && x.Amount > 0).OrderBy(x => x.RebateRate).ToList();
            var callBackList = DataCallback.LoadCallback( effectiveDate, entityId ).Where( x => x.IssueId == issueId ).ToList();

            var remainingQuantity = quantity  - (quantity % 100);

            foreach ( var item in contractList )
            {
                decimal quantityToCallback = 0;
                item.MktPrice = item.Amount / item.Quantity;
                item.Quantity = item.Quantity - ( callBackList.Where( x => x.ContractNumber == item.ContractNumber ).Sum( x => x.ReturnQuantity ) );

                if (( item.Quantity > 0 ) && (remainingQuantity > 0))                    
                {

                    if ( item.Quantity > remainingQuantity )
                    {
                        quantityToCallback = remainingQuantity;
                    }
                    else
                    {
                        quantityToCallback = item.Quantity;
                    }

                    SL_Callback callBack = new SL_Callback();
                    callBack.EffectiveDate = item.EffectiveDate;
                    callBack.EntityId = item.EntityId;
                    callBack.ContraEntity = item.ContraEntity;
                    callBack.ContractNumber = item.ContractNumber;
                    callBack.RebateRate = Convert.ToDouble( item.RebateRate );
                    callBack.ReturnQuantity = quantityToCallback;
                    callBack.ReturnAmount = quantityToCallback * item.MktPrice;
                    callBack.TradeType = TradeType.StockBorrow;
                    callBack.IssueId = item.IssueId;
                    callBack.MadeIndicatorId = SL_MadeIndicator.PENDING;
                    
                    callBack.MemoInfo = "";

                    DataCallback.AddCallBack( callBack );

                    DataExternalOperations.AddNewBorrowCallback( callBack );
                    remainingQuantity = remainingQuantity - callBack.ReturnQuantity;
                }

                if ( remainingQuantity == 0 ) break;
            }

            return Extended.JsonMax( new [] { true }, JsonRequestBehavior.AllowGet );
        }

        public JsonResult CancelRecallByEntityAndQuantity( DateTime effectiveDate, string entityId, int issueId, decimal excess )
        {
            var recallList = DataRecalls.LoadRecallsByIssue( effectiveDate, entityId, issueId).Where( x => x.TradeType == TradeType.StockLoan && x.QuantityRemaining > 0 && x.Status != SL_RecallStatus.CLOSED ).OrderBy( x => x.BuyInDate ).ToList();

            var remainingQuantity = excess - ( excess % 100 );

            foreach ( var item in recallList )
            {
             
                DataExternalOperations.CancelRecall(item);

                remainingQuantity = remainingQuantity - item.QuantityRemaining;

                if ( remainingQuantity <= 0 ) break;
            }

            return Extended.JsonMax( new[] { true }, JsonRequestBehavior.AllowGet );
        }

        public JsonResult ReturnCallBack( SL_Callback callBack )
        {
            var _callback = DataCallback.LoadCallbackByPK( callBack );

            
            return Extended.JsonMax( new[] { _callback }, JsonRequestBehavior.AllowGet );
        }

        public JsonResult LendCallback( SL_Callback callBack )
        {
            var _callback = DataCallback.LoadCallbackByPK( callBack);

            try
            {   
                DataCallback.UpdateCallBack( _callback );
            }
            catch
            {

            }

            return Extended.JsonMax( new[] { _callback }, JsonRequestBehavior.AllowGet );
        }

        public JsonResult ReleaseAllCallbacks( SL_BorrowExcessProjection dataItem )
        {
            var list = new List<SL_Callback>();
            var count = 0;

            try
            {
                list = DataCallback.LoadCallback( dataItem.EffectiveDate, dataItem.EntityId ).Where( x => x.IssueId == dataItem.IssueId ).ToList();

                foreach ( var _item in list )
                {                    
                }
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( count, JsonRequestBehavior.AllowGet );
        }


        public JsonResult ReleaseAllStockBorrowCallbacks(DateTime effectiveDate, List<string> entityIdList)
        {
            var list = new List<SL_Callback>();
            var count = 0;

            try
            {
                foreach (var entityid in entityIdList)
                {
                    var pendingCallBacks = DataCallback.LoadCallbackExtended(effectiveDate, entityid);

                    foreach (var item in pendingCallBacks.Where(x => x.TradeType == TradeType.StockBorrow && x.ReturnStatusId == StatusMain.Pending))
                    {
                        SL_Callback callback = DataCallback.LoadCallbackByPK(item.SLCallback);

                        DataExternalOperations.AddNewReturn(
                            callback.EntityId,
                            callback.ContraEntity,
                            callback.ContractNumber,
                            callback.TradeType,
                            callback.IssueId,
                            callback.ReturnQuantity,
                            callback.ReturnAmount,
                            SL_ActivitySubType.ReturnPartial,
                            "E",
                            "L",
                            SL_DeliveryCode.CCF);

                        callback.ReturnStatusId = StatusMain.Settled;

                        DataCallback.UpdateCallBack(callback);

                        count = count + 1;
                    }
                }                   
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(count, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ReleaseAllMemoSeg(DateTime effectiveDate, string entityId)
        {
            var memoSegList = DataDTCC.LoadDepoExtended(effectiveDate, entityId);
            var count = 0;

            foreach(var item in memoSegList)
            {
                if (item.SegregatedQuantity != 0)
                {
                    try
                    {
                        SL_MemoSeg memoSeg = new SL_MemoSeg();
                        memoSeg.EffectiveDate = effectiveDate;
                        memoSeg.EntityId = entityId;
                        memoSeg.IssueId = (int)item.IssueId;
                        memoSeg.MemoSegItemRequest = "";
                        memoSeg.MemoSegItemResponse = "";
                        memoSeg.MemoSegOperatorId = SL_MemoSegOperator.OVERLAY;
                        memoSeg.Quantity = 0;
                        memoSeg.StatusMain = StatusMain.Pending;


                        DataDTCC.AddMemoSeg(memoSeg);
                        count++;
                    }
                    catch(Exception e)
                    {
                        return ThrowJsonError(e);
                    }              
                }
            }

            return Extended.JsonMax(count, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SetupMemoSeg(SL_MemoSeg memoSeg)
        {
            try
            {
                DataDTCC.AddMemoSeg(memoSeg);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }
    }
}