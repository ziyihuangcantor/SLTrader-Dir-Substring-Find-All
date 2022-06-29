using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BondFire.Entities;
using BondFire.Entities.Projections;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using SLTrader.Custom;
using SLTrader.Models;
using SLTrader.Tools;
using SLTrader.Tools.Helpers;

namespace SLTrader.Areas.CashSourcing.Controllers
{
    public class CashSourcingController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
