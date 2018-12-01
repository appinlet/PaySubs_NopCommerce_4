using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PaySubs.Components
{
    [ViewComponent(Name = "PaySubs")]
    public class PaySubsViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Nop.Plugin.Payments.PaySubs/Views/PaymentInfo.cshtml");
        }
    }
}
