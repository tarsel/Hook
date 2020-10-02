using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class PaymentInstrument
    {
        public long PaymentInstrumentId { get; set; }
        public long CustomerId { get; set; }
        public string PaymentIntrumentAlias { get; set; }
        public long PaymentInstrumentTypeId { get; set; }
        public string AccountNumber { get; set; }
        public long AccountBalance { get; set; }
        public string IdNumber { get; set; }
        public bool IsMobileWallet { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public DateTime DateLinked { get; set; }
        public bool Verified { get; set; }
        public DateTime? DateVerified { get; set; }
        public bool AllowDebit { get; set; }
        public bool AllowCredit { get; set; }
        public bool IsDefaultFIAccount { get; set; }
        public bool IsSuspended { get; set; }
        public bool Delinked { get; set; }
        public bool IsActive { get; set; }
        public string CardNumber { get; set; }
        public DateTime? CardExpiryDate { get; set; }
        public long LoyaltyPointBalance { get; set; }
    }
}