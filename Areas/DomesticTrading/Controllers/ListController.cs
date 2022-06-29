using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.IO;
using ExcelDataReader;
using SLTrader.Custom;
using SLTrader.Tools;
using BFLogic.ParseLogic;
using SLTrader.Helpers.SessionHelper;
using SLTrader.ExcelDataHelper;

namespace SLTrader.Areas.DomesticTrading.Controllers
{

    public class ListController : BaseController
    {
        public static HttpPostedFileBase RegulatoryFile;
        public static string entityId;
        public static SLRegulatoryType regType;
        public int updateCount = 0;
        public int addCount = 0;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read_RegulatoryList( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, SLRegulatoryType regList )
        {
            var regulatoryList = new List<SL_RegulatoryListProjection>();

            if ( entityId == null ) return Json( regulatoryList.ToDataSourceResult( request ) );
            try
            {
                regulatoryList = DataRegulatoryList.LoadRegulatoryList( effectiveDate, entityId, regList );
            }
            catch
            {
                regulatoryList = new List<SL_RegulatoryListProjection>();
            }

            return Extended.JsonMax( regulatoryList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }


        public void AddRegulatoryList( SLRegulatoryType regType, string entityId, int issueId )
        {
            try
            {
                var isOneDay = bool.Parse(DataSystemValues.LoadSystemValue("RegListUseOnceDaily", "false"));



                var regulatoryItem = new SL_RegulatoryList()
                {
                    EffectiveDate = DateTime.Today,
                    EntityId = entityId,
                    IssueId = issueId,
                    Quantity = 0,
                    StartDate = DateTime.Today,
                    StopDate = null,
                    RegulatoryType = regType,
                    Source = SessionService.User.UserName
                };

                if (isOneDay)
                {
                    regulatoryItem.StopDate = DateTime.Today;
                }

                DataRegulatoryList.AddRegulatoryList(regulatoryItem);

                StaticDataCache.SecurityPanelUpdateIssue(entityId, issueId, regType, true);
            }
            catch
            {

            }
        }

        public void DeleteRegulatoryList( SLRegulatoryType regType, string entityId, int issueId )
        {
            var regulatoryList = new List<SL_RegulatoryListProjection>();

            try
            {
                regulatoryList = DataRegulatoryList.LoadRegulatoryListByIssue(DateTime.Today, entityId, issueId.ToString());

                var regItem = regulatoryList.Where(x => x.RegulatoryType == regType).First();

                DataRegulatoryList.DeleteRegulatoryList(DataRegulatoryList.LoadRegulatoryListByPk(regItem.SLRegulatoryList));

                StaticDataCache.SecurityPanelUpdateIssue(entityId, issueId, regType, false);
            }
            catch
            {
                regulatoryList = new List<SL_RegulatoryListProjection>();
            }
        }

        public void DeleteRegulatoryProjectionList(IEnumerable<SL_RegulatoryListProjection> list)
        {
            var regulatoryList = new List<SL_RegulatoryListProjection>();

            try
            {
                foreach (var item in list)
                {

                    DataRegulatoryList.DeleteRegulatoryList(DataRegulatoryList.LoadRegulatoryListByPk(item.SLRegulatoryList));

                    StaticDataCache.SecurityPanelUpdateIssue(entityId, (int)item.IssueId, item.RegulatoryType, false);
                }
            }
            catch
            {
                regulatoryList = new List<SL_RegulatoryListProjection>();
            }
        }


        public PartialViewResult LoadDeleteRegulatoryList(IEnumerable<SL_RegulatoryListProjection> list)
        {
            return PartialView("~/Areas/DomesticTrading/Views/List/_ListRegDeletePartial.cshtml", list);            
        }

        private List<SL_RegulatoryList> ParseExcelAttachment()
        {
            ExcelData excelData = new ExcelData( RegulatoryFile.FileName );
            IEnumerable<DataRow> dataRows;
            List<SL_RegulatoryList> _regulatoryList = new List<SL_RegulatoryList>();

            try
            {
                dataRows = excelData.GetDataFromMemory( RegulatoryFile.InputStream.ToByteArray(), "", true );
            }
            catch
            {
                throw new Exception( "Not valid excel data, trying txt file." );
            }

            foreach ( var dataRow in dataRows )
            {
                int index = 0;
                SL_RegulatoryList _regItem = new SL_RegulatoryList();

                foreach ( var column in dataRow.ItemArray )
                {
                    if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "cusip" ) )
                    {
                        if ( !String.IsNullOrWhiteSpace( column.ToString() ) )
                        {
                            try
                            {
                                _regItem.EffectiveDate = DateTime.Today;
                                _regItem.EntityId = entityId;
                                _regItem.IssueId = DataIssue.LoadIssue( column.ToString() ).IssueId;
                                _regItem.Quantity = 0;
                                _regItem.StartDate = DateTime.Today;
                                _regItem.StopDate = DateTime.Today;
                                _regItem.RegulatoryType = regType;
                                _regItem.Source = SessionService.User.UserName;
                            }
                            catch
                            {
                                _regItem = null;
                            }
                        }
                    }


                    if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "quantity" ) )
                    {
                        if ( ( column.ToString().Length > 0 ) && ( _regItem != null ) )
                        {
                            _regItem.Quantity = Decimal.Parse( column.ToString() );
                        }
                    }

