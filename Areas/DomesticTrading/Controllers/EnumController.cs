using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Collections;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Models;
using SLTrader.Tools;
using SLTrader.Enums;

namespace SLTrader.Areas.DomesticTrading.Controllers
{
    public class EnumController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FilterMenuCustomization_TradeType(List<Company> entityList)
        {
            List<TradeType> tradeTypes = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeType().Select(x => x.TradeType).Distinct().ToList();

            List<TradeTypeTag> tagList = new List<TradeTypeTag>();
            tagList = tradeTypes.Select(x => new TradeTypeTag()
            {
                TradeType = x,
                TradeTypeDescription = x.ToString()
            }).ToList();

            return Extended.JsonMax(tagList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLDeliveryCodeEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_DeliveryCode flag in Enum.GetValues(typeof(SL_DeliveryCode)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLCollateralFlagEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_CollateralFlag flag in Enum.GetValues(typeof(SL_CollateralFlag)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLReturnCallback([DataSourceRequest] DataSourceRequest request)
        {           
            List<EnumModel> enumList = new List<EnumModel>();

            enumList.Add(AddEntityTypeItem(SL_EntityType.Callback));
            enumList.Add(AddEntityTypeItem(SL_EntityType.Return));

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLTradeTypeByExecutingSystem([DataSourceRequest] DataSourceRequest request, SL_ExecutionSystemType? executingSystem)
        {
            var list = DataExecutingSystemTypeTradeType.LoadExecutingSystemTypeTradeType( (SL_ExecutionSystemType) executingSystem);
      
            List<EnumModel> enumList = new List<EnumModel>();
            if (executingSystem != null)
            {
                if (executingSystem != null)
                {
                    foreach (var item in list)
                    {
                        enumList.Add(AddTradeTypeItem(item.TradeType));
                    }
                }
            }
 
            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLCollateralFlagByExecutingSystem([DataSourceRequest] DataSourceRequest request, SL_ExecutionSystemType? executingSystem)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            if (executingSystem != null)
            {

                switch (executingSystem)
                {
                    case SL_ExecutionSystemType.GLOBALONE:
                        enumList.Add(AddCollateralFlagItem(SL_CollateralFlag.N));
                        enumList.Add(AddCollateralFlagItem(SL_CollateralFlag.C));
                        enumList.Add(AddCollateralFlagItem(SL_CollateralFlag.P));
                        break;

                    default:
                    case SL_ExecutionSystemType.LOANETINTL:
                    case SL_ExecutionSystemType.LOANET:
                        enumList.Add(AddCollateralFlagItem(SL_CollateralFlag.B));
                        enumList.Add(AddCollateralFlagItem(SL_CollateralFlag.C));
                        enumList.Add(AddCollateralFlagItem(SL_CollateralFlag.L));
                        enumList.Add(AddCollateralFlagItem(SL_CollateralFlag.S));
                        break;
                }
            }

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Read_SLDeliveryCodeByTradeType([DataSourceRequest] DataSourceRequest request, TradeType? tradeType, SL_ExecutionSystemType executingSystem)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            if ((executingSystem == SL_ExecutionSystemType.LOANET) || (executingSystem == SL_ExecutionSystemType.LOANETINTL))
            {
                enumList.Add(AddDeliveryCodeItem(SL_DeliveryCode.PTS));
                enumList.Add(AddDeliveryCodeItem(SL_DeliveryCode.CCF));
                enumList.Add(AddDeliveryCodeItem(SL_DeliveryCode.NONE));
            }
            else if (executingSystem == SL_ExecutionSystemType.GLOBALONE)
            {
                enumList.Add(AddDeliveryCodeItem(SL_DeliveryCode.NONE));
                enumList.Add(AddDeliveryCodeItem(SL_DeliveryCode.DVP));
            }
            else
            {
                enumList.Add(AddDeliveryCodeItem(SL_DeliveryCode.NONE));
            }

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public EnumModel AddEntityTypeItem(SL_EntityType flag)
        {
            EnumModel model = new EnumModel();

            model.Text = flag.ToString();
            model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
            model.Description = flag.GetDescriptionDisplay();

            return model;
        }

        public EnumModel AddTradeTypeItem(TradeType flag)
        {
            EnumModel model = new EnumModel();

            model.Text = flag.ToString();
            model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
            model.Description = flag.GetDescriptionDisplay();

            return model;
        }


        public EnumModel AddDeliveryCodeItem(SL_DeliveryCode flag)
        {
            EnumModel model = new EnumModel();

            model.Text = flag.ToString();
            model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
            model.Description = flag.GetDescriptionDisplay();

            return model;
        }

        public EnumModel AddCollateralFlagItem(SL_CollateralFlag flag)
        {
            EnumModel model = new EnumModel();

            model.Text = flag.ToString();
            model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
            model.Description = flag.GetDescriptionDisplay();

            return model;
        }

        public EnumModel AddePriceAmountTypeItem(SL_TradePriceAmountCalcType flag)
        {
            EnumModel model = new EnumModel();

            model.Text = flag.ToString();
            model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
            model.Description = flag.GetDescriptionDisplay();

            return model;
        }



        

        public JsonResult Read_BankLoanMoneyTypeEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_BankLoanMoneyType flag in Enum.GetValues(typeof(SL_BankLoanMoneyType)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_BankLoanActionTypeEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_BankLoanActionType flag in Enum.GetValues(typeof(SL_BankLoanActionType)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLOperatorEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_Operator flag in Enum.GetValues(typeof(SL_Operator)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLActivityActionMarkerEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            EnumModel contractEnumModel = new EnumModel();
            contractEnumModel.Text = SL_EntityType.Contract.ToString();
            contractEnumModel.Value = Convert.ToInt32(SL_EntityType.Contract).ToString(CultureInfo.InvariantCulture);
            contractEnumModel.Description = SL_EntityType.Contract.ToString();

            enumList.Add(contractEnumModel);

            EnumModel tradeEnumModel = new EnumModel();
            tradeEnumModel.Text = SL_EntityType.Trade.ToString();
            tradeEnumModel.Value = Convert.ToInt32(SL_EntityType.Trade).ToString(CultureInfo.InvariantCulture);
            tradeEnumModel.Description = SL_EntityType.Trade.ToString();

            enumList.Add(tradeEnumModel);

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLOperatorStringOperatorEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();


            EnumModel model1 = new EnumModel();

            model1.Text = SL_Operator.contains.ToString();
            model1.Value = Convert.ToInt32(SL_Operator.contains).ToString(CultureInfo.InvariantCulture);
            model1.Description = SL_Operator.contains.GetDescriptionDisplay();

            enumList.Add(model1);

            EnumModel model2 = new EnumModel();

            model2.Text = SL_Operator.notcontain.ToString();
            model2.Value = Convert.ToInt32(SL_Operator.notcontain).ToString(CultureInfo.InvariantCulture);
            model2.Description = SL_Operator.notcontain.GetDescriptionDisplay();

            enumList.Add(model2);

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Read_SLColumnTypeEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_ColumnType flag in Enum.GetValues(typeof(SL_ColumnType)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLRecallReasonEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_RecallReason flag in Enum.GetValues(typeof(SL_RecallReason)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLRecallIndicatorEnum([DataSourceRequest] DataSourceRequest request)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            foreach (SL_RecallIndicator flag in Enum.GetValues(typeof(SL_RecallIndicator)))
            {
                EnumModel model = new EnumModel();

                model.Text = flag.ToString();
                model.Value = Convert.ToInt32(flag).ToString(CultureInfo.InvariantCulture);
                model.Description = flag.GetDescriptionDisplay();

                enumList.Add(model);
            }


            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Read_SLTradePriceAmountTypeEnum([DataSourceRequest] DataSourceRequest request, SL_ExecutionSystemType? executingSystem)
        {
            List<EnumModel> enumList = new List<EnumModel>();

            if (executingSystem == SL_ExecutionSystemType.LOANET)
            {
                enumList.Add(AddePriceAmountTypeItem(SL_TradePriceAmountCalcType.CASH));
                enumList.Add(AddePriceAmountTypeItem(SL_TradePriceAmountCalcType.MARKET));
                enumList.Add(AddePriceAmountTypeItem(SL_TradePriceAmountCalcType.ROUND));
            }
            else
            {
                enumList.Add(AddePriceAmountTypeItem(SL_TradePriceAmountCalcType.CASH));
            }

            return Extended.JsonMax(enumList, JsonRequestBehavior.AllowGet);
        }
    }
}
