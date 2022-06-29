using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class RecallController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_Recall([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId)
        {
            List<SL_RecallExtendedProjection> recallList = new List<SL_RecallExtendedProjection>();

            try
            {
                foreach (var _entityId in entityId)
                {
                    recallList.AddRange(DataRecalls.LoadRecallsExtended(effectiveDate, _entityId));
                }
            }
            catch
            {
                recallList = new List<SL_RecallExtendedProjection>();
            }

            return Extended.JsonMax(recallList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_RecallExposure( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, List<string> entityId )
        {
            List<SL_RecallExposureProjection> recallList = new List<SL_RecallExposureProjection>();

            try
            {
                foreach (var _entityId in entityId)
                {
                    recallList.AddRange(DataRecalls.LoadRecallsExposure(effectiveDate, _entityId));
                }
            }
            catch
            {
                recallList = new List<SL_RecallExposureProjection>();
            }

            return Extended.JsonMax(recallList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_RecallByIssue([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, int issueId)
        {
            List<SL_RecallExtendedProjection> recallList;

            try
            {
                recallList = DataRecalls.LoadRecallsByIssue(effectiveDate, entityId, issueId);
            }
            catch
            {
                recallList = new List<SL_RecallExtendedProjection>();
            }

            return Extended.JsonMax(recallList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_RecallByCriteria([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string criteria)
        {
            List<SL_RecallExtendedProjection> recallList;

            try
            {
                Issue issue = DataIssue.LoadIssue(criteria);
                recallList = DataRecalls.LoadRecallsByIssue(effectiveDate, entityId, issue.IssueId);
            }
            catch
            {
                recallList = new List<SL_RecallExtendedProjection>();
            }

            return Extended.JsonMax(recallList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update_Recall(SL_RecallExtendedProjection recall)
        {
            if (recall != null && ModelState.IsValid)
            {
                try
                {
                    //DataExternalOperations.Add(recall);
                }
                catch 
                {

                }
            }

            return null;
        }

        public ActionResult Update_PendingRecall(SL_RecallExtendedProjection recallExtended)
        {
            var recallUpdate = DataRecalls.LoadRecalls((DateTime)recallExtended.EffectiveDate, recallExtended.EntityId).Where(x => x.SLRecall == recallExtended.SLRecall).First();

            if (ModelState.IsValid)
            {

                try
                {
                    recallUpdate.ContraEntity = recallExtended.ContraEntity;
                    recallUpdate.QuantityRecalled = recallExtended.QuantityRecalled;
                    recallUpdate.RecallReason = recallExtended.RecallReason;
                    recallUpdate.RecallDate = (DateTime)recallExtended.RecallDate;
                    recallUpdate.BuyInDate = (DateTime)recallExtended.BuyInDate;
                    recallUpdate.RecallFlag = recallExtended.RecallFlag;

                    DataRecalls.UpdateRecall(recallUpdate);

                    DataActivity.AddRecallActivity(recallExtended, SL_ActivityFlag.Completed);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            return Extended.JsonMax(recallExtended, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_PendingRecallExposure(SL_RecallExposureProjection recallExtended)
        {
            var recallUpdate = DataRecalls.LoadRecalls((DateTime)recallExtended.EffectiveDate, recallExtended.EntityId).Where(x => x.SLRecall == recallExtended.SLRecall).First();

            if (ModelState.IsValid)
            {

                try
                {
                    recallUpdate.ContraEntity = recallExtended.ContraEntity;
                    recallUpdate.QuantityRecalled =(recallExtended.TradeType == TradeType.StockBorrow) ? (decimal) recallExtended.BorrowQuantityRecalled : (decimal) recallExtended.LoanQuantityRecalled;
                    recallUpdate.RecallReason = (recallExtended.TradeType == TradeType.StockBorrow) ? (SL_RecallReason)recallExtended.BorrowRecallReason : (SL_RecallReason)recallExtended.LoanRecallReason;
                    recallUpdate.RecallDate = (DateTime)recallExtended.RecallDate;
                    recallUpdate.BuyInDate = (DateTime)recallExtended.BuyInDate;

                    DataRecalls.UpdateRecall(recallUpdate);

                    DataActivity.AddRecallActivity(recallExtended, SL_ActivityFlag.Completed);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            return Extended.JsonMax(recallExtended, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete_Recall(List<SL_RecallExtendedProjection> recalls)
        {
            List<SL_RecallExtendedProjection> processRecalls = new List<SL_RecallExtendedProjection>();

            if ( recalls != null )
            {
                try
                {
                    foreach ( SL_RecallExtendedProjection item in recalls )
                    {
                        if (( item.Status == SL_RecallStatus.OPEN ) || (item.Status == SL_RecallStatus.PEND))
                        {
                            try
                            {
                                DataRecalls.DeleteRecall( item );
                                processRecalls.Add( item );
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch
                {

                }
            }

            return Extended.JsonMax( processRecalls, JsonRequestBehavior.AllowGet );
        }

        public ActionResult Delete_RecallExposure(List<SL_RecallExposureProjection> recalls)
        {
            
            List<SL_RecallExtendedProjection> recallExtendedList = new List<SL_RecallExtendedProjection>();
            List<SL_RecallExtendedProjection> processRecalls = new List<SL_RecallExtendedProjection>();

            if (recalls != null)
            {
                var _EffectiveDate = (DateTime) recalls.Select(x => x.EffectiveDate).Distinct().First();
                var _EntityId = recalls.Select(x => x.EntityId).Distinct().First();

                recallExtendedList = DataRecalls.LoadRecallsExtended(_EffectiveDate, _EntityId);

                try
                {
                    foreach (SL_RecallExposureProjection item in recalls)
                    {
                        var recallItem = recallExtendedList.Where(x => x.SLRecall == item.SLRecall).First();

                        if ((recallItem.Status == SL_RecallStatus.OPEN) || (recallItem.Status == SL_RecallStatus.PEND))
                        {
                            try
                            {
                                DataRecalls.DeleteRecall(recallItem);
                                processRecalls.Add(recallItem);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch
                {

                }
            }

            return Extended.JsonMax(processRecalls, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BuyIn_Recall( List<SL_RecallExtendedProjection> recalls )
        {

            List<SL_RecallExtendedProjection> processRecalls = new List<SL_RecallExtendedProjection>();

            if ( recalls != null )
            {
                try
                {
                    foreach ( SL_RecallExtendedProjection item in recalls )
                    {
                        if ( item.Status == SL_RecallStatus.OPEN )
                        {
                            try
                            {
                                DataRecalls.AddRecallStatus( item, SL_RecallStatus.BUYIN );
                                processRecalls.Add( item );
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch
                {

                }
            }

            return Json(processRecalls, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Load_RecallAction (SL_RecallExtendedProjection recall)
        {
            var recallList = new List<SL_RecallAction>();

            if (recall == null)
                return PartialView("~/Areas/DomesticTrading/Views/Recall/_RecallActions.cshtml", recallList);
            try
            {
                recallList = DataRecalls.LoadRecallAction(recall);
            }
            catch (Exception)
            {
            }

            return PartialView("~/Areas/DomesticTrading/Views/Recall/_RecallActions.cshtml", recallList);
        }

        public PartialViewResult Load_PendingRecallUpdate(SL_RecallExtendedProjection recall)
        {
            var recallEdit = DataRecalls.LoadRecallsExtended( (DateTime) recall.EffectiveDate, recall.EntityId).Where(x => x.SLRecall == recall.SLRecall).First();

            try
            {
                if (recallEdit.Status != SL_RecallStatus.PEND)
                {
                    recallEdit = null;
                }              
            }
            catch (Exception)
            {
            }
            
            return PartialView("~/Areas/DomesticTrading/Views/Recall/_UpdatePendingRecall.cshtml", recallEdit);
        }

        public PartialViewResult Load_PendingRecallExposureUpdate(SL_RecallExposureProjection recall)
        {
            var recallEdit = DataRecalls.LoadRecallsExtended((DateTime)recall.EffectiveDate, recall.EntityId).Where(x => x.SLRecall == recall.SLRecall).First();

            try
            {
                if (recallEdit.Status != SL_RecallStatus.PEND)
                {
                    recallEdit = null;
                }
            }
            catch (Exception)
            {
            }

            return PartialView("~/Areas/DomesticTrading/Views/Recall/_UpdatePendingRecall.cshtml", recallEdit);
        }

    }
}
