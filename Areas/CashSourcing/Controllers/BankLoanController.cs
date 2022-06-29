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
using SLTrader.Models;
using SLTrader.Helpers.ExportHelper;

namespace SLTrader.Areas.CashSourcing.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class BankLoanController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReadBanks([DataSourceRequest] DataSourceRequest request, string entityId)
        {
            List<SL_Bank> bankList;

            try
            {
                bankList = DataBankLoan.LoadBanks(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(bankList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public ActionResult ReadPortfolioRequirement([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate)
        {
            List<PortfolioRequirementModel> portfolioList = DataBoxCalculation.LoadBoxCalculation(effectiveDate, "42", 0,0,false,false,false, false).Select(x => new PortfolioRequirementModel()
            {
                EffectiveDate  =DateTime.Today,
                IssueId  =(int)x.IssueId,
                SecurityNumber = x.SecurityNumber,
                Ticker = x.Ticker,
                Price = Convert.ToDecimal(x.Price),
                DeliverableQuantity = x.CnsFailToDeliverPositionSettled + x.DvpFailToDeliverPositionSettled + x.BrokerFailToDeliverPositionSettled,
                DeliverableMoney = x.CnsFailToDeliverPositionSettledAmt + x.DvpFailToDeliverPositionSettledAmt + x.BrokerFailToDeliverPositionSettledAmt,
                ReceiveableQuantity = x.CnsFailToRecievePositionSettled + x.DvpFailToRecievePositionSettled + x.BrokerFailToRecievePositionSettled,
                RecieveableMoney = x.CnsFailToRecievePositionSettledAmt + x.DvpFailToRecievePositionSettledAmt + x.BrokerFailToRecievePositionSettledAmt,
                MarginRequirement = 0,
                Perecentage = 0,
                FailChargeAmount = 0
            }).ToList();


            var totalRquirement = portfolioList.Sum(x => x.DeliverableMoney - x.ReceiveableQuantity);


            foreach (var item in portfolioList)
            {
                item.Perecentage = Math.Abs((item.DeliverableMoney - item.RecieveableMoney) / totalRquirement) * 100;

                item.MarginRequirement = (item.DeliverableMoney - item.RecieveableMoney) * item.Perecentage;
            }


            return Extended.JsonMax(portfolioList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult ReadBankLoanBookKeeping(decimal bankLoanId)
        {
            List<SL_BankLoanBookKeeping> bankList = new List<SL_BankLoanBookKeeping>();

            try
            {
                bankList = DataBankLoan.LoadBankLoanBookKeeping(bankLoanId);
            }
            catch (Exception e)
            {
                return PartialView("~/Areas/DomesticTrading/views/Shared/templates/_ErrorMessage.cshtml", e.Message);
            }

            return PartialView("~/Areas/CashSourcing/Views/LoanManagement/_BankLoanBookKeeping.cshtml", bankList);
        }

        public ActionResult Read_BankLoans([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, decimal bankId)
        {
            List<SL_BankLoanExtendedProjection> bankLoanList;

            try
            {
                bankLoanList = DataBankLoan.LoadBankLoanExtended(effectiveDate, entityId, bankId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(bankLoanList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReadOSIMovements([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate)
        {
            List<SL_OSIStockMovement> bankLoanList;

            try
            {
                bankLoanList = DataBankLoan.LoadOSIMovements(effectiveDate, "");
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(bankLoanList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_BankLoan([DataSourceRequest] DataSourceRequest request, SL_BankLoanExtendedProjection bankLoan)
        {

            try
            {
                var _bankLoan = new SL_BankLoan()
                {
                    AccountNumber = string.IsNullOrWhiteSpace(bankLoan.AccountNumber) ? "" : bankLoan.AccountNumber,
                    BankId = bankLoan.BankId,
                    AccountType = string.IsNullOrWhiteSpace(bankLoan.AccountType) ? "" : bankLoan.AccountType,
                    BankLoanMoneyType = bankLoan.BankLoanMoneyType,
                    EntityId = bankLoan.EntityId,
                    LoanDate = bankLoan.LoanDate,
                    LoanLimit = bankLoan.LoanLimit,
                    Name = string.IsNullOrWhiteSpace(bankLoan.Name) ? "" : bankLoan.Name,
                    SLBankLoan = bankLoan.SLBankLoan
                };

                DataBankLoan.AddBankLoan(_bankLoan);

                bankLoan.SLBankLoan = _bankLoan.SLBankLoan;
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { bankLoan }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_BankLoan([DataSourceRequest] DataSourceRequest request, SL_BankLoanExtendedProjection bankLoan)
        {
            SL_BankLoan _bankLoan = new SL_BankLoan();

            try
            {
                _bankLoan = DataBankLoan.LoadBankLoanByPk(bankLoan.SLBankLoan);

                _bankLoan.AccountNumber = string.IsNullOrWhiteSpace(bankLoan.AccountNumber) ? "" : bankLoan.AccountNumber;
                _bankLoan.BankLoanMoneyType = bankLoan.BankLoanMoneyType;
                _bankLoan.LoanDate = bankLoan.LoanDate;
                _bankLoan.LoanLimit = bankLoan.LoanLimit;
                _bankLoan.AccountType = string.IsNullOrWhiteSpace(bankLoan.AccountType) ? "" : bankLoan.AccountType;
                _bankLoan.Name = string.IsNullOrWhiteSpace(bankLoan.Name) ? "" : bankLoan.Name;

                DataBankLoan.UpdateBankLoan(_bankLoan);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { bankLoan }.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddBank([DataSourceRequest] DataSourceRequest request, SL_Bank bankObject)
        {
            try
            {
                DataBankLoan.AddBank(bankObject);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { bankObject }, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdateBank([DataSourceRequest] DataSourceRequest request, SL_Bank bankObject)
        {
            SL_Bank _bankObject = new SL_Bank();

            try
            {
                _bankObject = DataBankLoan.LoadBankByPk(bankObject.SLBank);

                _bankObject.Bank = bankObject.Bank;
                _bankObject.Name = bankObject.Name;
                _bankObject.IsOCC = bankObject.IsOCC;

                DataBankLoan.UpdateBank(_bankObject);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { _bankObject }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddBookkeeping([DataSourceRequest] DataSourceRequest request, SL_BankLoanBookKeeping bankObject)
        {
            try
            {
                bankObject.AccountNumber = string.IsNullOrWhiteSpace(bankObject.AccountNumber) ? "" : bankObject.AccountNumber;

                DataBankLoan.AddBankLoanBookKeeping(bankObject);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { bankObject }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateBookkeeping([DataSourceRequest] DataSourceRequest request, SL_BankLoanBookKeeping bankObject)
        {
            SL_BankLoanBookKeeping _bankObject = new SL_BankLoanBookKeeping();
            try
            {
                _bankObject = DataBankLoan.LoadBankLoanBookKeepingByPk(bankObject.SLBankLoanBookKeeping);

                _bankObject.AccountNumber = string.IsNullOrWhiteSpace(bankObject.AccountNumber) ? "" : bankObject.AccountNumber;
                _bankObject.BankLoanActionType = bankObject.BankLoanActionType;
                _bankObject.FirmId = bankObject.FirmId;
                _bankObject.FromLocation = bankObject.FromLocation;
                _bankObject.ToLocation = bankObject.ToLocation;

                DataBankLoan.UpdateBankLoanBookKeeping(_bankObject);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { _bankObject }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteBookkeeping([DataSourceRequest] DataSourceRequest request, SL_BankLoanBookKeeping bankObject)
        {
            try
            {
                DataBankLoan.DeleteBankLoanBookKeeping(DataBankLoan.LoadBankLoanBookKeepingByPk(bankObject.SLBankLoanBookKeeping));
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(new[] { bankObject }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_BanksDropdown(string entityId)
        {
            List<SL_Bank> bankList;

            try
            {
                bankList = DataBankLoan.LoadBanks(entityId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(bankList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_BankLoansDropdown(string entityId, decimal bankId)
        {
            List<SL_BankLoan> bankLoanList;

            try
            {
                bankLoanList = DataBankLoan.LoadBankLoans(entityId, bankId);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(bankLoanList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_BankLoanActivity([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, decimal bankId, DateTime loanDate)
        {
            List<SL_BankLoanActivityExtendedProjection> bankLoanActivityList;

            try
            {
                bankLoanActivityList = DataBankLoan.LoadBankLoanActivityByLoanDate(effectiveDate, entityId, bankId, loanDate.Date);
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(bankLoanActivityList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_BankLoanPosition([DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId)
        {
            List<SL_BankLoanPositionExtendedProjection> bankLoanPositionList = new List<SL_BankLoanPositionExtendedProjection>();
            List<SL_Bank> bankList = DataBankLoan.LoadBanks(entityId);

            try
            {
                foreach ( var bank in bankList )
                {
                    var bankLoanList = DataBankLoan.LoadBankLoanExtended( effectiveDate, entityId, bank.SLBank );

                    foreach ( var bankLoan in bankLoanList )
                    {
                        var bankLoanPositions = DataBankLoan.LoadBankLoanPositions( effectiveDate, entityId, bank.SLBank, bankLoan.LoanDate, bankLoan.AccountNumber );
                        var bankLoanActivity = DataBankLoan.LoadBankLoanActivityByLoanDate( effectiveDate, entityId, bank.SLBank, bankLoan.LoanDate );

                        foreach ( var bankLoanPositionItem in bankLoanPositions )
                        {
                            bankLoanPositionItem.Quantity = bankLoanPositionItem.Quantity - ( bankLoanActivity.Where( x => x.IssueId == bankLoanPositionItem.IssueId && x.ActionType == SL_BankLoanActionType.Release && ( x.BankLoanMadeStatus != SL_DtccItemDOPendMade.Made ) ) ).Sum( x => x.Quantity );
                            bankLoanPositionItem.Amount = bankLoanPositionItem.Quantity * bankLoanPositionItem.Price;
                        }

                        bankLoanPositionList.AddRange( bankLoanPositions );
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowJsonError(e);
            }

            return Extended.JsonMax(bankLoanPositionList.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult BankLoanPositionsPartial(DateTime effectiveDate, string entityId)
        {
            List<SL_BankLoanPositionExtendedProjection> bankLoanPositionList = new List<SL_BankLoanPositionExtendedProjection>();
            List<SL_Bank> bankList = DataBankLoan.LoadBanks(entityId);

            try
            {
                foreach(var bank in bankList)
                {
                    var bankLoanList = DataBankLoan.LoadBankLoanExtended(effectiveDate, entityId, bank.SLBank);

                    foreach (var bankLoan in bankLoanList)
                    {
                        var bankLoanPositions = DataBankLoan.LoadBankLoanPositions(effectiveDate, entityId, bank.SLBank, bankLoan.LoanDate, bankLoan.AccountNumber);
                        var bankLoanActivity = DataBankLoan.LoadBankLoanActivityByLoanDate(effectiveDate, entityId, bank.SLBank, bankLoan.LoanDate);

                       foreach (var bankLoanPositionItem in bankLoanPositions)
                        {
                            bankLoanPositionItem.Quantity = bankLoanPositionItem.Quantity - (bankLoanActivity.Where(x => x.IssueId == bankLoanPositionItem.IssueId && x.ActionType == SL_BankLoanActionType.Release && (x.BankLoanDepositorySubmissionStatus == StatusMain.InProgress || x.BankLoanDepositorySubmissionStatus == StatusMain.Pending))).Sum(x => x.Quantity);
                            bankLoanPositionItem.Amount = bankLoanPositionItem.Quantity * bankLoanPositionItem.Price;
                        }

                        bankLoanPositionList.AddRange(bankLoanPositions);
                    }
                }
            }
            catch (Exception e)
            {
                ThrowJsonError(e);
            }


            return PartialView("~/Areas/CashSourcing/Views/LoanManagement/_BankLoanPosition.cshtml", bankLoanPositionList);
        }

        public PartialViewResult BankLoanPledgePartial(List<SL_BoxCalculationExtendedProjection> pledgeItems)
        {
            var bankLoanPledgeItems =  pledgeItems.Select(x => new BankLoanPledgeModel()
            {
                EntityId = x.EntityId,
                Issue = (int)x.IssueId,
                SecurityNumber = x.SecurityNumber,
                Ticker = x.Ticker,
                Price = Convert.ToDecimal(x.Price),
                PledgeQuantity = x.DepositorySettled,
                PledgeAmount = x.DepositorySettledAmt
            }).ToList();

            return PartialView("~/Areas/CashSourcing/Views/LoanManagement/_BankLoanPledge.cshtml", bankLoanPledgeItems);
        }

        public PartialViewResult BankLoanReleasePartial(List<SL_BankLoanPositionExtendedProjection> releaseItems)
        {        
            return PartialView("~/Areas/CashSourcing/Views/LoanManagement/_BankLoanRelease.cshtml", releaseItems);
        }




        public ActionResult UpdateBankLoanPledgeModel([DataSourceRequest] DataSourceRequest request, BankLoanPledgeModel model)
        {
            model.PledgeAmount = model.PledgeQuantity * model.Price;

            return Extended.JsonMax(new[] { model }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteBankLoanPledgeModel([DataSourceRequest] DataSourceRequest request, BankLoanPledgeModel model)
        {      
            return Extended.JsonMax(new[] { model }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateBankLoanReleaseModel([DataSourceRequest] DataSourceRequest request, SL_BankLoanPositionExtendedProjection model)
        {
            model.Amount = model.Quantity * model.Price;

            return Extended.JsonMax(new[] { model }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteBankLoanReleaseModel([DataSourceRequest] DataSourceRequest request, SL_BankLoanPositionExtendedProjection model)
        {
            return Extended.JsonMax(new[] { model }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_BankLoanSecurityHypothicationDropdown()
        {
            List<EnumModel> enumList = new List<EnumModel>();



            enumList.Add(new EnumModel()
            {
                Text = "[1] MultipleCustomers",
                Description = "",
                Value = "1"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[2] SingleCustomer",
                Description = "",
                Value = "2"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[3] NoCustomer",
                Description = "",
                Value = "3"
            });

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_BankLoanPLedgePurposeDropdown()
        {
            List<EnumModel> enumList = new List<EnumModel>();



            enumList.Add(new EnumModel()
            {
                Text = "[1] NewLoan",
                Description = "",
                Value = "1"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[2] IncreaseLoan",
                Description = "",
                Value = "2"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[3] AdditionalCollateral",
                Description = "",
                Value = "3"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[4] SubstitutionCollateral",
                Description = "",
                Value = "4"
            });

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read_BankLoanReleaseReasonDropdown()
        {
            List<EnumModel> enumList = new List<EnumModel>();

            enumList.Add(new EnumModel()
            {
                Text = "[1] Delivery",
                Description = "",
                Value = "1"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[2] SubstitutionofCollateral",
                Description = "",
                Value = "2"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[3] DecreaseofCollateral",
                Description = "",
                Value = "3"
            });

            enumList.Add(new EnumModel()
            {
                Text = "[4] ReleaseofExcessCollateral",
                Description = "",
                Value = "4"
            });

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessBankLoanPledge(
            string entityId,
            string securityHypothication,
            string pledgePurpose,
            DateTime LoanDate,
            decimal BankId,
            SL_BankLoanMoneyType moneyType,
            List<BankLoanPledgeModel> items
            )
        {
            var returnPledgeList = new List<BankLoanPledgeModel>();

            foreach (var item in items)
            {
                try
                {
                    SL_BankLoanActivity activity = new SL_BankLoanActivity();

                    activity.SecurityHypothication = securityHypothication;
                    activity.Amount = item.PledgeAmount;
                    activity.Quantity = item.PledgeQuantity;
                    activity.BankId = BankId;
                    activity.BankLoanActionType = SL_BankLoanActionType.Pledge;
                    activity.BankLoanBookKeepingStatus = StatusMain.Pending;
                    activity.BankLoanDepositorySubmissionStatus = StatusMain.Pending;
                    activity.BankLoanItemRequest = "";
                    activity.BankLoanItemResponse = "";
                    activity.BankLoanMadeStatus = SL_DtccItemDOPendMade.Unknown;
                    activity.BankLoanMoneyType = moneyType;
                    activity.BankLoanPledgePurpose = pledgePurpose;
                    activity.BankLoanReleaseType = "";
                    activity.EffectiveDate = DateTime.Today;
                    activity.EntityId = entityId;
                    activity.IssueId = item.Issue;
                    activity.LoanDate = LoanDate;
                    activity.MemoInfo = "";

                    DataBankLoan.AddBankLoanActivity( activity );
                }
                catch(Exception e)
                {
                    returnPledgeList.Add( item );
                    item.MemoInfo = e.Message;
                }
            }

            return Extended.JsonMax( returnPledgeList, JsonRequestBehavior.AllowGet );
        }

        public JsonResult ProcessBankLoanRelease(
            string releaseReason,
            SL_BankLoanMoneyType moneyType,
               List<SL_BankLoanPositionExtendedProjection> items
            )
        {
            List<SL_BankLoanPositionExtendedProjection> positionExtendedList = new List<SL_BankLoanPositionExtendedProjection>();
            foreach ( var item in items )
            {
                try
                {
                    SL_BankLoanActivity activity = new SL_BankLoanActivity();

                    activity.SecurityHypothication = "";
                    activity.Amount = item.Amount;
                    activity.Quantity = item.Quantity;
                    activity.BankId = item.BankId;
                    activity.BankLoanActionType = SL_BankLoanActionType.Release;
                    activity.BankLoanBookKeepingStatus = StatusMain.Pending;
                    activity.BankLoanDepositorySubmissionStatus = StatusMain.Pending;
                    activity.BankLoanItemRequest = "";
                    activity.BankLoanItemResponse = "";
                    activity.BankLoanMadeStatus = SL_DtccItemDOPendMade.Unknown;
                    activity.BankLoanMoneyType = moneyType;
                    activity.BankLoanPledgePurpose = "";
                    activity.BankLoanReleaseType = releaseReason;
                    activity.EffectiveDate = DateTime.Today;
                    activity.EntityId = item.EntityId;
                    activity.IssueId = Convert.ToInt32( item.IssueId );
                    activity.LoanDate = item.LoanDate;
                    activity.MemoInfo = "";

                    DataBankLoan.AddBankLoanActivity( activity );
                }
                catch ( Exception e )
                {
                    item.MemoInfo = e.Message;
                    positionExtendedList.Add( item );
                }
            }

            return Extended.JsonMax( positionExtendedList, JsonRequestBehavior.AllowGet );
        }

        public JsonResult RetransmitOSIStockMovement(List<SL_OSIStockMovement> stockMovementList)
        {
            List<SL_OSIStockMovement> _stockMovementList = new List<SL_OSIStockMovement>();

            foreach (var item in stockMovementList)
            {
                var _stockMovement = new SL_OSIStockMovement();

                try
                {
                    _stockMovement = DataBankLoan.LoadOSIMovementByPK(item.SLOSIStockMovement);

                    _stockMovement.StatusMain = StatusMain.Ready;
                    _stockMovement.MemoInfo = "";

                    DataBankLoan.UpdateOSIMovement(_stockMovement);


                }
                catch (Exception e)
                {
                    _stockMovement.MemoInfo = e.Message;
                }

                _stockMovementList.Add(_stockMovement);
            }

            return Extended.JsonMax(_stockMovementList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportOSIBreakReportToExcel(DateTime effectiveDate, string entityId)
        {
            var reportDetails = new List<SL_OSIStockMovement>();
            var stockMovementList = DataBankLoan.LoadOSIMovements(effectiveDate, entityId);

            reportDetails = stockMovementList.Where(x => x.StatusMain == StatusMain.Error).ToList();

            var fName = string.Format("osiStockMovementReport-{0}.xlsx", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

            return File(ExcelHelper.ExportExcelBankLoanOSIBreak(reportDetails, "osiStockMovementReport", true), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fName);
        }
    }
}
