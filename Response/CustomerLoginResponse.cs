using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Hook.Response
{
    [DataContract]
    public class CustomerLoginResponse
    {
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("is_test_customer")]
        public bool IsTestCustomer { get; set; }
        [JsonProperty("valid_password")]
        public bool ValidPassword { get; set; }
        [JsonProperty("user_type_id")]
        public int UserTypeId { get; set; }
        [JsonProperty("application_session_string")]
        public string ApplicationSessionString { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("id_field_populated")]
        public bool IdFieldPopulated { get; set; }
        [JsonProperty("terms_accepted")]
        public bool TermsAccepted { get; set; }
        /// <summary>
        /// One Time Pin (The value if changed)
        /// </summary>
        [JsonProperty("otp_changed")]
        public bool OTPChanged { get; set; }
        [JsonProperty("user_logged_in")]
        public bool UserLoggedIn { get; set; }
        [JsonProperty("action")]
        public string Action { get; set; }
        [JsonProperty("remaining_login_attempts")]
        public int RemainingLoginAttempts { get; set; }
        [JsonProperty("referer_referal_code")]
        public string RefererReferalCode { get; set; }
        [JsonProperty("referal_code")]
        public string ReferalCode { get; set; }
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("full_names")]
        public string FullNames { get; set; }
        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}