using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class ActionController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetActionList( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId, string name, SL_ActionType type )
        {
            var list = DataAction.LoadActionList( effectiveDate, entityId, name, type );

            return Json( list.ToDataSourceResult( request ) );
        }

        public ActionResult GetActionListNames( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            var list = DataAction.LoadActionNameList( effectiveDate, entityId );

            return Json( list.ToDataSourceResult( request ) );
        }

        public ActionResult GetActionListNamesDropdown( [DataSourceRequest] DataSourceRequest request, DateTime effectiveDate, string entityId )
        {
            var list = DataAction.LoadActionNameList( effectiveDate, entityId );

            return Json( list, JsonRequestBehavior.AllowGet );
        }

        public void AddActionBoxListItem( List<SL_BoxCalculationExtendedProjection> boxList, string contraEntityId, bool useNet, bool useBorrow, bool useLoan, bool useReturn, bool useRecall, SL_ActionType type, string name, bool useTraded )
        {

            foreach ( var item in boxList )
            {
                if ( item.IssueId == null ) continue;
                var action = new SL_ActionList
                {
                    EntityId = item.EntityId,
                    EffectiveDate = DateTime.Today,
                    ContraEntity = contraEntityId,
                    IssueId = (int)item.IssueId,
                    Name = name,
                    ActionType = type
                };

                if ( useNet )
                {
                    action.Quantity = item.NetPositionSettled;
                }
                else if ( useBorrow )
                {
                    action.Quantity = ( useTraded ) ? item.SuggestionBorrowTraded : item.SuggestionBorrowSettled;
                }
                else if ( useLoan )
                {
                    action.Quantity = ( useTraded ) ? item.SuggestionLoanTraded : item.SuggestionLoanSettled;
                }
                else if ( useReturn )
                {
                    action.Quantity = ( useTraded ) ? item.SuggestionReturnTraded : item.SuggestionReturnSettled;
                }
                else if ( useRecall )
                {
                    action.Quantity = ( useTraded ) ? item.SuggestionRecallTraded : item.SuggestionRecallSettled;
                }
                else
                {
                    action.Quantity = 0;
                }

                DataAction.AddActionItem( action );
            }
        }

        public void AddActionContractListItem( List<SL_ContractExtendedProjection> contractList, SL_ActionType type, string name )
        {
            foreach ( var action in contractList.Select( item => new SL_ActionList
            {
                EntityId = item.EntityId,
                EffectiveDate = DateTime.Today,
                ContraEntity = item.ContraEntity,
                IssueId = item.IssueId,
                Name = name,
                ActionType = type,
                Quantity = item.Quantity
            } ) )
            {
                DataAction.AddActionItem( action );
            }
        }

        public void AddActionListItem( List<SL_RecallExtendedProjection> recallList, SL_ActionType type, string name )
        {
            foreach ( var action in from item in recallList
                                    where item.IssueId != null
                                    select new SL_ActionList
                                        {
                                            EntityId = item.EntityId,
                                            EffectiveDate = DateTime.Today,
                                            ContraEntity = item.ContraEntity,
                                            IssueId = (int)item.IssueId,
                                            Name = name,
                                            ActionType = type,
                                            Quantity = item.QuantityRecalled
                                        } )
            {
                DataAction.AddActionItem( action );
            }
        }

        public void AddActionListItem( List<SL_RegulatoryListProjection> regList, SL_ActionType type, string name )
        {
            foreach ( var action in regList.Select( item => item.IssueId != null ? new SL_ActionList
            {
                EntityId = item.EntityId,
                EffectiveDate = DateTime.Today,
                ContraEntity = "",
                IssueId = (int)item.IssueId,
                Name = name,
                ActionType = type,
                Quantity = item.Quantity
            } : null ) )
            {
                DataAction.AddActionItem( action );
            }
        }

        public PartialViewResult LoadActionBoxCalculationPartial( List<SL_BoxCalculationExtendedProjection> list )
        {
            return PartialView( "~/Areas/DomesticTrading/Views/Action/_ActionAddBoxCalculation.cshtml", list );
        }

        public PartialViewResult LoadActionContractPartial( List<SL_ContractExtendedProjection> list )
        {
            return PartialView( "~/Areas/DomesticTrading/Views/Action/_ActionAddContract.cshtml", list );
        }

        public PartialViewResult LoadActionRecallPartial( List<SL_RecallExtendedProjection> list )
        {
            return PartialView( "~/Areas/DomesticTrading/Views/Action/_ActionAddRecall.cshtml", list );
        }

        public PartialViewResult LoadActionRegPartial( List<SL_RegulatoryListProjection> boxList )
        {
            return PartialView( "~/Areas/DomesticTrading/Views/Action/_ActionAddReg.cshtml", boxList );
        }

        public PartialViewResult LoadActionListPartial( DateTime effectiveDate, string entity, string name, SL_ActionType type )
        {
            List<SL_ActionListProjection> actionList = DataAction.LoadActionList( effectiveDate, entity, name, type );

            return PartialView( "~/Areas/DomesticTrading/Views/Action/_ActionViewListPartial.cshtml", actionList );
        }
    }
}
