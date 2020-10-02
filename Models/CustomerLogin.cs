using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class CustomerLogin
    {
        public long CustomerId { get; set; }
        public bool IsTestCustomer { get; set; }
        public bool ValidPassword { get; set; }
        public int UserTypeId { get; set; }
        public string ApplicationSessionString { get; set; }
        public string Error { get; set; }
        public bool IdFieldPopulated { get; set; }
        public bool TermsAccepted { get; set; }
        public bool OTPChanged { get; set; }
        public bool UserLoggedIn { get; set; }
        public string Action { get; set; }
        public int RemainingLoginAttempts { get; set; }
        public string ReferalCode { get; set; }
        public string RefererReferalCode { get; set; }
        public string FullNames { get; set; }
        public long Msisdn { get; set; }
    }
}