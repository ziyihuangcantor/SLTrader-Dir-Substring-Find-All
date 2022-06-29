using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ContactController : BaseController
    {
        public ActionResult Index()
        {
           
            return View();
        }

        public JsonResult Read_Contacts([DataSourceRequest] DataSourceRequest request, string contraEntityId)
        {
            List<SL_Contact> contactList;
           
            try
            {
                contactList = DataContact.LoadContacts(contraEntityId);
            }
            catch
            {
                contactList = new List<SL_Contact>();
            }

            return Json(contactList.ToDataSourceResult(request));
        }

        public JsonResult Add_Contact(SL_Contact item)
        {            
            try
            {
                DataContact.AddContact(item);
            }
            catch(Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete_Contact(SL_Contact item)
        {
            try
            {
                var contactItem = DataContact.LoadContactByPK((int)item.SLContact);
                DataContact.DeleteContact(contactItem);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { item }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Update_Contact(SL_Contact item)
        {
            SL_Contact contactItem = new SL_Contact();

            try
            {
                contactItem = DataContact.LoadContactByPK((int)item.SLContact);

                contactItem.SLContactType = item.SLContactType;
                contactItem.FirstName = string.IsNullOrWhiteSpace(item.FirstName) ? "" : item.FirstName;
                contactItem.LastName = string.IsNullOrWhiteSpace(item.LastName) ? "" : item.LastName;
                contactItem.Phone = string.IsNullOrWhiteSpace(item.Phone) ? "" : item.Phone;
                contactItem.Comment = item.Comment;
                contactItem.EMail = item.EMail;
                contactItem.Title = item.Title;

                DataContact.UpdateContact(contactItem);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { contactItem }, JsonRequestBehavior.AllowGet);
        }
    }
}
