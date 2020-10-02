using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class CustomerDetailsResponse
    {
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("access_channel_id")]
        public long AccessChannelId { get; set; }
        [JsonProperty("blacklist_reason_id")]
        public long BlacklistReasonId { get; set; }
        [JsonProperty("customer_type_id")]
        public long CustomerTypeId { get; set; }
        [JsonProperty("deactivated_account")]
        public bool DeactivatedAccount { get; set; }
        [JsonProperty("deactivate_msisdns")]
        public string DeactivateMsisdns { get; set; }
        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("fully_registered")]
        public bool FullyRegistered { get; set; }
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }
        [JsonProperty("id_type_id")]
        public long IdTypeId { get; set; }
        [JsonProperty("information_mode_id")]
        public long InformationModeId { get; set; }
        [JsonProperty("is_blacklisted")]
        public bool IsBlacklisted { get; set; }
        [JsonProperty("is_staff")]
        public bool IsStaff { get; set; }
        [JsonProperty("is_test_customer")]
        public bool IsTestCustomer { get; set; }
        [JsonProperty("language_id")]
        public long LanguageId { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("login_attempts")]
        public int LoginAttempts { get; set; }
        [JsonProperty("middle_name")]
        public string MiddleName { get; set; }
        [JsonProperty("postal_address")]
        public string PostalAddress { get; set; }
        [JsonProperty("registered_by_username")]
        public string RegisteredByUsername { get; set; }
        [JsonProperty("tax_number")]
        public string TaxNumber { get; set; }
        [JsonProperty("terms_accepted")]
        public bool TermsAccepted { get; set; }
        [JsonProperty("town_id")]
        public long TownId { get; set; }
        [JsonProperty("user_name")]
        public string UserName { get; set; }
        [JsonProperty("user_type_id")]
        public long UserTypeId { get; set; }
        [JsonProperty("sub_county_id")]
        public long SubCountyId { get; set; }
        [JsonProperty("terms_accepted_date")]
        public DateTime? TermsAcceptedDate { get; set; }
        [JsonProperty("deactivated_date")]
        public DateTime? DeactivatedDate { get; set; }
        [JsonProperty("created_date")]
        public DateTime CreatedDate { get; set; }
        [JsonProperty("msisdn")]
        public long Msisdn { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}