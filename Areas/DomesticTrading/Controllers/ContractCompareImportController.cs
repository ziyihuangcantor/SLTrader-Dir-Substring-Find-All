using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using BFLogic.ParseLogic;
using BondFire.Core.Dates;
using BondFire.Entities;
using BondFire.Loanet.Messages;
using BondFire.Loanet.Messages.Parsers;
using SLTrader.Models.ContractRelatedModels;
using SLTrader.Custom;
using SLTrader.ExcelDataHelper;
using SLTrader.Tools;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ContractCompareImportController : BaseController
    {
        public static List<ContractCompareImportModel> CCImportData = new List<ContractCompareImportModel>();

        public ActionResult Index()
        {
            return View( CCImportData );
        }

        #region File processing
        public ActionResult ContractCompareImport_Init( [DataSourceRequest] DataSourceRequest request )
        {
            CCImportData.Clear();

            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
        public ActionResult Process_ContractCompareImportFiles( [DataSourceRequest] DataSourceRequest request, IEnumerable<HttpPostedFileBase> ContractCompareImportFiles )
        {
            // The Name of the Upload component is "files"
            if ( ContractCompareImportFiles != null )
            {
                int modelId = ((CCImportData.Count > 0) ? CCImportData.Max( o => o.Id ) : 0) + 1;

                foreach ( var file in ContractCompareImportFiles )
                {
                    try
                    {
                        CCImportData.AddRange( ProcessImportFile( file ) );

                        var physicalPath = ConfigurationManager.AppSettings[ "ContractImportPath" ].ToString( CultureInfo.InvariantCulture );
                        if ( !string.IsNullOrWhiteSpace( physicalPath ) )
                        {
                            if ( !Directory.Exists( physicalPath ) )
                            {
                                Directory.CreateDirectory( physicalPath );
                            }

                            // Some browsers send file names with full path.
                            // We are only interested in the file name.
                            var fileName = Path.GetFileNameWithoutExtension( file.FileName );
                            var fileExtension = Path.GetExtension( file.FileName );
                            var labeledFileName = $"{fileName}_{SessionService.SecurityContext.UserName}_{DateTime.Now.Ticks}.{fileExtension}";
                            //var physicalPath = Path.Combine( Server.MapPath( "~/App_Data" ), fileName );
                            file.SaveAs( Path.Combine( physicalPath, labeledFileName ) );
                        }
                    }
                    catch ( Exception ex )
                    {
                        CCImportData.Add( new ContractCompareImportModel()
                        {
                            Status = StatusMain.Error,
                            StatusMessage = $"Could not process {file}, Message = {ex.Message}",
                        } );
                    }
                }
            }

            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Remove_ContractCompareImportFiles( [DataSourceRequest] DataSourceRequest request, string[] fileNames )
        {
            // The Name of the Upload component is "files"
            if ( fileNames != null )
            {
                foreach ( var file in fileNames )
                {
                    FileInfo fi = new FileInfo( file );
                    CCImportData.RemoveAll( o => o.SourceFile == fi.Name );
                }
            }

            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult ContractCompareImport_Complete( [DataSourceRequest] DataSourceRequest request )
        {
            //Label ids for import data
            for ( int ii = 0; ii < CCImportData.Count; ii++ )
            {
                CCImportData[ ii ].Id = ii;
            }

            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
        #endregion File processing

        #region Contract Compare Processing
        public ActionResult ContractCompareImport_Action( [DataSourceRequest] DataSourceRequest request, ContractCompareImportModel model )
        {
            var loadedModels = CCImportData.Where( o => o.Id == model.Id );

            if ( loadedModels.Count() == 0 )
            {
                throw new Exception( $"Unable to find selected Import Model - {model.Id} {model.EffectiveDate} {model.EntityLoanetId} {model.ContraEntity} {model.TradeType} {model.ContractAmount}" );
            }
            else if ( loadedModels.Count() > 1 )
            {
                foreach ( var loadedModel in loadedModels )
                {
                    loadedModel.Status = StatusMain.Error;
                    loadedModel.StatusMessage = "Duplicate Model Ids encounetered, please reload data";
                }
            }
            else
            {
                var loadedModel = loadedModels.Single();

                //Guarded out by UI
                if ( loadedModel.Status != StatusMain.Ready )
                {
                    throw new Exception( $"Import Model not in Correct State - {loadedModel.Status} {model.Id} {model.EffectiveDate} {model.EntityLoanetId} {model.ContraEntity} {model.TradeType} {model.ContractAmount}" );
                }

                try
                {
                    loadedModel.StatusMessage = string.Empty;
                    loadedModel.Status = StatusMain.InProgress;
                    loadedModel.ResultActivities = PerformAction( loadedModel );

                    if ( loadedModel.ResultActivities.Count > 0 )
                    {
                        loadedModel.StatusMessage = CreateStatusMessage( loadedModel.ResultActivities );

                        if ( DataContractCompareImport.CheckAnyActivitiesStatus( SL_ActivityFlag.Failed, loadedModel.ResultActivities ) )
                        {
                            throw new Exception( $"Error encountered processing Import Model - {model.Id} {model.EffectiveDate} {model.EntityLoanetId} {model.ContraEntity} {model.TradeType} {model.ContractAmount}" );
                        }
                    }
                    else
                    {
                        loadedModel.Status = StatusMain.Ready;
                        loadedModel.StatusMessage = "No Requests sent";
                    }
                }
                catch ( Exception ex )
                {
                    loadedModel.Status = StatusMain.Error;
                    loadedModel.StatusMessage += $"{ex.Message} : ";
                }
            }

            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult ContractCompareImport_SimpleRefresh( [DataSourceRequest] DataSourceRequest request )
        {
            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
        public ActionResult ContractCompareImport_Refresh( [DataSourceRequest] DataSourceRequest request )
        {
            UpdateModelActions( CCImportData );
            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }

        public ActionResult Read_ContractCompareImport( [DataSourceRequest] DataSourceRequest request )
        {
            return Extended.JsonMax( CCImportData.ToDataSourceResult( request ), JsonRequestBehavior.AllowGet );
        }
        #endregion Contract Compare Processing

        public List<SL_Activity> PerformAction( ContractCompareImportModel model )
        {
            return (model.UpdateType == ContractCompareImportUpdateType.New) ?
                DataContractCompareImport.CreateContractRequest( model ) :
                DataContractCompareImport.CreateContractUpdateRequest( model );
        }

        public IEnumerable<ContractCompareImportModel> ProcessImportFile( HttpPostedFileBase fileBase )
        {
            var fi = new FileInfo( fileBase.FileName );
            List<DataRow> dataSet;

            switch ( fi.Extension.ToUpper() )
            {
                case ".CSV":
                    var table = CsvData.GetDataTabletFromCSVFile( fileBase.InputStream.ToByteArray(), true );
                    dataSet = CsvData.GetRowsFromTable( table );
                    break;
                case ".XLSX":
                    var excelData = new ExcelData( fileBase.FileName );
                    dataSet = excelData.GetDataFromMemory( fileBase.InputStream.ToByteArray(), "Contracts", true ).ToList();
                    break;
                default:
                    throw new Exception( $"Invalid filename {fi.Extension} (only csv and xlsx allowed)" );
            }

            var models = new List<ContractCompareImportModel>();
            foreach ( var row in dataSet )
            {
                models.Add( ConvertToModel( row, fi.Name ) );
            }
            UpdateModelActions( models );

            return models;
        }

        public IEnumerable<ContractCompareImportModel> UpdateModelActions( IEnumerable<ContractCompareImportModel> models )
        {
            foreach ( var model in models.Where( o => (o.UpdateType != ContractCompareImportUpdateType.NotApplicable) ) )
            {
                try
                {
                    switch ( model.UpdateType )
                    {
                        case ContractCompareImportUpdateType.New:
                            if ( model.Status == StatusMain.Ready )
                            {
                                if ( string.IsNullOrWhiteSpace( model.ContractNumber ) )
                                {
                                    model.StatusMessage = "Ready to Create Contract";
                                }
                                else
                                {
                                    model.StatusMessage = ( model.Contract != null ) ?
                                            $"Ready to Re-Create Contract Request, previously created ContractId = {model.Contract.SLContract}" :
                                            $"Ready to Re-Create Contract Request, cannot find previously created Contract";
                                }
                            }
                            else if ( ( model.Status == StatusMain.InProgress ) || (model.Status == StatusMain.Error) )
                            {
                                RespondToProcessingNewContract( model );
                            }
                            break;
                        case ContractCompareImportUpdateType.Update:
                            var contracts = DataContractCompareImport.LoadContractList( model ).ToList();
                            if ( (contracts.Count == 0) || (contracts.Count > 1) )
                            {
                                model.Status = StatusMain.Error;
                                model.StatusMessage = $"Invalid number of Contracts found {contracts.Count}";
                            }
                            else
                            {
                                //Update Contract reference
                                model.Contract = contracts.Single();

                                if ( model.Status == StatusMain.Ready )
                                {
                                    if ( (model.Amount == model.Contract.Amount) &&
                                        (model.Quantity == model.Contract.Quantity) &&
                                        (model.RebateRate == model.Contract.RebateRate) &&
                                        (model.MarkParamterAmount == model.Contract.Mark) )
                                    {
                                        model.StatusMessage = "Fields Matched";
                                    }
                                    else
                                    {
                                        model.StatusMessage = "Ready to Update";
                                    }
                                }
                                else if ( model.Status == StatusMain.InProgress )
                                {
                                    RespondToProcessingUpdateContract( model );
                                }
                            }
                            break;
                    }
                }
                catch ( Exception ex )
                {
                    model.Status = StatusMain.Error;
                    model.StatusMessage += $"{ex.Message} : ";
                }
            }

            return models;
        }

        public static void RespondToProcessingUpdateContract( ContractCompareImportModel model )
        {
            var activityIds = model.ResultActivities.Select( o => o.SLActivity ).ToList();
            var loadedActivities = DataActivity.LoadActivitiesById( activityIds, SL_ExecutionSystemType.LOANET );
            string ids = loadedActivities.Count > 0 ? String.Join( " ", loadedActivities.Select( o => o.SLActivity.ToString() ).ToArray() ) : string.Empty;

            if ( DataContractCompareImport.CheckAllActivitiesStatus( SL_ActivityFlag.Completed, loadedActivities ) )
            {
                model.Status = StatusMain.Ready;
                model.StatusMessage = $"{CreateStatusMessage( loadedActivities )} : ";
                model.ResultActivities.Clear();
            }
            else if ( DataContractCompareImport.CheckAllActivitiesStatus( SL_ActivityFlag.Processing, loadedActivities ) )
            {
                model.Status = StatusMain.InProgress;
                model.StatusMessage = $"{CreateStatusMessage( loadedActivities )} : ";
            }
            else if ( DataContractCompareImport.CheckAnyActivitiesStatus( SL_ActivityFlag.Failed, loadedActivities ) )
            {
                model.Status = StatusMain.Error;
                model.StatusMessage = $"{CreateStatusMessage( loadedActivities )} : ";
            }
            else
            {
                model.Status = StatusMain.InProgress;
                model.StatusMessage = $"{CreateStatusMessage( loadedActivities )} : ";
            }
        }
        public static void RespondToProcessingNewContract( ContractCompareImportModel model )
        {
            var activityIds = model.ResultActivities.Select( o => o.SLActivity ).ToList();
            var loadedActivities = DataActivity.LoadActivitiesById( activityIds, SL_ExecutionSystemType.LOANET );
            string ids = loadedActivities.Count > 0 ? String.Join( " ", loadedActivities.Select( o => o.SLActivity.ToString() ).ToArray() ) : string.Empty;
            model.StatusMessage = string.Empty;

            if ( loadedActivities.Count == 1 )
            {
                SL_Activity activity = loadedActivities.Single();
                if ( !string.IsNullOrWhiteSpace( activity.ActivityResponse ) )
                {
                    LoanetMessage replyMessage = LoanetReplyMessageParser.Parse( activity.ActivityResponse );
                    IReplyMessage replyCodeMessage = replyMessage as IReplyMessage;

                    if ( DataContractCompareImport.CheckAllActivitiesStatus( SL_ActivityFlag.Completed, loadedActivities ) )
                    {
                        if ( replyCodeMessage.ReplyCode == LoanetReplyCode.Accepted )
                        {
                            model.Status = StatusMain.Settled;
                            model.ContractNumber = DataContractCompareImport.RetrieveLoanetContractNumber( replyMessage, replyCodeMessage.ReplyCode == LoanetReplyCode.Accepted );
                            model.StatusMessage = $"Completed/Created ContractNumber {model.ContractNumber} : ";

                            var contracts = DataContractCompareImport.LoadContractList( model ).ToList();

                            if ( contracts.Count == 0 )
                            {
                                string awaitCreationMessage = $"Still awaiting Contract Creation : ";
                                if ( !model.StatusMessage.Contains( awaitCreationMessage ) )
                                {
                                    model.StatusMessage += awaitCreationMessage;
                                }
                            }
                            else if ( contracts.Count > 1 )
                            {
                                model.Status = StatusMain.Error;
                                model.StatusMessage = $"Invalid number of Contracts found {contracts.Count}";
                            }
                            else
                            {
                                model.Contract = contracts.Single();
                            }
                        }
                        else
                        {
                            string error = DataContractCompareImport.RetrieveNackError( replyMessage );
                            model.Status = StatusMain.Error;
                            model.StatusMessage = $"Request rejected. ReplyId = {activity.SLActivity} Errors = {error}";

                            model.ResultActivities.Clear();
                        }
                    }
                    else if ( DataContractCompareImport.CheckAllActivitiesStatus( SL_ActivityFlag.Failed, loadedActivities ) )
                    {
                        string error = DataContractCompareImport.RetrieveNackError( replyMessage );
                        model.StatusMessage = $"Unable to Create Contract. Response Ids = {ids} Errors = {error}";
                        model.Status = StatusMain.Error;

                        model.ResultActivities.Clear();
                    }
                }
                else
                {
                    model.StatusMessage = $"Awaiting for response for RequestId = {activity.SLActivity}";
                }
            }
            else if ( loadedActivities.Count > 0 )
            {
                model.StatusMessage += $"Invalid Number of Requests {loadedActivities.Count} Ids = {ids}";
            }
        }

        public static ContractCompareImportModel ConvertToModel( DataRow dataRow, string file )
        {
            string statusMessage = string.Empty;
            var model = new ContractCompareImportModel()
            {
                SourceFile = file,
                EffectiveDate = DateCalculator.Default.Today,
                Status = StatusMain.Ready,
            };

            int index = 0;
            foreach ( var column in dataRow.ItemArray )
            {
                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "quantity" ) )
                {
                    string quantityValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( quantityValue ) )
                    {
                        try
                        {
                            model.Quantity = Decimal.Parse( quantityValue );
                        }
                        catch
                        {
                            statusMessage += $"Invalid Quantity {quantityValue} : ";
                            model.Status = StatusMain.Error;
                        }
                    }
                    else
                    {
                        statusMessage += $"Blank Quantity : ";
                        model.Status = StatusMain.Error;
                        break;
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "amount" ) )
                {
                    string amountValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( amountValue ) )
                    {
                        try
                        {
                            model.Amount = Decimal.Parse( amountValue );
                        }
                        catch
                        {
                            statusMessage += $"Invalid Amount {amountValue} : ";
                            model.Status = StatusMain.Error;
                            break;
                        }
                    }
                    else
                    {
                        statusMessage += $"Blank Amount : ";
                        model.Status = StatusMain.Error;
                        break;
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "rate" ) )
                {
                    string rateValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( rateValue ) )
                    {
                        try
                        {
                            model.RebateRate = Double.Parse( rateValue );
                        }
                        catch
                        {
                            statusMessage += $"Invalid Rate {rateValue} : ";
                            model.Status = StatusMain.Error;
                            break;
                        }
                    }
                    else
                    {
                        statusMessage += $"Blank Rate : ";
                        model.Status = StatusMain.Error;
                        break;
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "mark" ) )
                {
                    string markValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( markValue ) && (column.ToString().Length > 0) )
                    {
                        try
                        {
                            model.MarkParamterAmount = double.Parse( markValue );
                        }
                        catch
                        {
                            statusMessage += $"Invalid Mark {markValue} : ";
                            model.Status = StatusMain.Error;
                            break;
                        }
                    }
                    else
                    {
                        statusMessage += $"Blank Mark : ";
                        model.Status = StatusMain.Error;
                        break;
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "entity" ) )
                {
                    string entityValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( entityValue ) )
                    {
                        model.EntityLoanetId = entityValue;
                    }
                    else
                    {
                        statusMessage += $"Blank Entity : ";
                        model.Status = StatusMain.Error;
                        break;
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "contraparty" ) )
                {
                    string contraPartyValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( contraPartyValue ) )
                    {
                        model.ContraEntity = contraPartyValue;
                    }
                    else
                    {
                        statusMessage += $"Blank ContraParty : ";
                        model.Status = StatusMain.Error;
                        break;
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "tradetype" ) )
                {
                    string tradeTypeValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( tradeTypeValue ) )
                    {
                        try
                        {
                            model.TradeType = (TradeType)Enum.Parse( typeof( TradeType ), tradeTypeValue );

                            if ( (model.TradeType != TradeType.StockBorrow) && (model.TradeType != TradeType.StockLoan) )
                            {
                                statusMessage += $"Invalid TradeType {tradeTypeValue} : ";
                                model.Status = StatusMain.Error;
                                break;
                            }
                        }
                        catch
                        {
                            statusMessage += $"Unknown TradeType {tradeTypeValue} : ";
                            model.Status = StatusMain.Error;
                            break;
                        }
                    }
                    else
                    {
                        statusMessage += $"Blank TradeType : ";
                        model.Status = StatusMain.Error;
                        break;
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "contractnumber" ) )
                {
                    model.ContractNumber = column.ToString();
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "cusip" ) )
                {
                    model.SecurityNumber = column.ToString();
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "rebateratecode" ) )
                {
                    model.RebateRateCode = column.ToString();
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "profitid" ) )
                {
                    model.ProfitId = column.ToString();
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "termdate" ) )
                {
                    string termDateValue = column.ToString();
                    if ( !string.IsNullOrWhiteSpace( termDateValue ) )
                    {
                        try
                        {
                            model.TermDate = DateTime.Parse( termDateValue );
                        }
                        catch
                        {
                            statusMessage += $"Invalid TermDate {termDateValue} : ";
                            model.Status = StatusMain.Error;
                            break;
                        }
                    }
                }

                if ( ParseLogic.FindMatchEquals( dataRow.GetColumn( index ).ToLower(), "comments" ) )
                {
                    model.Comments = column.ToString();
                }

                index++;
            }

            if ( model.Status == StatusMain.Ready )
            {
                if ( !string.IsNullOrWhiteSpace( model.SecurityNumber ) )
                {
                    Issue issue = DataIssue.LoadIssue( model.SecurityNumber );

                    if ( issue.IssueId == 0 )
                    {
                        statusMessage += $"Unable to find Issue with SecurityNumber {model.SecurityNumber} : ";
                        model.Status = StatusMain.Error;
                    }
                    else
                    {
                        model.Issue = issue;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( model.EntityLoanetId ) )
                {
                    try
                    {
                        var company = DataEntity.LoadEntityByLoanetId( model.EntityLoanetId );
                        model.EntityId = company.CompanyId.ToString();
                    }
                    catch
                    {
                        statusMessage += $"Unable to find Entity with LoanetId {model.EntityLoanetId} : ";
                        model.Status = StatusMain.Error;
                    }
                }

                model.UpdateType = string.IsNullOrWhiteSpace( model.ContractNumber ) ?
                    ContractCompareImportUpdateType.New : ContractCompareImportUpdateType.Update;
            }
            else
            {
                model.UpdateType = ContractCompareImportUpdateType.NotApplicable;
            }

            model.StatusMessage += statusMessage + "Processed";

            return model;
        }

        public static string CreateStatusMessage( List<SL_Activity> activities )
        {
            if ( activities.Count > 0 )
            {
                string pendingIds = String.Join( " ", activities.Where( o => o.ActivityFlag == SL_ActivityFlag.Pending ).Select( o => o.SLActivity.ToString() ) );
                string progressIds = String.Join( " ", activities.Where( o => o.ActivityFlag == SL_ActivityFlag.Processing ).Select( o => o.SLActivity.ToString() ) );
                string completedIds = String.Join( " ", activities.Where( o => o.ActivityFlag == SL_ActivityFlag.Completed ).Select( o => o.SLActivity.ToString() ) );
                string errorIds = String.Join( " ", activities.Where( o => o.ActivityFlag == SL_ActivityFlag.Failed ).Select( o => o.SLActivity.ToString() ) );

                if ( pendingIds.Length > 0 )
                {
                    pendingIds = $"Pending {pendingIds}";
                }
                if ( progressIds.Length > 0 )
                {
                    progressIds = $"Processing {progressIds}";
                }
                if ( completedIds.Length > 0 )
                {
                    completedIds = $"Completed {completedIds}";
                }
                if ( errorIds.Length > 0 )
                {
                    errorIds = $"Failed {errorIds}";
                }

                return $"Request Id(s) = {pendingIds} {progressIds} {completedIds} {errorIds}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
