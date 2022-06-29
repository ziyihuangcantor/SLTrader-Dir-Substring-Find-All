using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using SLTrader.Tools;
using System.Xml.Serialization;
using BondFire.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using BFLogic.ParseLogic;
using System.Configuration;
using System.Globalization;
using System.IO;
using ExcelDataReader;
using SLTrader.Custom;
using SLTrader.Helpers.ExportHelper;
using SLTrader.Helpers.EmailHelper;
using BondFire.FileImportProcesses.Utility;

namespace SLTrader.Areas.Admin.Controllers
{
    public class AdminController : BaseController
    {
        public static HttpPostedFileBase DictionaryFile;

        public int updateCount = 0;
        public int addCount = 0;


        public ActionResult Index()
        {
            return View();
        }

        public JsonResult UpdateUserPassword( int userId, string oldPassword, string newPassword )
        {
            bool success = false;

            try
            {
                DataUser.UpdateUserPassword( userId, oldPassword, newPassword );

                var user = DataUser.LoadUser(userId);

                string urlShortCut = ConfigurationManager.AppSettings["UrlShortCut"].ToString(CultureInfo.InvariantCulture);

                string body = "URL : " + urlShortCut + "\r\nUser ID : " + user.UserName + "\r\nNew Password : " + newPassword;

                EmailMessageHelper.SendNotification(ConfigurationManager.AppSettings["EWSServer"].ToString(CultureInfo.InvariantCulture),
                                                    ConfigurationManager.AppSettings["EWSServerUserId"].ToString(CultureInfo.InvariantCulture),
                                                    ConfigurationManager.AppSettings["EWSServerPassword"].ToString(CultureInfo.InvariantCulture),
                                                    ConfigurationManager.AppSettings["EWSServerDomain"].ToString(CultureInfo.InvariantCulture),
                                                      ConfigurationManager.AppSettings["EWSServerSender"].ToString(CultureInfo.InvariantCulture),
                                                    user.EMail, user.EMail, "HelixSL Login - Updated Password", body);
                success = true;
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( success, JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadUsers( [DataSourceRequest] DataSourceRequest request )
        {
            var userList = DataUser.LoadUsers();

            return Extended.JsonMax( userList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadUserError( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, int userId )
        {
            var userErrorList = DataError.LoadUserErrors( effectiveDate, userId );

            return Extended.JsonMax( userErrorList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult UpdateUserPasswordByUserId( int userId )
        {
            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_UserChangePassword.cshtml", userId );
        }

        public ActionResult UploadLoadDictionaryUplpad()
        {
            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_DictionaryUpload.cshtml" );
        }

        public ActionResult LoadUserByUserId( int userId )
        {
            var user = DataUser.LoadUser( userId );

            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_User.cshtml", user );
        }

        public ActionResult LoadUserByNewUser( int companyId )
        {

            var user = new User { UserId = -1, CompanyId = companyId };

            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_User.cshtml", user );
        }

        public ActionResult LoadParentCompany( [DataSourceRequest] DataSourceRequest request )
        {
            var companyList = DataUser.LoadHoldingCompanies();

            return Extended.JsonMax( companyList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult LoadParentCompanyDropdown( [DataSourceRequest] DataSourceRequest request )
        {
            var companyList = DataUser.LoadHoldingCompanies();

            return Extended.JsonMax( companyList, JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadCompany( [DataSourceRequest] DataSourceRequest request, int parentCompany )
        {
            var companyList = DataUser.LoadCompanies( parentCompany );

            return Extended.JsonMax( companyList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult LoadUsersByCompanyIdDropdown( [DataSourceRequest] DataSourceRequest request, int companyId )
        {
            var userList = DataUser.LoadUsers( companyId );

            return Extended.JsonMax( userList, JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadUsersByCompanyId( [DataSourceRequest] DataSourceRequest request, int companyId )
        {
            var userList = DataUser.LoadUsers( companyId );

            return Extended.JsonMax( userList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }


        public PartialViewResult RetrievePartial( string url )
        {
            if (url.Equals(@"/Areas/DomesticTrading/Views/Trade/_Scratchpad.cshtml"))
            {
                if (SessionService.UserPreference.SLTradeLayoutTypeId == SL_TradeLayoutType.BREAKOUT)
                {
                    url = "/Areas/DomesticTrading/Views/Trade/_ScratchpadBreakOut.cshtml";
                }
            }

            return PartialView( url );
        }

        public JsonResult UpdateUser( User user )
        {
            string password = "";

            try
            {
                password = user.PasswordHash;

                DataUser.AddUser( user );
                
                string urlShortCut = ConfigurationManager.AppSettings["UrlShortCut"].ToString(CultureInfo.InvariantCulture);

                string body = "URL : " + urlShortCut + "\r\nUser ID : " + user.UserName + "\r\nPassword : " + password;

                EmailMessageHelper.SendNotification(ConfigurationManager.AppSettings["EWSServer"].ToString(CultureInfo.InvariantCulture),
                                                    ConfigurationManager.AppSettings["EWSServerUserId"].ToString(CultureInfo.InvariantCulture),
                                                    ConfigurationManager.AppSettings["EWSServerPassword"].ToString(CultureInfo.InvariantCulture),
                                                    ConfigurationManager.AppSettings["EWSServerDomain"].ToString(CultureInfo.InvariantCulture),
                                                      ConfigurationManager.AppSettings["EWSServerSender"].ToString(CultureInfo.InvariantCulture),
                                                    user.EMail, user.EMail, "HelixSL Login", body);
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( true, JsonRequestBehavior.AllowGet );
        }

        public JsonResult UpdateCompany( Company company )
        {
            try
            {
                DataUser.AddCompany( company );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Json( true );
        }

        public ActionResult LoadCompanyByCompanyId( int companyId, int? parentCompanyId )
        {
            var company = DataUser.LoadCompany( companyId, parentCompanyId );

            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_Company.cshtml", company );
        }

        public ActionResult LoadCompanyByNewCompany( int? parentCompanyId )
        {
            var company = new Company { CompanyId = -1, ParentCompany = parentCompanyId };

            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_Company.cshtml", company );
        }

        public ActionResult LoadUserRoleType( [DataSourceRequest] DataSourceRequest request )
        {
            var userRoleList = DataUser.LoadUserRoleType();

            return Extended.JsonMax( userRoleList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadUserRoleTypeByUserId( int userId )
        {
            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_UserRole.cshtml", userId );
        }

        public ActionResult LoadManagerTasksByRoleId( int roleId )
        {
            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_ManagerTasks.cshtml", roleId );
        }

        public ActionResult LoadUserPreference(int userId)
        {
            var userPreference = new SL_UserPreference();

            try
            {
                userPreference = DataUser.LoadUserPreference(userId);
            }
            catch
            {
                userPreference.Application = SL_Application.DomesticTrading;
                userPreference.Customcss = "BlueOpal";
                userPreference.DefaultFirm = "";
                userPreference.FontSize = 10;
                userPreference.FontFamily = "Verdana";
                userPreference.ShowActivity = true;
                userPreference.ShowMatchingIssues = false;
                userPreference.ShowOnStartUp = false;
                userPreference.ShowSecMaster = true;
                userPreference.ShowInventoryMarker = false;
                userPreference.SLSecurityLayoutTypeId = SL_SecurityLayoutType.SIDE;
                userPreference.SLTradeLayoutTypeId = SL_TradeLayoutType.STANDARD;
                userPreference.UserTypeId = SL_UserType.DOMESTIC;
                userPreference.UserId = userId;
                userPreference.SLUserPreference = -1;
            }

            return PartialView("~/Areas/Admin/Views/Admin/_UserPreferences.cshtml", userPreference);
        }

        public ActionResult LoadUserRoleTypes( [DataSourceRequest] DataSourceRequest request, int userId )
        {
            var userRoleList = DataUser.LoadUserRoleTypeByUserId( userId );

            return Extended.JsonMax( userRoleList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public JsonResult RemoveRoleFromUser( int userId, int roleId )
        {
            const bool success = true;

            try
            {
                DataUser.DeleteUserRoleByUserId( userId, roleId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( success, JsonRequestBehavior.AllowGet );
        }

        public JsonResult AddTaskToUserRoleType( int userRoleTypeId, int managerTask )
        {
            const bool success = true;

            var managerList = new List<ManagerTask> { (ManagerTask)managerTask };

            try
            {
                DataUser.AddTaskToUserRoleType( userRoleTypeId, managerList );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( success, JsonRequestBehavior.AllowGet );
        }

        public JsonResult AddRoleToUser( int userId, int roleId )
        {
            var success = DataUser.AddUserRoleByUserId( userId, roleId );

            return Extended.JsonMax( success, JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadAvailableRolesByUserId( [DataSourceRequest] DataSourceRequest request, int userId )
        {
            var tempList = new List<UserRoleType>();
            var userRoleList = DataUser.LoadUserRoleTypeByUserId( userId );
            var roleList = DataUser.LoadUserRoleType();

            foreach ( var roleItem in roleList )
            {
                var found = false;

                var item = roleItem;

                foreach ( var userRoleItem in userRoleList.Where( userRoleItem => item.UserRoleTypeId == userRoleItem.UserRoleTypeId ) )
                {
                    found = true;
                }

                if ( !found )
                {
                    tempList.Add( roleItem );
                }
            }

            return Extended.JsonMax( tempList, JsonRequestBehavior.AllowGet );
        }

        public JsonResult RemoveUser( int userId )
        {
            const bool success = true;

            try
            {
                DataUser.DeactivateUserByUserId( userId );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( success, JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadManagerTaskByRoleId( [DataSourceRequest] DataSourceRequest request, int roleId )
        {
            var managerList = DataUser.LoadManagerTasks( roleId );

            return Extended.JsonMax( managerList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }


        public ActionResult LoadExecutingSystemTypeTradeType([DataSourceRequest] DataSourceRequest request)
        {
            var managerList = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeType();

            return Extended.JsonMax(managerList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateExecutingSystemTypeTradeType([DataSourceRequest] DataSourceRequest request, SL_ExecutingSystemTypeTradeType dataObject)
        {
            try
            {
                DataExecutingSystemTypeTradeType.AddExecutingSystemTypeTradeType(dataObject);
            }
            catch (Exception e)
            {
                ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { dataObject }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }



        public JsonResult UpdateExecutingSystemTypeTradeType(SL_ExecutingSystemTypeTradeType dataObject)
        {
            try
            {
                var item = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeTypeByPk(dataObject.SLExecutingSystemTypeTradeType);

                item.ExecutionSystemType = dataObject.ExecutionSystemType;
                item.TradeType = dataObject.TradeType;
                item.DefaultCashSettleLocation = dataObject.DefaultCashSettleLocation;
                item.DefaultSecuritySettleLocation = dataObject.DefaultSecuritySettleLocation;
                item.DefaultCurrency = dataObject.DefaultCurrency;

                DataExecutingSystemTypeTradeType.UpdateExecutingSystemTypeTradeType(item);
            }
            catch (Exception e)
            {
                ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { dataObject }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DestroyExecutingSystemTypeTradeType(SL_ExecutingSystemTypeTradeType dataObject)
        {
            try
            {
                var item = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeTypeByPk(dataObject.SLExecutingSystemTypeTradeType);

                DataExecutingSystemTypeTradeType.DeleteExecutingSystemTypeTradeType(item);
            }
            catch (Exception e)
            {
                ThrowJsonError(e);
            }

            return Extended.JsonMax(dataObject, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadUserRoleTypeAdd()
        {
            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_UserRoleType.cshtml" );
        }

        public JsonResult AddUserRoleType( UserRoleType item )
        {
            var roleList = DataUser.LoadUserRoleType();
            var max = ( roleList.Last() ).UserRoleTypeId + 1;
            item.UserRoleTypeId = max;
            const bool success = true;

            try
            {
                DataUser.AddUserRoleType( item );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( success, JsonRequestBehavior.AllowGet );
        }

        public JsonResult RemoveRole(int userRoleTypeId)
        {
          
            const bool success = true;

            try
            {
                DataUser.DeleteUserRoleType(userRoleTypeId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(success, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDictionary( [DataSourceRequest] DataSourceRequest request )
        {
            var mDataObject = DataDictionary.LoadDictionary();

            return Extended.JsonMax( mDataObject.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult UpdateDictionary( [DataSourceRequest] DataSourceRequest request, SL_Dictionary dataObject )
        {
            var mDataObject = DataDictionary.UpdateDictionaryItem( dataObject );

            return Extended.JsonMax( new[] { mDataObject }, JsonRequestBehavior.AllowGet );
        }

        public ActionResult LoadHolidayByNewHoliday()
        {
            var holiday = new Holiday { Country = DataHoliday.RetrieveCountry(), };

            return PartialView( "~/Areas/Admin/Views/Shared/Templates/_Holiday.cshtml", holiday );
        }

        public JsonResult AddHoliday( Holiday holiday )
        {
            try
            {
                ///WTH?
                holiday.Country = DataHoliday.RetrieveCountry();
                DataHoliday.AddHoliday( holiday );
            }
            catch ( Exception e )
            {
                return ThrowJsonError( e );
            }

            return Extended.JsonMax( true, JsonRequestBehavior.AllowGet );
        }


        public ActionResult SaveDictionaryFile( IEnumerable<HttpPostedFileBase> DictionaryFiles )
        {
            DictionaryFile = DictionaryFiles.Single();

            return Content( "" );
        }

        public JsonResult DictionaryFileProcess()
        {
            updateCount = 0;
            addCount = 0;

            try
            {
                if ( DictionaryFile != null )
                {
                    if ( DictionaryFile.FileName.ToLower().Contains( ".xls" ) )
                    {
                        List<SL_Dictionary> dictionaryItems = DataDictionary.LoadDictionary();

                        List<SL_Dictionary> processedDictionaryItems = ParseExcelAttachment();

                        foreach ( var newItem in processedDictionaryItems )
                        {
                            if ( dictionaryItems.Any( x => x.PropertyName.ToLower().Equals( newItem.PropertyName.ToLower() ) ) )
                            {
                                var changeItem = dictionaryItems.Where( x => x.PropertyName.ToLower().Equals( newItem.PropertyName.ToLower() ) ).First();

                                changeItem.DisplayName = newItem.DisplayName;
                                changeItem.ColumnWidth = newItem.ColumnWidth;


                                DataDictionary.UpdateDictionaryItem( changeItem );
                                updateCount++;
                            }
                            else
                            {
                                DataDictionary.AddDictionaryItemObject( newItem );
                                addCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                return ThrowJsonError(error);
            }

            return Extended.JsonMax( new { message = string.Format( "Updated {0} items, Added {1} items to Dictionary", updateCount, addCount ) }, JsonRequestBehavior.AllowGet );
        }


        private List<SL_Dictionary> ParseExcelAttachment()
        {
            ExcelData excelData = new ExcelData( DictionaryFile.FileName );
            List<SL_Dictionary> _dictionaryList = new List<SL_Dictionary>();
            IEnumerable<DataRow> dataRows;

            try
            {
                dataRows = excelData.GetDataFromMemory( DictionaryFile.InputStream.ToByteArray(), "");
            }
            catch
            {
                throw new Exception( "Not valid excel data, trying txt file." );
            }

            foreach ( var dataRow in dataRows )
            {
                int index = 0;
                SL_Dictionary _dictionary = new SL_Dictionary();

                foreach ( var column in dataRow.ItemArray )
                {
                    if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "displayname") )
                    {
                        if (! String.IsNullOrWhiteSpace( column.ToString() ) )
                        {
                            _dictionary.DisplayName = column.ToString();
                        }
                        else
                        {
                            _dictionary.DisplayName = null;
                        }
                    }


                    if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "columnwidth") )
                    {
                        if ( column.ToString().Length > 0 )
                        {
                            _dictionary.ColumnWidth = Int32.Parse( column.ToString() );
                        }
                    }

                    if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "propertyname") )
                    {
                        _dictionary.PropertyName = column.ToString();
                    }

                    index++;
                }

                _dictionaryList.Add( _dictionary );
            }

            return _dictionaryList;
        }


        public ActionResult ExportDictionaryToExcel()
        {
            var dataDictionary = DataDictionary.LoadDictionary();

            var fName = string.Format( "dictionary-{0}.xls", DateTime.Now.ToString( "yyyy-MM-dd hh:mm:ss" ) );

            return File( ExcelHelper.ExportExcel( dataDictionary, "dictionary" ), "application/vnd.ms-excel", fName );
        }
    }



    static class Extensions
    {
        public static string GetColumn( this DataRow Row, int Ordinal )
        {
            return Row.Table.Columns[ Ordinal ].ColumnName;
        }

        public static byte[] ToByteArray( this Stream stream )
        {
            stream.Position = 0;
            byte[] buffer = new byte[ stream.Length ];
            for ( int totalBytesCopied = 0; totalBytesCopied < stream.Length; )
                totalBytesCopied += stream.Read( buffer, totalBytesCopied, Convert.ToInt32( stream.Length ) - totalBytesCopied );
            return buffer;
        }
    }
}
