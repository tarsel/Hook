using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class Biller
    {
        public bool AllowRegularReversal { get; set; }
        public string BillerEmailAddress { get; set; }
        public int BillerId { get; set; }
        public string BillerName { get; set; }
        public string BillerNumber { get; set; }
        public int BillerTypeId { get; set; }
        public int BlackListReasonId { get; set; }
        public string BusinessLocation { get; set; }
        public string CreatedByUsername { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CustomerContactId { get; set; }
        public long CustomerId { get; set; }
        public string CustomSMSTemplateName { get; set; }
        public string DeactivatedBillerNumber { get; set; }
        public bool EarnAirtimeCommission { get; set; }
        public bool HasCustomMenu { get; set; }
        public bool IpnActive { get; set; }
        public string IpnUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsEscrowAccount { get; set; }
        public long NotificationCustomerId { get; set; }
        public bool NotifyCustomersOnResume { get; set; }
        public long OperationPaymentInstrumentId { get; set; }
        public int? OrganizationId { get; set; }
        public long OverflowPaymentInstrumentId { get; set; }
        public bool ServiceOnline { get; set; }
        public bool ShowInUI { get; set; }
        public long SubsidizedAmount { get; set; }
        public int SupportApplicationId { get; set; }
        public DateTime TerminationDate { get; set; }
        public bool UsesCustomSMSTemplate { get; set; }
        public bool UsesMultiStepSMSProcessing { get; set; }
    }
}