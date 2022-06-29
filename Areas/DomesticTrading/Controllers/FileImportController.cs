using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Data;
using System.Linq;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class FileImportController : BaseController
    {
        public ActionResult Index()
        {
           
            return View();
        }


        public JsonResult Read_ClientFileProjection([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<ClientFileProjection> clientList = new List<ClientFileProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    clientList = DataClientFile.LoadClientFile(entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(clientList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        



        public JsonResult Update_ClientFileProjection(ClientFileProjection itemProjection)
        {
            try
            {
                var client = DataClientFile.LoadClient(itemProjection.clientItem.EntityId.ToString()).Where(x => x.SLClientID == itemProjection.clientItem.SLClientID).First();


                client.SourceId = itemProjection.clientItem.SourceId;
                client.Description = itemProjection.clientItem.Description;

                var clientFile = DataClientFile.LoadClientFile(itemProjection.clientItem.EntityId.ToString(), itemProjection.Item.SLClientID.ToString()).Where(x => x.Item.SLClientFileID == itemProjection.Item.SLClientFileID).First();

                clientFile.Item.UsePriorBusinessDay = itemProjection.Item.UsePriorBusinessDay;
                clientFile.Item.IsEasySource = itemProjection.Item.IsEasySource;


                DataClientFile.UpdateClient(client);
                DataClientFile.UpdateClientFile(clientFile.Item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(itemProjection, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_Client([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<Client> clientList = new List<Client>();        
                           
            try
            {
                if (!string.IsNullOrWhiteSpace(entityId))
                {
                    clientList = DataClientFile.LoadClient(entityId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(clientList.ToDataSourceResult(request));
        }

        public JsonResult Read_ClientFile([DataSourceRequest] DataSourceRequest request, string entityId, string clientId)
        {
            List<ClientFileProjection> clientFileProjectionList = new List<ClientFileProjection>();

            try
            {
                if (!string.IsNullOrWhiteSpace(entityId) && !string.IsNullOrWhiteSpace(clientId))
                {
                    clientFileProjectionList = DataClientFile.LoadClientFile(entityId, clientId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(clientFileProjectionList.ToDataSourceResult(request));
        }

        public JsonResult Read_ClientFileLayout([DataSourceRequest] DataSourceRequest request, string clientFileId)
        {
            List <ClientFileLayout> clientFileLayoutList = new List<ClientFileLayout>();

            try
            {
                if (!string.IsNullOrWhiteSpace(clientFileId))
                {
                    clientFileLayoutList = DataClientFile.LoadClientFileLayout(clientFileId);
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(clientFileLayoutList.ToDataSourceResult(request));
        }


        public JsonResult Update_ClientFile(ClientFileProjection itemProjection)
        {            
            try
            {
                DataClientFile.UpdateClientFile(itemProjection.Item);                
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(itemProjection, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Update_Client(Client item)
        {
            try
            {
                DataClientFile.UpdateClient(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(item, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadFileDownloadConfigPartial()
        {
            var list = DataClientFile.LoadFileDownloadConfig();


            return PartialView("~/Areas/DomesticTrading/Views/FileImport/_FileDownLoadConfig.cshtml", list);
        }
    }
}
