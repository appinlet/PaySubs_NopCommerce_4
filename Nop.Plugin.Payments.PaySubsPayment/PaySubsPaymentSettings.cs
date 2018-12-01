using Nop.Core.Configuration;
using System;

namespace Nop.Plugin.Payments.PaySubs
{
    public class PaySubsPaymentSettings : ISettings
    {
        public decimal AdditionalFee
        {
            get;
            set;
        }

        public string HashParameter
        {
            get;
            set;
        }

        public string Instruction
        {
            get;
            set;
        }

        public string TerminalId
        {
            get;
            set;
        }

        public bool RecurringPayments
        {
            get;
            set;
        }

        public string RecurrenceFrequency
        {
            get;
            set;
        }
    }
}