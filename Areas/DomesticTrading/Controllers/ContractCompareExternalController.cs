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

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ContractCompareExternalController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_ContractCompareExternal([DataSourceRequest] DataSourceRequest request)
        {
            var list = DataContractCompareExternal.LoadContractCompare();

            return Extended.JsonMax(list.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_ContractCompareExternal([DataSourceRequest] DataSourceRequest request, SL_ContractCompareExternal contractCompare)
        {
            try
            {
                DataContractCompareExternal.AddContractCompare(contractCompare);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }


            return Extended.JsonMax(new[] { contractCompare }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Destroy_ContractCompareExternal([DataSourceRequest] DataSourceRequest request, SL_ContractCompareExternal contractCompare)
        {
            try
            {
                DataContractCompareExternal.DestroyContractCompare(contractCompare);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { contractCompare }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ContractCompareExternal([DataSourceRequest] DataSourceRequest request, SL_ContractCompareExternal contractCompare)
        {
            var item = DataContractCompareExternal.LoadContractCompareByPK(contractCompare.SLCONTRACTCOMPAREEXTERNAL);

            try
            {

                item.ContraEntity = contractCompare.ContraEntity;
                item.ContraLei = contractCompare.ContraLei;
                item.Lei = contractCompare.Lei;
                item.ExecutionSystemType = contractCompare.ExecutionSystemType;
                item.ExternalConnectionString = contractCompare.ExternalConnectionString;
                item.FileMask = contractCompare.FileMask;
                item.FixedIncomeEntity = contractCompare.FixedIncomeEntity;
                item.IsFixedIncome = contractCompare.IsFixedIncome;
                item.EntityId = contractCompare.EntityId;
                item.PROCESSEDFILE = contractCompare.PROCESSEDFILE;

                DataContractCompareExternal.UpdateContractCompare(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { item }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Create_ContractCompareExternal([DataSourceRequest] DataSourceRequest request, SL_ContractCompareExternal contractCompare)
        {
            try
            {
                DataContractCompareExternal.AddContractCompare(contractCompare);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { contractCompare }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}
