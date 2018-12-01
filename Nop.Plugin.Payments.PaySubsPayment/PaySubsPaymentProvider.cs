using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.PaySubs.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Nop.Plugin.Payments.PaySubs
{
    public class PaySubsPaymentProvider : BasePlugin, IPaymentMethod
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingService meSettingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                if (GetRecurringPayments())
                {
                    return RecurringPaymentType.Automatic;
                }
                else
                {
                    return RecurringPaymentType.NotSupported;
                }
            }
        }

        public bool SkipPaymentInfo
        {
            get
            {
                return true;
            }
        }

        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaySubsPayment/Configure";
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PaySubs site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.PaySubs.PaymentMethodDescription"); }
        }

        public string VCSUrl
        {
            get
            {
                return "https://www.vcs.co.za/vvonline/ccform.asp";
            }
        }

        public PaySubsPaymentProvider(ISettingService paSettingService, IHttpContextAccessor paHttpContext, IWebHelper webHelper, ILocalizationService localizationService)
        {
            this._localizationService = localizationService;
            this.meSettingService = paSettingService;
            this._httpContextAccessor = paHttpContext;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            CancelRecurringPaymentResult cancelRecurringPaymentResult = new CancelRecurringPaymentResult();
            //cancelRecurringPaymentResult.AddError("Recurring payment not supported");
            return cancelRecurringPaymentResult;
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }
            return order.PaymentStatus == Core.Domain.Payments.PaymentStatus.Pending && (DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes >= 1.0;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            CapturePaymentResult capturePaymentResult = new CapturePaymentResult();
            capturePaymentResult.AddError("Capture method not supported");
            return capturePaymentResult;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            PaySubsPaymentSettings vCSPaymentSettings = this.meSettingService.LoadSetting<PaySubsPaymentSettings>(0);
            return vCSPaymentSettings.AdditionalFee;
        }

        public bool GetRecurringPayments()
        {
            PaySubsPaymentSettings vCSPaymentSettings = this.meSettingService.LoadSetting<PaySubsPaymentSettings>(0);
            var a = vCSPaymentSettings;
            return vCSPaymentSettings.RecurringPayments;
        }

        public string GetRecurrenceFrequency()
        {
            PaySubsPaymentSettings vCSPaymentSettings = this.meSettingService.LoadSetting<PaySubsPaymentSettings>(0);
            return vCSPaymentSettings.RecurrenceFrequency;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaySubsPayment";
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("Namespaces", "Nop.Plugin.Payments.PaySubs.Controllers");
            routeValueDictionary.Add("area", null);
            routeValues = routeValueDictionary;
        }

        public Type GetControllerType()
        {
            return typeof(PaySubsPaymentController);
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <param name="viewComponentName">View component name</param>
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentInfo";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return "PaySubsPayment";
        }

        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaySubsPayment";
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("Namespaces", "Nop.Plugin.Payments.PaySubs.Controllers");
            routeValueDictionary.Add("area", null);
            routeValues = routeValueDictionary;
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public override void Install()
        {
            PaySubsPaymentSettings vCSPaymentSettings = new PaySubsPaymentSettings
            {
                TerminalId = string.Empty,
                HashParameter = string.Empty,
                AdditionalFee = 0.00m,
                Instruction = "After order confirmation you will be redirected to the PaySubs site to complete the order."
            };
            this.meSettingService.SaveSetting<PaySubsPaymentSettings>(vCSPaymentSettings, 0);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RedirectionTip", "You will be redirected to PaySubs to complete the order.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.TerminalId", "Terminal Id", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.TerminalId.Hint", "Provided by PaySubs", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.HashParameter", "Hash Parameter", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.HashParameter.Hint", "Provided by PaySubs", null);

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.AdditionalFee", "Additional Fee", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.AdditionalFee.Hint", "Additional Fee", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.Instruction", "Instruction", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.Instruction.Hint", "Message for customer re.: redirect to PaySubs", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.PaymentMethodDescription", "Pay by Credit/Debit Card", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurringPayments", "Enable recurring payments?", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurringPayments.Hint", "Enable recurring payments for this site", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurrenceFrequency", "Recurrence frequency", null);
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurrenceFrequency.Hint", "Recurrence frequency", null);
            base.Install();
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            PaySubsPaymentSettings vCSPaymentSettings = this.meSettingService.LoadSetting<PaySubsPaymentSettings>(0);
            RemotePost remotePost = new RemotePost();
            remotePost.FormName = "PaySubs";
            remotePost.Method = "POST";
            remotePost.Url = this.VCSUrl;
            RemotePost remotePost2 = remotePost;
            remotePost2.Add("p1", vCSPaymentSettings.TerminalId);
            remotePost2.Add("p2", postProcessPaymentRequest.Order.Id.ToString());
            remotePost2.Add("p3", "Order Id:" + postProcessPaymentRequest.Order.Id.ToString());
            remotePost2.Add("p4", string.Format(CultureInfo.InvariantCulture, "{0:0.00}", new object[]
            {
                Math.Round(Convert.ToDouble(postProcessPaymentRequest.Order.OrderTotal), 2, MidpointRounding.AwayFromZero)
            }));
            remotePost2.Add("URLSProvided", "Y");

            if (vCSPaymentSettings.RecurringPayments)
            {
                remotePost2.Add("p6", "U"); //There are repeat payments
                remotePost2.Add("p7", vCSPaymentSettings.RecurrenceFrequency);
            }

            string _approvedURL = string.Empty;
            string _cancelledURL = string.Empty;
            _approvedURL = _webHelper.GetStoreLocation(false) + "Plugin/PaySubsPayment/Callback";
            _cancelledURL = _webHelper.GetStoreLocation(false) + "Plugin/PaySubsPayment/CancelCallback";
            remotePost2.Add("ApprovedUrl", _approvedURL);
            remotePost2.Add("DeclinedUrl", _approvedURL);
            //Cancellation URL
            remotePost2.Add("p10", _cancelledURL);

            remotePost2.Add("CardholderEmail", postProcessPaymentRequest.Order.Customer.Email ?? string.Empty);
            remotePost2.Add("m_1", postProcessPaymentRequest.Order.Id.ToString());

            var ConversionAct = CreateFormCollectionFromNameValues(remotePost2.Params);
            remotePost2.Add("Hash", PaySubsHelper.CalculateSigningRequest(ConversionAct, vCSPaymentSettings.HashParameter));

            remotePost2.Post();
        }


        private FormCollection CreateFormCollectionFromNameValues(NameValueCollection CollectionNameVal)
        {
            var dictionaryData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();

            foreach (var item in CollectionNameVal.AllKeys)
            {
                dictionaryData.Add(item, new Microsoft.Extensions.Primitives.StringValues(CollectionNameVal[item]));
            }

            return new FormCollection(dictionaryData);
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            ProcessPaymentResult processPaymentResult = new ProcessPaymentResult();
            processPaymentResult.NewPaymentStatus = Core.Domain.Payments.PaymentStatus.Pending;
            return processPaymentResult;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            PaySubsPaymentSettings vCSPaymentSettings = this.meSettingService.LoadSetting<PaySubsPaymentSettings>(0);
            ProcessPaymentResult processPaymentResult = new ProcessPaymentResult();

            if (vCSPaymentSettings.RecurringPayments)
            {
                processPaymentResult.AllowStoringCreditCardNumber = true;
            }
            else
            {
                processPaymentResult.AddError("Recurring payment not supported");

            }
            return processPaymentResult;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            RefundPaymentResult refundPaymentResult = new RefundPaymentResult();
            refundPaymentResult.AddError("Refund method not supported");
            return refundPaymentResult;
        }

        public override void Uninstall()
        {
            this.meSettingService.DeleteSetting<PaySubsPaymentSettings>();
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.TerminalId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.TerminalId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.HashParameter");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.HashParameter.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.Instruction");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.Instruction.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.PaymentMethodDescription");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurringPayments");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurringPayments.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurrenceFrequency");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PaySubs.Fields.RecurrenceFrequency.Hint");

            base.Uninstall();
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            VoidPaymentResult voidPaymentResult = new VoidPaymentResult();
            voidPaymentResult.AddError("Void method not supported");
            return voidPaymentResult;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }
    }
}
