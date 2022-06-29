using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
namespace SLTrader.Areas.Locates.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class ClientController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create_LocateClient([DataSourceRequest] DataSourceRequest request, SL_LocateClient m_LocateClient)
        {
            try
            {
                m_LocateClient.Client = String.IsNullOrWhiteSpace(m_LocateClient.Client) ? "" : m_LocateClient.Client;
                m_LocateClient.ClientName = String.IsNullOrWhiteSpace(m_LocateClient.ClientName) ? "" : m_LocateClient.ClientName;
                m_LocateClient.EmailAddress = String.IsNullOrWhiteSpace(m_LocateClient.EmailAddress) ? "" : m_LocateClient.EmailAddress;

                m_LocateClient.MinPrice = 0;
                m_LocateClient.MinQty = 100;
                m_LocateClient.AutoApprovalQty = 1000000;
                m_LocateClient.InventoryPercentage = 1;

                m_LocateClient.AllowNoPartial = false;
                m_LocateClient.AllowPend = false;
                m_LocateClient.AllowEasy = true;
                m_LocateClient.AllowRestricted = true;                
                m_LocateClient.AllowThreshold = true;
                m_LocateClient.AllowNoLend = true;

                m_LocateClient.DecrementationTypeId = SL_LocateDecrementationType.CLIENT;
                m_LocateClient.ClientTypeId = SL_LocateClientType.Prime;
                m_LocateClient.IsActive = true;
                
                DataLocate.AddLocateClient(m_LocateClient);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { m_LocateClient }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update_LocateClient([DataSourceRequest] DataSourceRequest request, SL_LocateClient m_LocateClient)
        {
            SL_LocateClient locateClient = new SL_LocateClient();

            try
            {
                locateClient = DataLocate.LoadLocateClient(m_LocateClient.SLLocateClient);

                locateClient.Client = String.IsNullOrWhiteSpace(m_LocateClient.Client) ? "" : m_LocateClient.Client;
                locateClient.ClientName = String.IsNullOrWhiteSpace(m_LocateClient.ClientName) ? "" : m_LocateClient.ClientName;
                locateClient.EmailAddress = String.IsNullOrWhiteSpace(m_LocateClient.EmailAddress) ? "" : m_LocateClient.EmailAddress;

                locateClient.AllowEasy = m_LocateClient.AllowEasy;
                locateClient.AllowNoLend = m_LocateClient.AllowNoLend;
                locateClient.AllowRestricted = m_LocateClient.AllowRestricted;                
                locateClient.AllowThreshold = m_LocateClient.AllowThreshold;
                locateClient.AutoApprovalQty = m_LocateClient.AutoApprovalQty;
                locateClient.ClientTypeId = m_LocateClient.ClientTypeId;
                locateClient.DecrementationTypeId = m_LocateClient.DecrementationTypeId;
                locateClient.InventoryPercentage = m_LocateClient.InventoryPercentage;
                locateClient.MinPrice = m_LocateClient.MinPrice;
                locateClient.MinQty = m_LocateClient.MinQty;
                locateClient.AllowPend = m_LocateClient.AllowPend;
                locateClient.AllowNoPartial = m_LocateClient.AllowNoPartial;
                locateClient.IsActive = m_LocateClient.IsActive;

                DataLocate.UpdateLocateClient(locateClient);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { locateClient }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Load_LocateClient([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_LocateClient> locateClientList = DataLocate.LoadLocateClient(entityId);

            return Extended.JsonMax(locateClientList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ClientEmail([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            var clientEmailList = DataNotification.LoadClientEmail(entityId);

            return Extended.JsonMax(clientEmailList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_ClientEmailLinking([DataSourceRequest] DataSourceRequest request, int clientId)
        {
            var clientEmailList = DataClientEmailLinking.LoadClientEmailLinking(clientId);

            return Extended.JsonMax(clientEmailList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_ClientEmailLinking([DataSourceRequest] DataSourceRequest request, SL_ClientEmailLinking clientEmailLinking)
        {
            try
            {
                clientEmailLinking.EmailAddress = String.IsNullOrWhiteSpace(clientEmailLinking.EmailAddress) ? "" : clientEmailLinking.EmailAddress;
                clientEmailLinking.Source = String.IsNullOrWhiteSpace(clientEmailLinking.Source) ? "" : clientEmailLinking.Source;

                DataClientEmailLinking.AddClientEmailLinking(clientEmailLinking);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { clientEmailLinking }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ClientEmailLinking([DataSourceRequest] DataSourceRequest request, SL_ClientEmailLinking clientEmailLinking)
        {
            try
            {
                var m_ClientEmailLinking = DataClientEmailLinking.LoadClientEmailLinkingByPk(clientEmailLinking.SLClientEmailLinking);

                m_ClientEmailLinking.EmailAddress = String.IsNullOrWhiteSpace(clientEmailLinking.EmailAddress) ? "" : clientEmailLinking.EmailAddress;
                m_ClientEmailLinking.Source = String.IsNullOrWhiteSpace(clientEmailLinking.Source) ? "" : clientEmailLinking.Source;

                m_ClientEmailLinking.IsActive = clientEmailLinking.IsActive;
                
                DataClientEmailLinking.UpdateClientEmailLinking(m_ClientEmailLinking);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { clientEmailLinking }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult ReadClientEmailLinking(int clientEmailId)
        {
            return PartialView("~/Areas/Locates/Views/ClientEmail/_ClientEmailLinking.cshtml", clientEmailId);
        }

        [HttpPost]
        public JsonResult Read_LocateClientByPk(int value)
        {
            var clientEmailList = DataLocate.LoadLocateClient(Convert.ToDecimal(value));

            return Extended.JsonMax(clientEmailList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ClientEmail([DataSourceRequest] DataSourceRequest request, SL_ClientEmail clientEmail)
        {
            try
            {
                var localClientEmail = DataNotification.LoadClientEmailByPk(clientEmail.SLClientEmail);

                localClientEmail.Source = clientEmail.Source;
                localClientEmail.EmailAddress = clientEmail.EmailAddress;
                localClientEmail.AllowNeed = clientEmail.AllowNeed;
                localClientEmail.AllowInventory = clientEmail.AllowInventory;
                localClientEmail.AllowLocate = clientEmail.AllowLocate;
                localClientEmail.ClientId = clientEmail.ClientId;
                localClientEmail.AllowInventoryPrevDay = clientEmail.AllowInventoryPrevDay;
                localClientEmail.IsActive = clientEmail.IsActive;
                localClientEmail.UseLinkingOnly = clientEmail.UseLinkingOnly;
                localClientEmail.EmailAddress = String.IsNullOrWhiteSpace(clientEmail.EmailAddress) ? "" : clientEmail.EmailAddress;
                localClientEmail.Source = String.IsNullOrWhiteSpace(clientEmail.Source) ? "" : clientEmail.Source;

                DataNotification.UpdateClientEmail(localClientEmail);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { clientEmail }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create_ClientEmail([DataSourceRequest] DataSourceRequest request, SL_ClientEmail clientEmail)
        {
            try
            {
                clientEmail.SLFileDownloadConfigId = decimal.Parse(DataSystemValues.LoadSystemValue("ClientEmailDefaultConfig", "35"));

                clientEmail.EmailAddress = String.IsNullOrWhiteSpace(clientEmail.EmailAddress) ? "" : clientEmail.EmailAddress;
                clientEmail.Source = String.IsNullOrWhiteSpace(clientEmail.Source) ? "" : clientEmail.Source;
                clientEmail.ClientId = -1;

                DataNotification.AddClientEmail(clientEmail);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { clientEmail }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Create_LocateClientUser([DataSourceRequest] DataSourceRequest request, SL_LocateClientUserProjection m_LocateClientUser)
        {
            try
            {
                SL_LocateClient_User client = new SL_LocateClient_User();

                client.LocateClientId = m_LocateClientUser.LocateClientId;
                client.UserId = m_LocateClientUser.UserId;
                client.ViewClient = m_LocateClientUser.ViewClient;
                client.EditClient = m_LocateClientUser.EditClient;

                DataLocate.AddLocateClientUser(client);

                m_LocateClientUser =  DataLocate.LoadLocateClientUserByPk(client.SLLOCATECLIENTUSER);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { m_LocateClientUser }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
  
        public ActionResult Load_LocateClientUser([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_LocateClientUserProjection> locateClientList = DataLocate.LoadLocateClientUserByEntity();

            return Extended.JsonMax(locateClientList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadLocateList(SL_LocateClient client)
        {
            return PartialView("~/Areas/Locates/Views/Client/_ClientUsers.cshtml", client);
        }
    }
}
