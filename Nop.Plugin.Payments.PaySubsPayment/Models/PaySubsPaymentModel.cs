
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;


namespace Nop.Plugin.Payments.PaySubs.Models
{
	public class PaySubsPaymentModel : BaseNopModel
	{
		[NopResourceDisplayName("Plugins.Payments.PaySubs.Fields.AdditionalFee")]
		public decimal AdditionalFee
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugins.Payments.PaySubs.Fields.HashParameter")]
		public string HashParameter
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugins.Payments.PaySubs.Fields.TerminalId")]
		public string TerminalId
		{
			get;
			set;
		}

		[NopResourceDisplayName("Plugins.Payments.PaySubs.Fields.Instruction")]
		public string Instruction
		{
			get;
			set;
		}

        [NopResourceDisplayName("Plugins.Payments.PaySubs.Fields.RecurringPayments")]
        public bool RecurringPayments { get; set; }
        public bool RecurringPayments_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PaySubs.Fields.RecurrenceFrequency")]
		public string RecurrenceFrequency
		{
			get;
			set;
		}

        [NopResourceDisplayName("Plugins.Payments.PaySubs.Fields.RecurrenceFrequencies")]
        public List<SelectListItem> RecurrenceFrequencies { get; } = new List<SelectListItem>
        {
            new SelectListItem {Value = "D", Text = "Daily"},
            new SelectListItem {Value = "W", Text = "Weekly"},
            new SelectListItem {Value = "M", Text = "Monthly"},
            new SelectListItem {Value = "Q", Text = "Quarterly"},
            new SelectListItem {Value = "6", Text = "Bi-annually"},
            new SelectListItem {Value = "Y", Text = "Yearly"}
        };
	}
}
