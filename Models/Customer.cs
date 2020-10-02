using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class Customer
    {
        public long CustomerId { get; set; }
        public long AccessChannelId { get; set; }
        public long BlacklistReasonId { get; set; }
        public long CustomerTypeId { get; set; }
        public bool DeactivatedAccount { get; set; }
        public string DeactivateMsisdns { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public bool FullyRegistered { get; set; }
        public string IdNumber { get; set; }
        public long IdTypeId { get; set; }
        public long InformationModeId { get; set; }
        public bool IsBlacklisted { get; set; }
        public bool IsStaff { get; set; }
        public bool IsTestCustomer { get; set; }
        public long LanguageId { get; set; }
        public string LastName { get; set; }
        public int LoginAttempts { get; set; }
        public string MiddleName { get; set; }
        public string Nonce { get; set; }
        public string PostalAddress { get; set; }
        public string RegisteredByUsername { get; set; }
        public string Salt { get; set; }
        public string SecurityCode { get; set; }
        public string TaxNumber { get; set; }
        public bool TermsAccepted { get; set; }
        public long TownId { get; set; }
        public bool UserLoggedIn { get; set; }
        public string UserName { get; set; }
        public long UserTypeId { get; set; }
        public long SubCountyId { get; set; }
        public DateTime? TermsAcceptedDate { get; set; }
        public DateTime? DeactivatedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public long Msisdn { get; set; }
    }
}