using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Calculators;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Tools;
using SLTrader.Models;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ContraEntityController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_ContraEntityCapitalChargeSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId, Currency currencyCode)
        {
            List<SL_ContraEntityCapitalChargeProjection> contraList = new List<SL_ContraEntityCapitalChargeProjection>();
            List<ContraEntityCapitalChargeSummaryModel> list = new List<ContraEntityCapitalChargeSummaryModel>();

            if (entityId != null)
            {
                foreach (var item in entityId)
                {
                    contraList.AddRange(DataContraEntity.LoadContraEntityCapitalCharge(effectiveDate, item, currencyCode));
                }


                list = contraList.GroupBy(q => new
                {
                    q.EffectiveDate,
                    q.LegalEntityId,
                    q.LegalEntityName,
                    q.ContractCurrency
                }).Select(x => new ContraEntityCapitalChargeSummaryModel()
                {
                    EffectiveDate = x.Key.EffectiveDate,
                    LegalEntityId = x.Key.LegalEntityId,
                    LegalEntityName = x.Key.LegalEntityName,
                    ContractCurrency = x.Key.ContractCurrency,
                    ReportingCurrency = currencyCode,                    
                    ReportingCollateralDepositAmount = x.Sum(m => m.ReportingCollateralDepositAmount),
                    ReportingCollateralRecievedAmount = x.Sum(m => m.ReportingCollateralRecievedAmount),                    
                    ReportingCreditLimitAmount = 0,
                    ReportingNetBorrowLoanAmount = x.Sum(m => m.ReportingNetBorrowLoanAmount),
                    ReportingNetCollateralMarkToMarketAdjustmentAmount = x.Sum(m => m.ReportingNetCollateralMarkToMarketAdjustmentAmount),
                    ReportingNetCollateralReceivedAmount = x.Sum(m => m.ReportingNetCollateralReceivedAmount),
                    ReportingSuggstedNetMarkToMarketAmount = x.Sum(m => m.ReportingSuggstedNetMarkToMarketAmount),
                    ReportingTotalBorrowAmount = x.Sum(m => m.ReportingTotalBorrowAmount),
                    ReportingTotalLoanAmount = x.Sum(m => m.ReportingTotalLoanAmount),
                    ReportingExcessPayment = x.Sum(m => (m.ReportingNetBorrowLoanAmount - m.ReportingNetCollateralReceivedAmount)),
                    ReportingCapitalCharge = x.Sum(m => (m.ReportingNetBorrowLoanAmount - m.ReportingNetCollateralReceivedAmount) - m.NetBorrowLoanAmount * (m.CapitalChargePercent/100) )
                }).ToList();

                foreach (var item in list)
                {

                }
            }

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_ContraEntityCapitalCharge([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId, Currency currencyCode, int legalEntityId, Currency reportingCurrency)
        {
            List<SL_ContraEntityCapitalChargeProjection> contraList = new List<SL_ContraEntityCapitalChargeProjection>();

            if (entityId != null)
            {
                foreach (var item in entityId)
                {
                    contraList.AddRange(DataContraEntity.LoadContraEntityCapitalCharge(effectiveDate, item, reportingCurrency).Where(x => x.LegalEntityId == legalEntityId && x.ContractCurrency == currencyCode).ToList());
                }
            }

            return Extended.JsonMax(contraList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_CapitalRateCharge(SL_CapitalRateCharge item)
        {
            try
            {
               DataCapitalRateCharge.AddCapitalRateCharge(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_CapitalRateCharge([DataSourceRequest] DataSourceRequest request)
        {
            List<SL_CapitalRateCharge> fxRateList = new List<SL_CapitalRateCharge>();
            try
            {
                fxRateList = DataCapitalRateCharge.LoadCapitalRateCharge();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(fxRateList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_CapitalRateCharge(SL_CapitalRateCharge item)
        {
            SL_CapitalRateCharge _item = new SL_CapitalRateCharge();

            try
            {
                _item = DataCapitalRateCharge.LoadCapitalRateChargeByPK(_item.SLCapitalRateCharge);

                _item.Charge = item.Charge;
                _item.ChargeDescription = item.ChargeDescription;
                _item.ChargeType = item.ChargeType;


                DataCapitalRateCharge.UpdateCapitalRateCharge(_item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { _item }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_CapitalRateCharge(SL_CapitalRateCharge item)
        {
            try
            {
                SL_CapitalRateCharge _item = DataCapitalRateCharge.LoadCapitalRateChargeByPK(item.SLCapitalRateCharge);

                DataCapitalRateCharge.DeleteCapitalRateCharge(_item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadLegalEntity([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<SL_ContraEntitiesParentProjection> ();

            if (entityId.Equals("")) return Json(response.ToDataSourceResult(request));

            try
            {
                response = DataContraEntity.LoadContraEntityParentByEntityId(DateTime.Today, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadAvailableContraEntity([DataSourceRequest] DataSourceRequest request, string entityId, string contraEntityParent)
        {
            var response = new List<ContraEntityCompanyModel>();

            if (entityId.Equals("")) return Json(response.ToDataSourceResult(request));

            try
            {
                foreach (var company in SessionService.UserFirms)
                {
                    var contraEntityList = DataContraEntity.LoadContraEntityByEntityId(company.CompanyId.ToString());

                    contraEntityList.RemoveAll(x => x.ContraEntityParent.ToString() == contraEntityParent);

                    foreach (var contraEntityItem in contraEntityList)
                    {
                        ContraEntityCompanyModel model = new ContraEntityCompanyModel()
                        {
                            Company = company,
                            ContraEntity = contraEntityItem
                        };

                        response.Add(model);
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadAssginedContraEntity([DataSourceRequest] DataSourceRequest request, string entityId, string contraEntityParent)
        {
            var response = new List<ContraEntityCompanyModel>();

            if (entityId.Equals("")) return Json(response.ToDataSourceResult(request));

            try
            {
                foreach (var company in SessionService.UserFirms)
                {
                    var contraEntityList = DataContraEntity.LoadContraEntityByEntityId(company.CompanyId.ToString());

                    contraEntityList.RemoveAll(x => x.ContraEntityParent.ToString() != contraEntityParent);

                    foreach (var contraEntityItem in contraEntityList)
                    {
                        ContraEntityCompanyModel model = new ContraEntityCompanyModel()
                        {
                            Company = company,
                            ContraEntity = contraEntityItem
                        };

                        response.Add(model);
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_ContraEntityBreakOut([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var response = new List<SL_ContraEntitiesBreakOutSummaryProjection>();

            if (entityId.Equals("")) return Json(response.ToDataSourceResult(request));

            try
            {
                response = DataContraEntity.LoadContraEntityBreakOut(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Read_ContraEntitySummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            var response = new List<SL_ContraEntitiesSummaryProjection>();

            if (entityId.Equals("")) return Json(response.ToDataSourceResult(request));

            try
            {
                response = DataContraEntity.LoadContraEntity(effectiveDate, entityId);         
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ContraEntityParentSummary( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            var response = new List<SL_ContraEntitiesParentProjection>();

            if ( entityId.Equals( "" ) ) return Json( response.ToDataSourceResult( request ) );

            try
            {
                response = DataContraEntity.LoadContraEntityParentByEntityId( effectiveDate, entityId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadLegalEntityAssignment(string entityId)
        {
            return PartialView("~/Areas/DomesticTrading/Views/ContraEntity/_LegalEntityManagement.cshtml", entityId);
        }

        public PartialViewResult LoadCapitalChargeRatePartial()
        {
            return PartialView("~/Areas/DomesticTrading/Views/ContraEntity/_CapitalRateCharge.cshtml");
        }


        public ActionResult Read_ContraEntityDetailList( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            var response = new List<SL_ContraEntity>();

            if ( entityId.Equals( "" ) ) return Json( response.ToDataSourceResult( request ) );
            try
            {
                response = StaticDataCache.ContraEntityStaticGet(entityId);

                //response = DataContraEntity.LoadContraEntityByEntityId( entityId );             
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ContraEntitySummaryChart([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_ContraEntitiesSummaryProjection> response;

            try
            {
                response = DataContraEntity.LoadContraEntity(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            response = response.Where(x => x.LoanAmount > 0 || x.BorrowAmount > 0).ToList();

            return Extended.JsonMax(response.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ContraEntity([DataSourceRequest] DataSourceRequest request, SL_ContraEntitiesSummaryProjection contraSummary)
        {
            try
            {
                SL_ContraEntity contraEntity = DataContraEntity.LoadContraEntity( contraSummary.ContraEntity.EntityId, contraSummary.ContraEntity.ContraEntity );

                contraEntity.IsEnabled = contraSummary.IsEnabled;
                contraEntity.AllowBorrow = contraSummary.AllowBorrow;
                contraEntity.AllowLoan = contraSummary.AllowBorrow;
                contraEntity.IsFPL = contraSummary.IsFPL;

                contraEntity.InternalLimit = (decimal)contraSummary.InternalLimit;

                DataContraEntity.UpdateContraEntity( contraEntity );

                contraSummary.ContraEntity = contraEntity;

                contraEntity.IsEnabled = contraEntity.IsEnabled;
                contraEntity.AllowBorrow = contraEntity.AllowBorrow;
                contraEntity.AllowLoan = contraEntity.AllowBorrow;
                contraEntity.IsFPL = contraEntity.IsFPL;
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Json(new [] {contraSummary}.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ContraEntityDetail( [DataSourceRequest] DataSourceRequest request, SL_ContraEntity contraEntityDetail )
        {
            SL_ContraEntity contraEntity = new SL_ContraEntity();
            
            if ( ModelState.IsValid )
            {               
                try
                {
                    if ( contraEntityDetail.SLContraEntity == -1 )
                    {
                        DataContraEntity.AddContraEntity( contraEntityDetail );
                    }
                    else
                    {
                        contraEntity = DataContraEntity.LoadContraEntity( contraEntityDetail.EntityId, contraEntityDetail.ContraEntity );

                        contraEntity.IsEnabled = contraEntityDetail.IsEnabled;

                        if ( bool.Parse( DataSystemValues.LoadSystemValue( "AllowContraEntityEdit", "false" ) ) )
                        {
                            contraEntity.ContraEntityParent = contraEntityDetail.ContraEntityParent;
                            contraEntity.AccountName = contraEntityDetail.AccountName;
                            contraEntity.AddressLine1 = contraEntityDetail.AddressLine1;
                            contraEntity.AddressLine2 = contraEntityDetail.AddressLine2;
                            contraEntity.AddressLine3 = contraEntityDetail.AddressLine3;
                            contraEntity.AddressLine4 = contraEntityDetail.AddressLine4;
                            contraEntity.AssocCBEntity = contraEntityDetail.AssocCBEntity;
                            contraEntity.AssocEntity = contraEntityDetail.AssocEntity;
                            contraEntity.BizAmt = contraEntityDetail.AssocEntity;
                            contraEntity.BizIndex = contraEntityDetail.BizIndex;
                            contraEntity.InternalLimit = contraEntityDetail.InternalLimit;
                            contraEntity.BNDBorrowRate = contraEntityDetail.BNDBorrowRate;
                            contraEntity.BNDLoanRate = contraEntityDetail.BNDLoanRate;
                            contraEntity.BorrowColl = contraEntityDetail.BorrowColl;
                            contraEntity.BorrowMarkCode = contraEntityDetail.BorrowMarkCode;
                            contraEntity.BorrowDateChng = ( contraEntity.BorrowColl != contraEntityDetail.BorrowColl ) ? DateTime.Today : contraEntity.BorrowDateChng;
                            contraEntity.LoanColl = contraEntityDetail.LoanColl;
                            contraEntity.LoanMarkCode = contraEntityDetail.LoanMarkCode;
                            contraEntity.LoanDateChng = ( contraEntity.LoanColl != contraEntityDetail.LoanColl ) ? DateTime.Today : contraEntity.LoanDateChng;
                            contraEntity.STKBorrowRate = contraEntityDetail.STKBorrowRate;
                            contraEntity.STKLoanRate = contraEntityDetail.STKLoanRate;
                            contraEntity.IsEnabled = contraEntityDetail.IsEnabled;
                            contraEntity.BorrowLmt = contraEntityDetail.BorrowLmt;
                            contraEntity.FaxNumber = contraEntityDetail.FaxNumber;
                            contraEntity.LoanLmt = contraEntityDetail.LoanLmt;
                            contraEntity.LEINumber = contraEntityDetail.LEINumber;
                            contraEntity.AlternateContractCompare = contraEntityDetail.AlternateContractCompare;
                        }

                        DataContraEntity.UpdateContraEntity( contraEntity );

                        contraEntity = DataContraEntity.LoadContraEntity( contraEntityDetail.EntityId, contraEntityDetail.ContraEntity );
                    }
                }
                catch ( Exception e )
                {
                    return ThrowJsonError( e );
                }
            }

            return Json( new[] { contraEntity }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Update_ContraEntityParentDetail([DataSourceRequest] DataSourceRequest request, SL_ContraEntityParent contraEntityParentDetail)
        {
            SL_ContraEntityParent contraEntityParent = new SL_ContraEntityParent();

            if (ModelState.IsValid)
            {
                try
                {
                    contraEntityParent = DataContraEntity.LoadContraEntityParentByPK(contraEntityParentDetail.SLContraEntityParent);

                    contraEntityParent.IsEnabled = contraEntityParentDetail.IsEnabled;

                    contraEntityParent.AccountName = contraEntityParentDetail.AccountName;
                    contraEntityParent.BorrowLmt = contraEntityParentDetail.BorrowLmt;
                    contraEntityParent.LoanLmt = contraEntityParentDetail.LoanLmt;
                    contraEntityParent.PrepayLimit = contraEntityParentDetail.PrepayLimit;

                    DataContraEntity.UpdateContraEntityParent(contraEntityParent);
                }
                catch (Exception e)
                {
                    return ThrowJsonError(e);
                }
            }

            return Json(new[] { contraEntityParent }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_ContraEntityDetail( [DataSourceRequest] DataSourceRequest request, SL_ContraEntity contraEntityDetail )
        {
            try
            {
                DataContraEntity.AddContraEntity( contraEntityDetail );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( new[] { contraEntityDetail }.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult Read_EntityDropdown([DataSourceRequest] DataSourceRequest request)
        {
            return Extended.JsonMax(SessionService.UserFirms, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ContraEntityDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<SL_ContraEntity>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                response = StaticDataCache.ContraEntityStaticGet(entityId).Where(x => x.IsEnabled == true).ToList();
                //response = DataContraEntity.LoadContraEntity(entityId).Where(x => x.IsEnabled == true).ToList();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ContraEntityDropdownByTradeType([DataSourceRequest] DataSourceRequest request, string entityId, TradeType? tradeType)
        {
            var response = new List<SL_ContraEntity>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                if (tradeType == null)
                {
                    response = StaticDataCache.ContraEntityStaticGet(entityId).Where(x => x.IsEnabled == true).ToList();
                }
                else
                {
                    if ((tradeType == TradeType.FullyPaidBorrow) || (tradeType == TradeType.FullyPaidLoan))
                    {
                        response = StaticDataCache.ContraEntityStaticGet(entityId).Where(x => x.IsEnabled == true && x.IsFPL == true).ToList();
                    }
                    else
                    {
                        response = StaticDataCache.ContraEntityStaticGet(entityId).Where(x => x.IsEnabled == true).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ContraEntitySummaryDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_ContraEntity> response = new List<SL_ContraEntity>();
            List<SL_ContraEntitiesSummaryProjection> contraSummaryList = new List<SL_ContraEntitiesSummaryProjection>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                response = DataContraEntity.LoadContraEntity(entityId).Where(x => x.IsEnabled == true).ToList();

                contraSummaryList = response.Select(x => new SL_ContraEntitiesSummaryProjection()
                {
                    EntityId = x.EntityId,
                    ContraEntityId = x.ContraEntity,
                    ContraEntityName = x.AccountName
                }).ToList();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(contraSummaryList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ContraEntityParentDropdown([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var response = new List<SL_ContraEntitiesParentProjection>();

            if ((entityId.Equals(""))) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                response = DataContraEntity.LoadContraEntityParentByEntityId(DateTime.Today, entityId).ToList();
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(response, JsonRequestBehavior.AllowGet);
        }



        public JsonResult Read_ContraEntityDetailResult([DataSourceRequest] DataSourceRequest request, string entityId, string contraEntityId)
        {
            var response = new SL_ContraEntity();

            if (entityId.Equals("")) return Json(response, JsonRequestBehavior.AllowGet);
            try
            {
                response = DataContraEntity.LoadContraEntity(entityId, contraEntityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ContraEntityDetailResultWithPrice([DataSourceRequest] DataSourceRequest request, string entityId, string contraEntityId, decimal marketPrice)
        {           
            var contraEntityModel = new ContraEntityWithPriceModel();

            if (entityId.Equals("")) return Json(contraEntityModel, JsonRequestBehavior.AllowGet);
            try
            {
                contraEntityModel.ContraEntity = DataContraEntity.LoadContraEntity(entityId, contraEntityId);

                contraEntityModel.Price = SLTradeCalculator.CalculatePrice(entityId, marketPrice, contraEntityModel.ContraEntity);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(contraEntityModel, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Read_ContraEntityDetail(string entityId, string contraEntityId)
        {
            var item = new SL_ContraEntity();

            if ( contraEntityId.Equals( "" ) )
            {
                item.EntityId = entityId;
                item.ContraEntity = "";
                item.SLContraEntity = -1;
                item.AccountName = "";
                item.AddressLine1 = "";
                item.AddressLine2 = "";
                item.AddressLine3 = "";
                item.AddressLine4 = "";
                item.BorrowColl = "102";
                item.BorrowLmt = 0;
                item.BorrowMarkCode = "%";
                item.BorrowSecurityLmt = 0;
                item.LoanColl = "102";
                item.LoanLmt = 0;
                item.LoanMarkCode = "%";
                item.LoanSecurityLmt = 0;
                item.MarkRndHse = 0.9;
                item.MarkValInst = "U";
                item.MarkValHse = 0.9;
                item.MarkRndInst = "U";
                item.MinMarkPrice = 1;
                item.MinMarkAmt = 499;
                item.ContraEntityParent = -1;
                item.AlternateContractCompare = false;
            }
            else
            {
                item = DataContraEntity.LoadContraEntity( entityId, contraEntityId );
            }

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContraEntityDetail.cshtml", item);
        }

        public PartialViewResult Read_ContraEntityDetailContactByContract(string entityId, string contraEntity)
        {
            var contraEntityItem = DataContraEntity.LoadContraEntity(entityId, contraEntity);

            var item = DataContraEntity.LoadContraEntityByPk((int)contraEntityItem.SLContraEntity);

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContraEntityDetailContact.cshtml", item);
        }



        public PartialViewResult Read_ContraEntityDetailContact(string contraEntityId)
        {
            var item = DataContraEntity.LoadContraEntityByPk(int.Parse(contraEntityId));

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContraEntityDetailContact.cshtml", item);
        }

        public PartialViewResult Read_ContraEntityDetailProfile(string entityId, string contraEntityId)
        {
            var item = DataContraEntity.LoadContraEntityByPk(int.Parse(contraEntityId));

            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContraEntityProfile.cshtml", item);
        }

        public JsonResult Change_ContraEntityBoxRate(string entityId, string contraEntity, string stockType, TradeType tradeType, decimal rate)
        {
            bool response;

            try
            {
                response = DataExternalOperations.AddContraEntityBoxRate(entityId, contraEntity, stockType, tradeType, rate);
            }
            catch
            {
                response = false;
            }

            return Json(response);
        }

        public ActionResult Read_ContraEntityByContraParent( [DataSourceRequest] DataSourceRequest request, string entityId, decimal contraEntityParent )
        {
            List<SL_ContraEntitiesSummaryProjection> contraEntityList = new List<SL_ContraEntitiesSummaryProjection>();

            try
            {
                contraEntityList =  DataContraEntity.LoadContraEntityByContraParent( entityId, contraEntityParent );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( contraEntityList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }


        public JsonResult AddLegalEntityContraEntity(List<ContraEntityCompanyModel> contraEntityList, int contraEntityParentId)
        {
            foreach (var item in contraEntityList)
            {
                var _contra = DataContraEntity.LoadContraEntityByEntityId(item.ContraEntity.EntityId).Where(x => x.ContraEntity == item.ContraEntity.ContraEntity).First();

                _contra.ContraEntityParent = contraEntityParentId;
                DataContraEntity.UpdateContraEntity(_contra);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }


        public JsonResult DeleteLegalEntityContraEntity(List<ContraEntityCompanyModel> contraEntityList)
        {
            foreach (var item in contraEntityList)
            {
                var _contra = DataContraEntity.LoadContraEntityByEntityId(item.ContraEntity.EntityId).Where(x => x.ContraEntity == item.ContraEntity.ContraEntity).First();

                _contra.ContraEntityParent = -1;
                DataContraEntity.UpdateContraEntity(_contra);
            }

            return Extended.JsonMax(true, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Read_ContraEntityParentDetail(decimal contraEntityParentId)
        {
            var contraEntityParent = DataContraEntity.LoadContraEntityParentByPK(contraEntityParentId);


            return PartialView("~/Areas/DomesticTrading/Views/Shared/Templates/_ContraEntityParentDetail.cshtml", contraEntityParent);
          
        }

        public JsonResult Read_ContraEntityProfile([DataSourceRequest] DataSourceRequest request, string entityId,string contraEntityId)
        {
            List<SL_ContraEntityProfile> contraEntityProfileList = new List<SL_ContraEntityProfile>();

            try
            {
                contraEntityProfileList = DataContraEntityProfile.LoadContraEntityProfileByEntityIdAndContraEntity(entityId, contraEntityId);
            }
            catch
            {
                contraEntityProfileList = new List<SL_ContraEntityProfile>();
            }

            return Json(contraEntityProfileList.ToDataSourceResult(request));
        }

        public JsonResult Add_ContraEntityProfile(SL_ContraEntityProfile item)
        {
            try
            {
                DataContraEntityProfile.AddContraEntityProfile(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete_ContraEntityProfile(SL_ContraEntityProfile item)
        {
            try
            {
                var contraEntityProfileItem = DataContraEntityProfile.LoadContraEntityProfileByPk((int)item.SLContraEntityProfile);
                DataContraEntityProfile.DeleteContraEntityProfile(contraEntityProfileItem);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update_ContraEntityProfile(SL_ContraEntityProfile item)
        {
            SL_ContraEntityProfile _ContraEntityProfile = new SL_ContraEntityProfile();

            try
            {
                _ContraEntityProfile = DataContraEntityProfile.LoadContraEntityProfileByPk((int)item.SLContraEntityProfile);

                _ContraEntityProfile.TradeType = item.TradeType;
                _ContraEntityProfile.BatchCode = string.IsNullOrWhiteSpace(item.BatchCode) ? "" : item.BatchCode;
                _ContraEntityProfile.IncomeTracked = item.IncomeTracked;
                _ContraEntityProfile.Currency = item.Currency;
                _ContraEntityProfile.Rate = item.Rate;

                DataContraEntityProfile.UpdateContraEntityProfile(_ContraEntityProfile);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { item }, JsonRequestBehavior.AllowGet);
        }
    }
}
