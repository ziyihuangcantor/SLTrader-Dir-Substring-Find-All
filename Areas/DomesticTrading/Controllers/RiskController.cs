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
using SLTrader.Enums;
using SLTrader.Models;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class RiskController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_ExcessNetCollateralSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var modelList = new List<ExcessCollateralModel>();
            var excessSummary = new List<ExcessCollateralSummaryModel>();
            if ( entityId == "" ) return Json( modelList.ToDataSourceResult( request ) );

            try
            {
                var contractList = DataContracts.LoadContracts( effectiveDate, effectiveDate, entityId );

                foreach ( var item in contractList )
                {
                    item.Quantity = item.Quantity - item.QuantityOnRecallOpen;


                    decimal price;
                    try
                    {
                        price = ( item.AmountSettled / item.QuantitySettled );
                    }
                    catch
                    {
                        price = 0;
                    }

                    item.Amount = item.Amount - ( item.QuantityOnRecallOpen * price );

                    var model = new ExcessCollateralModel { Contract = item, MarkValue = item.Quantity * item.MktPrice };

                    model.Difference = model.MarkValue - item.AmountSettled;

                    modelList.Add( model );
                }

                excessSummary = modelList.GroupBy( x => new { x.Contract.EffectiveDate, x.Contract.EntityId, x.Contract.ContraEntity, x.Contract.AccountName, x.Contract.CollateralFlag, x.Contract.CurrencyCode } )
                    .Select( x => new ExcessCollateralSummaryModel()
                    {
                        EntityId = x.Key.EntityId,
                        ContraEntity = x.Key.ContraEntity,
                        AccountName = x.Key.AccountName,
                        CollateralFlag = x.Key.CollateralFlag,
                        CurrencyCode = x.Key.CurrencyCode,
                        BorrowBalance = x.Where(q => q.Contract.TradeType == TradeType.StockBorrow).Sum(q => q.Contract.Amount),
                        MoneyOut = x.Where( q => q.Contract.TradeType == TradeType.StockBorrow ).Sum( q => q.MarkValue ),
                        LoanBalance = x.Where( q => q.Contract.TradeType == TradeType.StockLoan ).Sum( q => q.Contract.Amount ),
                        MoneyIn = x.Where( q => q.Contract.TradeType == TradeType.StockLoan ).Sum( q => q.MarkValue )
                    } ).ToList();
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( excessSummary.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }


        public ActionResult Read_ExcessNetCollateral([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string contraEntity, SL_CollateralFlag collateralFlag, Currency currency)
        {
            var modelList = new List<ExcessCollateralModel>();

            if (entityId == "") return Json(modelList.ToDataSourceResult(request));
            
            try
            {
                var contractList = DataContracts.LoadContracts(effectiveDate, effectiveDate, entityId).Where(x => x.ContraEntity == contraEntity && x.CollateralFlag == collateralFlag && x.CurrencyCode == currency).ToList();

                foreach (var item in contractList)
                {
                    item.Quantity = item.Quantity - item.QuantityOnRecallOpen;


                    decimal price;
                    try
                    {
                        price = (item.AmountSettled / item.QuantitySettled);
                    }
                    catch
                    {
                        price = 0;
                    }

                    item.Amount = item.Amount - (item.QuantityOnRecallOpen * price);

                    var model = new ExcessCollateralModel {Contract = item, MarkValue = item.Quantity*item.MktPrice};

                    model.Difference = model.MarkValue - item.AmountSettled;

                    modelList.Add(model);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(modelList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_SecurityRisk([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, SummaryKey summaryKey)
        {
            var contractList = new List<SL_ContractExtendedProjection>();
            var boxList = new List<SL_BoxCalculationExtendedProjection>();

            var modelList = new List<SecurityRiskModel>();

            if (entityId == null) return Json(modelList.ToDataSourceResult(request));
            
            try
            {
                if (summaryKey.Equals(SummaryKey.ContraID) || summaryKey.Equals(SummaryKey.Security))
                {
                    contractList = DataContracts.LoadContracts(effectiveDate, effectiveDate, entityId);
                }

                if (summaryKey.Equals(SummaryKey.ContraID))
                {
                    boxList = DataBoxCalculation.LoadBoxCalculation(DateTime.Today, entityId, 0, 100, false, false, false, false);                       
                }

                modelList = GetContractItems(modelList, contractList, summaryKey);
                modelList = GetBoxItems(modelList, boxList, summaryKey);

                foreach (var item in modelList)
                {
                    item.RecieveableAmount = item.Recieveable * item.Price;
                    item.DeliverableAmount = item.Deliverable * item.Price;
                    item.HoldingAmount = item.Holding * item.Price;
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(modelList.ToDataSourceResult(request));
        }

        public List<SecurityRiskModel> GetContractItems(List<SecurityRiskModel> modelList, List<SL_ContractExtendedProjection> contractList, SummaryKey summaryKey)
        {
            foreach (var item in contractList)
            {
                bool found = false;

                foreach (var lookupitem in modelList)
                {
                    if (item.EffectiveDate != lookupitem.EffectiveDate || item.EntityId != lookupitem.EntityId)
                        continue;
                    if ((!summaryKey.Equals(SummaryKey.ContraID) || !item.ContraEntity.Equals(lookupitem.SummaryKey)) &&
                        ((!summaryKey.Equals(SummaryKey.Security) || !item.SecurityNumber.Equals(lookupitem.SummaryKey))))
                        continue;
                    if (item.TradeType == TradeType.StockBorrow)
                    {
                        lookupitem.Deliverable += item.Quantity;
                    }
                    else
                    {
                        lookupitem.Recieveable -= item.Quantity;
                    }

                    found = true;
                    break;
                }

                if (found) continue;
                var tempModel = new SecurityRiskModel {EffectiveDate = item.EffectiveDate, EntityId = item.EntityId};

                if (summaryKey.Equals(SummaryKey.ContraID))
                {
                    tempModel.SummaryKey = item.ContraEntity;
                    tempModel.Ticker = item.AccountName;
                }
                else
                {
                    tempModel.SummaryKey = item.SecurityNumber;
                    tempModel.Ticker = item.Ticker;
                }

                 
                tempModel.Deliverable = 0;
                tempModel.DeliverableAmount = 0;
                tempModel.RecieveableAmount = 0;
                tempModel.Recieveable = 0;
                tempModel.Holding = 0;
                tempModel.HoldingAmount = 0;

                if (item.AmountSettled == 0 || item.QuantitySettled == 0)
                {
                    tempModel.Price = 0;
                }
                else
                {
                    tempModel.Price = (item.AmountSettled / item.QuantitySettled);
                }

                if (item.TradeType == TradeType.StockBorrow)
                {
                    tempModel.Deliverable = item.Quantity;
                }
                else
                {
                    tempModel.Recieveable = item.Quantity * -1;
                }

                tempModel.Classification = item.Classification;

                var random = new Random();

                tempModel.Volatility = random.Next(100);

                modelList.Add(tempModel);
            }

            return modelList;
        }

        public List<SecurityRiskModel> GetBoxItems(List<SecurityRiskModel> modelList, List<SL_BoxCalculationExtendedProjection> boxList, SummaryKey summaryKey)
        {
            if (summaryKey.Equals(SummaryKey.ContraID))
            {
                return modelList;
            }
            
            foreach (var item in boxList)
            {
                var found = false;

                foreach (var lookupitem in modelList.Where(lookupitem => item.EffectiveDate == lookupitem.EffectiveDate && item.EntityId == lookupitem.EntityId && item.SecurityNumber == lookupitem.SummaryKey))
                {
                    lookupitem.Holding += item.ExcessPositionSettled;
                    found = true;
                    break;
                }

                if (found) continue;
                if (item.EffectiveDate == null) continue;
                var tempModel = new SecurityRiskModel
                {
                    EffectiveDate = (DateTime) item.EffectiveDate,
                    EntityId = item.EntityId,
                    SummaryKey = item.SecurityNumber,
                    Ticker = item.Ticker,
                    Classification = item.Classification,
                    Deliverable = 0,
                    DeliverableAmount = 0,
                    RecieveableAmount = 0,
                    Recieveable = 0,
                    Holding = 0,
                    HoldingAmount = 0,
                    Price = (decimal) item.Price
                };

                tempModel.Holding = item.ExcessPositionSettled;

                tempModel.Classification = item.Classification;

                var random = new Random();

                tempModel.Volatility = random.Next(100);

                modelList.Add(tempModel);
            }


            return modelList;
        }     
    }
}
