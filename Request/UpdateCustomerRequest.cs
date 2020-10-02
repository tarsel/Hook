using System.Runtime.Serialization;

using Newtonsoft.Json;


namespace Hook.Request
{
    [DataContract]
    public class UpdateCustomerRequest
    {
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("access_channel_id")]
        public long AccessChannelId { get; set; }
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
        [JsonProperty("middle_name")]
        public string MiddleName { get; set; }
        [JsonProperty("postal_address")]
        public string PostalAddress { get; set; }
        [JsonProperty("tax_number")]
        public string TaxNumber { get; set; }
        [JsonProperty("town_id")]
        public long TownId { get; set; }
        [JsonProperty("user_type_id")]
        public long UserTypeId { get; set; }
        [JsonProperty("sub_county_id")]
        public long SubCountyId { get; set; }
        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }
        [JsonProperty("updated_by")]
        public string UpdatedBy { get; set; }
    }
}