using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services;
using Microsoft.AspNetCore.Http;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.Payments.PaySubs.Models;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Microsoft.Extensions.Primitives;
namespace Nop.Plugin.Payments.PaySubs.Controllers
{

    public class PaySubsPaymentController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;

        public PaySubsPaymentController(ISettingService settingService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            ILogger logger,
            IWebHelper webHelper)
        {

            this._settingService = settingService;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
            this._orderProcessingService = orderProcessingService;
            this._orderService = orderService;
            this._logger = logger;
            this._webHelper = webHelper;
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            PaySubsPaymentSettings vCSPaymentSettings = _settingService.LoadSetting<PaySubsPaymentSettings>(0);
            var model = new PaySubsPaymentModel
            {
                AdditionalFee = vCSPaymentSettings.AdditionalFee,
                HashParameter = vCSPaymentSettings.HashParameter,
                TerminalId = vCSPaymentSettings.TerminalId,
                Instruction = vCSPaymentSettings.Instruction,
                RecurringPayments = vCSPaymentSettings.RecurringPayments,
                RecurrenceFrequency = vCSPaymentSettings.RecurrenceFrequency
            };
            return View("~/Plugins/Payments.PaySubs/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(PaySubsPaymentModel paModel)
        {
            if (!ModelState.IsValid)
                return Configure();


            PaySubsPaymentSettings vCSPaymentSettings = this._settingService.LoadSetting<PaySubsPaymentSettings>(0);
            vCSPaymentSettings.AdditionalFee = paModel.AdditionalFee;
            vCSPaymentSettings.HashParameter = paModel.HashParameter;
            vCSPaymentSettings.TerminalId = paModel.TerminalId;
            vCSPaymentSettings.Instruction = paModel.Instruction;
            vCSPaymentSettings.RecurringPayments = paModel.RecurringPayments;
            vCSPaymentSettings.RecurrenceFrequency = paModel.RecurrenceFrequency;
            this._settingService.SaveSetting<PaySubsPaymentSettings>(vCSPaymentSettings, 0);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [HttpGet]
        public IActionResult CancelCallback()
        {
            var orderById = Convert.ToInt32(_webHelper.QueryString<string>("p2"));

            var order = this._orderService.GetOrderById(orderById);
            if (order != null)
            {
                if (_orderProcessingService.CanCancelOrder(order))
                {
                    _orderProcessingService.CancelOrder(order, true);
                }
            }
            return RedirectToAction(orderById.ToString().Trim(), "orderdetails");
        }

        [HttpGet]
        public IActionResult Callback()
        {
            bool _isPaid = false;
            StringBuilder stringBuilder = new StringBuilder();

            var form = new Dictionary<string, string>();

            string f1 = Request.QueryString.Value.ToString();
            char[] delims = { '?', '&' };
            string[] parts = f1.Split(delims);
            foreach (var part in parts)
            {
                string[] param = part.Split('=');
                if (param[0] != "" && param[1] != "")
                {
                    form.Add(param[0], param[1]);
                }
            }

            try
            {
                string _approvedURL = string.Empty;
                _approvedURL = _webHelper.GetStoreLocation(false) + "Plugins/PaySubsPayment/Callback";
                PaySubsPaymentSettings vCSPaymentSettings = this._settingService.LoadSetting<PaySubsPaymentSettings>(0);

                // No checksum validation is necessary here as it is done on the gateway side
                string text = form["p3"].Length > 0 ? form["p3"] : "";
                int num;
                int.TryParse(form["m_1"], out num);
                Order orderById = this._orderService.GetOrderById(num);

                if (!string.IsNullOrEmpty(text) && text.Contains("APPROVED"))
                {
                    if (orderById != null && this._orderProcessingService.CanMarkOrderAsPaid(orderById))
                    {
                        this._orderProcessingService.MarkOrderAsPaid(orderById);
                        _isPaid = true;
                    }
                }
                string _redirectURL = string.Empty;
                _redirectURL = _webHelper.GetStoreLocation(false);

                if (_isPaid)
                {
                    return RedirectToAction("completed", "checkout");
                }
                else
                {
                    Order _failedOrder = this._orderService.GetOrderById(num);
                    OrderNote _note = new OrderNote();
                    _note.CreatedOnUtc = DateTime.Now;
                    _note.DisplayToCustomer = true;
                    _note.Note = "Payment failed with the following description: " + text;
                    _failedOrder.OrderStatus = OrderStatus.Cancelled;
                    _failedOrder.OrderNotes.Add(_note);
                    this._orderService.UpdateOrder(_failedOrder);
                    return RedirectToAction(num.ToString().Trim(), "orderdetails");
                }
            }
            catch (Exception ex)
            {
                this._logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "PaySubs Credit card error", ex.ToString(), null);
            }
            return Content(string.Format("<CallbackResponse>Failed</CallbackResponse>", stringBuilder.ToString()));
        }

        [HttpPost]
        public IActionResult Callback(IFormCollection form)
        {
            bool _isPaid = false;
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string _approvedURL = string.Empty;
                _approvedURL = _webHelper.GetStoreLocation(false) + "Plugins/PaySubsPayment/Callback";
                PaySubsPaymentSettings vCSPaymentSettings = this._settingService.LoadSetting<PaySubsPaymentSettings>(0);

                // No checksum validation is necessary here as it is done on the gateway side
                string text = form["p3"].Any() ? form["p3"].First() : "";
                int num;
                int.TryParse(form["m_1"].First(), out num);
                Order orderById = this._orderService.GetOrderById(num);

                if (!string.IsNullOrEmpty(text) && text.Contains("APPROVED"))
                {
                    if (orderById != null && this._orderProcessingService.CanMarkOrderAsPaid(orderById))
                    {
                        this._orderProcessingService.MarkOrderAsPaid(orderById);
                        _isPaid = true;
                    }
                }
                string _redirectURL = string.Empty;
                _redirectURL = _webHelper.GetStoreLocation(false);

                if (_isPaid)
                {
                    return RedirectToAction("completed", "checkout");
                }
                else
                {
                    Order _failedOrder = this._orderService.GetOrderById(num);
                    OrderNote _note = new OrderNote();
                    _note.CreatedOnUtc = DateTime.Now;
                    _note.DisplayToCustomer = true;
                    _note.Note = "Payment failed with the following description: " + text;
                    _failedOrder.OrderStatus = OrderStatus.Cancelled;
                    _failedOrder.OrderNotes.Add(_note);
                    this._orderService.UpdateOrder(_failedOrder);
                    return RedirectToAction(num.ToString().Trim(), "orderdetails");
                }
            }
            catch (Exception ex)
            {
                this._logger.InsertLog(Core.Domain.Logging.LogLevel.Error, "PaySubs Credit card error", ex.ToString(), null);
            }
            return Content(string.Format("<CallbackResponse>Failed</CallbackResponse>", stringBuilder.ToString()));
        }
    }
}