using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class PaymentInstrumentResponse
    {
        [JsonProperty("payment_instrument_id")]
        public long PaymentInstrumentId { get; set; }
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("payment_intrument_alias")]
        public string PaymentIntrumentAlias { get; set; }
        [JsonProperty("payment_instrument_type_id")]
        public long PaymentInstrumentTypeId { get; set; }
        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }
        [JsonProperty("account_balance")]
        public long AccountBalance { get; set; }
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }
        [JsonProperty("is_mobile_wallet")]
        public bool IsMobileWallet { get; set; }
        [JsonProperty("branch_name")]
        public string BranchName { get; set; }
        [JsonProperty("BranchCode")]
        public string BranchCode { get; set; }
        [JsonProperty("date_linked")]
        public DateTime DateLinked { get; set; }
        [JsonProperty("verified")]
        public bool Verified { get; set; }
        [JsonProperty("date_verified")]
        public DateTime? DateVerified { get; set; }
        [JsonProperty("allow_debit")]
        public bool AllowDebit { get; set; }
        [JsonProperty("allow_credit")]
        public bool AllowCredit { get; set; }
        [JsonProperty("is_default_fI_account")]
        public bool IsDefaultFIAccount { get; set; }
        [JsonProperty("is_suspended")]
        public bool IsSuspended { get; set; }
        [JsonProperty("delinked")]
        public bool Delinked { get; set; }
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
        [JsonProperty("card_number")]
        public string CardNumber { get; set; }
        [JsonProperty("card_expiry_date")]
        public DateTime? CardExpiryDate { get; set; }
        [JsonProperty("loyalty_point_balance")]
        public long LoyaltyPointBalance { get; set; }
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}