                    index++;
                }
                if ( _regItem != null )
                {
                    _regulatoryList.Add( _regItem );
                }
            }

            return _regulatoryList;
        }

        public JsonResult RegulatoryListFileProcess()
        {
            updateCount = 0;
            addCount = 0;

            if ( RegulatoryFile != null )
            {
                if ( RegulatoryFile.FileName.ToLower().Contains( ".xls" ) )
                {
                    List<SL_RegulatoryListProjection> regulatoryItems = DataRegulatoryList.LoadRegulatoryList( DateTime.Today, entityId, regType );

                    List<SL_RegulatoryList> processedRegulatoryItems = ParseExcelAttachment( );

                    foreach ( var newItem in processedRegulatoryItems )
                    {
                        if ( regulatoryItems.Any( x => x.IssueId == newItem.IssueId ) )
                        {
                            var changeItem = regulatoryItems.Where( x => x.IssueId == newItem.IssueId ).First();

                            updateCount++;
                        }
                        else
                        {
                            try
                            {
                                DataRegulatoryList.AddRegulatoryList( newItem );
                                addCount++;
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }

            return Extended.JsonMax( new { message = string.Format( "Ignored {0} dup'd items, Added {1} items to Regulatory List", updateCount, addCount ) }, JsonRequestBehavior.AllowGet );
        }

        public ActionResult SaveRegulatoryListFile( IEnumerable<HttpPostedFileBase> RegulatoryListFiles )
        {
            RegulatoryFile = RegulatoryListFiles.Single();

            return Content( "" );
        }

        public ActionResult UploadRegulatoryListUpload( string _entityId, SLRegulatoryType _regType )
        {
            entityId = _entityId;
            regType = _regType;

            return PartialView( "~/Areas/DomesticTrading/Views/Shared/Templates/_RegulatoryListUpload.cshtml" );
        }

        public ActionResult Read_FundingRates( [DataSourceRequest] DataSourceRequest request, string entityId, int feeId )
        {
            var fundingRateList = new List<SL_FundingRate>();

            if ( entityId == null ) return Extended.JsonMax( fundingRateList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );

            try
            {
                fundingRateList = DataFundingRates.LoadFundingRatesByFeeId( entityId, feeId );
            }
            catch
            {
                fundingRateList = new List<SL_FundingRate>();
            }

            return Extended.JsonMax( fundingRateList.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }


        public JsonResult Update_FundingRate([DataSourceRequest] DataSourceRequest request, SL_FundingRate item)
        {
            var fundingRateItem = new SL_FundingRate();

            try
            {
                fundingRateItem = DataFundingRates.LoadFundingRateByPK(item.SLFundingRate);

                fundingRateItem.Fund = item.Fund;

                DataFundingRates.UpdateFundingRate(fundingRateItem);
            }
            catch (Exception error)
            {
                ThrowJsonError(error);
            }

            return Json(new[] { fundingRateItem }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult Create_FundingRate([DataSourceRequest] DataSourceRequest request, SL_FundingRate item)
        {
            try
            {
                DataFundingRates.AddFundingRate(item);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        public JsonResult Delete_FundingRate(SL_FundingRate item)
        {
            try
            {
                SL_FundingRate _fundingRate = DataFundingRates.LoadFundingRateByPK(item.SLFundingRate);

                DataFundingRates.DeleteFundingRate(_fundingRate);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }         

            return Json(item, JsonRequestBehavior.AllowGet);
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
