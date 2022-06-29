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

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class PCMatrixController : BaseController
    {
        // GET: DomesticTrading/PCMatrix
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Read_PCMatrix([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_PCMatrix> list;

            try
            {
                list = DataPCMatrix.LoadPCMatrix(entityId);
            }
            catch (Exception)
            {
                list = new List<SL_PCMatrix>();
            }

            return Extended.JsonMax(list.OrderBy(X => X.PC).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_PCMatrix(SL_PCMatrix pcMatrix)
        {
            try
            {
                decimal testDecimal = 0;

                switch(pcMatrix.ColumnTypeId)
                {
                    case SL_ColumnType.AMOUNT:
                    case SL_ColumnType.QUANTITY:
                    case SL_ColumnType.REBATERATE:
                        if (!decimal.TryParse(pcMatrix.Value,System.Globalization.NumberStyles.Any, null, out testDecimal))
                        {
                            return ThrowJsonError("The column type " + pcMatrix.ColumnTypeId + " requires a numeric value.");
                        }
                        
                    break;
                }

                DataPCMatrix.AddPCMatrix(pcMatrix);
            }
            catch(Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax(new[] { pcMatrix }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_PCMatrix(SL_PCMatrix pcMatrix)
        {
            try
            {
                SL_PCMatrix tempPCMatrix = DataPCMatrix.LoadPCMatrixByPK(pcMatrix.SLPCMatrix);

                tempPCMatrix.PC = pcMatrix.PC;
                tempPCMatrix.ColumnTypeId = pcMatrix.ColumnTypeId;
                tempPCMatrix.OperatorId = pcMatrix.OperatorId;
                tempPCMatrix.Value = pcMatrix.Value;
                tempPCMatrix.Priority = pcMatrix.Priority;
                tempPCMatrix.IsActive = pcMatrix.IsActive;

                decimal testDecimal = 0;

                switch (pcMatrix.ColumnTypeId)
                {
                    case SL_ColumnType.AMOUNT:
                    case SL_ColumnType.QUANTITY:
                    case SL_ColumnType.REBATERATE:
                        if (!decimal.TryParse(pcMatrix.Value, System.Globalization.NumberStyles.Any, null, out testDecimal))
                        {
                            return ThrowJsonError("The column type " + pcMatrix.ColumnTypeId + " requires a numeric value.");
                        }
                        break;
                }

                DataPCMatrix.UpdatePCMatrix(tempPCMatrix);
            }
            catch (Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax(new[] { pcMatrix }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Delete_PCMatrix(SL_PCMatrix pcMatrix)
        {
            try
            {
                SL_PCMatrix tempPCMatrix = DataPCMatrix.LoadPCMatrixByPK(pcMatrix.SLPCMatrix);


                DataPCMatrix.DeletePCMatrix(tempPCMatrix);
            }
            catch (Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax(new[] { pcMatrix }, JsonRequestBehavior.AllowGet);
        }
    }
}