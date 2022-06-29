using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using System.Linq.Dynamic;


using BondFire.Core.Dates;

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
using SLTrader.Models.FailRelatedModels;

namespace SLTrader.Areas.FailMaster.Controllers
{

    public class FailMasterController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadFailMaster([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_FailMasterExtendedProjection> failList = new List<SL_FailMasterExtendedProjection>();

            try
            {
                if (!entityId.Equals(""))
                {
                    failList = StaticDataCache.FailMasterStaticGet(effectiveDate, entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }
            

            return Extended.JsonMax(failList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadFailMasterByCriteria([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, FailExposureEnum failExposureType, string key )
        {
            List<SL_FailMasterExtendedProjection> failList = new List<SL_FailMasterExtendedProjection>();

            try
            {
                if (!entityId.Equals(""))
                {
                    failList = StaticDataCache.FailMasterStaticGet(effectiveDate, entityId);

                    switch ( failExposureType)
                    {
                        case FailExposureEnum.Classification:
                            failList = failList.Where(x => x.Classification == key).ToList();
                            break;
                            
                        case FailExposureEnum.DeliveryDate:
                            failList = failList.Where(x => x.DeliveryDate.ToString() == key).ToList();
                            break;

                        case FailExposureEnum.ShortName:
                            failList = failList.Where(x => x.ShortName == key).ToList();
                            break;

                        case FailExposureEnum.Eligible:
                            failList = failList.Where(x => x.DtccEligible.ToString() == key).ToList();
                            break;

                        case FailExposureEnum.DepoStatus:
                            failList = failList.Where(x => x.CurrentDepoStatus.ToString() == key).ToList();
                            break;

                        case FailExposureEnum.BlotterCodeCategory:
                            failList = failList.Where(x => x.BlotterCodeCategory.ToString() == key).ToList();
                            break;
                    }
                    
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }


            return Extended.JsonMax(failList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadFailMasterExposureSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, FailExposureEnum failExposureType)
        {
            List<FailExposureByModel> failList = new List<FailExposureByModel>();

            try
            {
                
                if (!entityId.Equals(""))
                {                
                    List<SL_FailMasterExtendedProjection> failExtendedList = DataFailMaster.LoadFailMaster(effectiveDate, entityId);

                    switch (failExposureType)
                    {
                        case FailExposureEnum.Classification:
                            failList = FailExposureByModel.SummaryByClassification(failExtendedList);
                            break;
                        case FailExposureEnum.DeliveryDate:
                            failList = FailExposureByModel.SummaryByDeliveryDate(failExtendedList);
                            break;
                        case FailExposureEnum.Eligible:
                            failList = FailExposureByModel.SummaryByEligible(failExtendedList);
                            break;
                        case FailExposureEnum.ShortName:
                            failList = FailExposureByModel.SummaryByShortName(failExtendedList);
                            break;

                        case FailExposureEnum.DepoStatus:
                            failList = FailExposureByModel.SummaryByDepoStatus(failExtendedList);
                            break;

                        case FailExposureEnum.BlotterCodeCategory:
                            failList = FailExposureByModel.SummaryByBlotterCodeCategory(failExtendedList);
                            break;

                        default:
                            failList = new List<FailExposureByModel>();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }


            return Extended.JsonMax(failList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }   

        public ActionResult Read_FailMasterBlotterCodeByEntity([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_FailMasterBlotterCode> failList = new List<SL_FailMasterBlotterCode>();

            try
            {
                if (!entityId.Equals(""))
                {
                    failList = DataFailMaster.LoadFailMasterBlotterCode(entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }


            return Extended.JsonMax(failList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Create_FailMasterBlotterCode(SL_FailMasterBlotterCode failBlotterCode)
        {
            try
            {
                DataFailMaster.AddFailMasterBlotterCode(failBlotterCode);
            }
            catch (Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax(new[] { failBlotterCode }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_FailMasterBlotterCode(SL_FailMasterBlotterCode failBlotterCode)
        {
            try
            {
                SL_FailMasterBlotterCode tempFailMasterBlotterCode = DataFailMaster.LoadFailMasterBlotterCodeByPK(failBlotterCode.SLFailMasterBlotterCode);

                tempFailMasterBlotterCode.BlotterCode = failBlotterCode.BlotterCode;
                tempFailMasterBlotterCode.BlotterCodeCategory = failBlotterCode.BlotterCodeCategory;
                tempFailMasterBlotterCode.MaximumAge = failBlotterCode.MaximumAge;
                tempFailMasterBlotterCode.MINAGE = failBlotterCode.MINAGE;
                tempFailMasterBlotterCode.UseAgeCategory = failBlotterCode.UseAgeCategory;

                DataFailMaster.UpdateFailMasterBlotterCode(tempFailMasterBlotterCode);

                failBlotterCode = tempFailMasterBlotterCode;
            }
            catch (Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax(new[] { failBlotterCode }, JsonRequestBehavior.AllowGet);
        }
    }
}
