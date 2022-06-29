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

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ClientNeedController : BaseController
    {
        public ActionResult Index()
        {
           
            return View();
        }

        public JsonResult LoadClientNeedCount()
        {
            var dataCount = DataClientNeed.LoadClientNeedNotification( DateTime.Today ).Count();

            return Extended.JsonMax( dataCount, JsonRequestBehavior.AllowGet );
        }

        public JsonResult Read_ClientNeed([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, decimal slClientEmailActionId)
        {
            List<SL_ClientNeedProjection> clientNeedList = new List<SL_ClientNeedProjection>();

            try
            {     
                clientNeedList = DataClientNeed.LoadClientNeed(effectiveDate, entityId);

                if (clientNeedList.Any(x => x.SLClientEmailActionId == slClientEmailActionId))
                {
                    clientNeedList.RemoveAll(x => x.SLClientEmailActionId != slClientEmailActionId);
                }
                else
                {
                    clientNeedList = new List<SL_ClientNeedProjection>();
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(clientNeedList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public JsonResult Destroy_ClientNeed([DataSourceRequest] DataSourceRequest request, SL_ClientNeed clientNeed)
        {
            try
            {
                SL_ClientNeed _ClientNeed = new SL_ClientNeed();

                _ClientNeed = DataClientNeed.LoadClientNeedByPK(clientNeed.SLClientNeed);

                DataClientNeed.DeleteClientNeed(_ClientNeed);
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { clientNeed }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_ClientNeedSummary([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_InventorySummaryFileProjection> clientNeedList = new List<SL_InventorySummaryFileProjection>();

            try
            {
                clientNeedList = DataClientNeed.LoadClientNeedSummary(effectiveDate, entityId, SessionService.SecurityContext);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(clientNeedList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public JsonResult Read_ClientNeedDropdown( DateTime? effectiveDate, string entityId )
        {
            List<SL_ClientNeedProjection> clientNeedList;

            try
            {
                if ( effectiveDate == null )
                {
                    effectiveDate = DateTime.Today;
                }

                clientNeedList = DataClientNeed.LoadClientNeed( (DateTime)effectiveDate, entityId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Json( clientNeedList.Select( x => new { x.Source, x.EmailAddress } ).Distinct().ToList(), JsonRequestBehavior.AllowGet );
        }     


        public JsonResult LoadClientNeedList(string entityId, string clientId)
        {
            List<SL_ClientNeedProjection> clientNeedList;

            try
            {
               
                clientNeedList = DataClientNeed.LoadClientNeed( DateTime.Today, entityId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Json( clientNeedList.Where(x => x.Source == clientId).ToList(), JsonRequestBehavior.AllowGet );
        }

        public JsonResult GetClientNeedNotification(DateTime effectiveDate, string entityId)
        {
            List<SL_ClientNeedProjection> clientNeedList;

            try
            {
                clientNeedList = DataClientNeed.LoadClientNeed(effectiveDate, entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(clientNeedList.Count, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadAddClientNeedList(string entityId, string emailAddress, string subject, string needList)
        {
            long count = 0;
            string displayMsg = "";

            List<ParsedListModel> parsedList = new List<ParsedListModel>();


            SL_ClientEmailAction action = new SL_ClientEmailAction()
            {
                EntityId = entityId,
                FileType = SL_FileType.NEED,
                ItemProcessed = count,
                Subject = subject,
                ClientEmail = -1,
                Source = emailAddress
            };


            DataNotification.AddClientEmailAction(action);

            try
            {
                parsedList = ListParsingService.GenerateList(entityId, needList, "");

                List<SL_ClientNeed> list = ListParsingService.GenerateClientNeed(parsedList, emailAddress, action.SLClientEmailAction);

                list.RemoveAll(x => x.IssueId == -1);

                foreach (var item in parsedList.Where(x => x.IssueId == -1).ToList())
                {
                    var message = item.EntityId + item.SecurityNumber + item.Quantity.ToString();
                }

                displayMsg = string.Format("Processed {0} items out of {1} items, with {2} errors", count, parsedList.Count(), parsedList.Count(x => x.IssueId == -1));

                action.ItemProcessed = list.Count();

                DataNotification.UpdateClientEmailAction(action);

                if (count > 0)
                {
                    SL_Activity activity = new SL_Activity()
                    {
                        EffectiveDate = DateCalculator.Default.Today,
                        EntityId = entityId,
                        Activity = string.Format("Uploaded {0} needs, from Source: {1}", count, emailAddress),
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
    }
}
