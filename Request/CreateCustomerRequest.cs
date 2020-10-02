using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    [DataContract]
    public class CreateCustomerRequest
    {
        [Required(ErrorMessage = "first_name must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "last_name must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("middle_name")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "msisdn must be provided!")]
        [DataMember(IsRequired = true)]
        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }

        [JsonProperty("id_type_id")]
        public int IdTypeId { get; set; }

        [Required(ErrorMessage = "id_number must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "is_test_customer must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("is_test_customer")]
        public bool IsTestCustomer { get; set; }

        [JsonProperty("customer_type_id")]
        public int CustomerTypeId { get; set; }

        [JsonProperty("user_type_id")]
        public int UserTypeId { get; set; }

        [JsonProperty("shared_msisdn")]
        public bool SharedMsisdn { get; set; }

        [Required(ErrorMessage = "user_name must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "fully_registered must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("fully_registered")]
        public bool FullyRegistered { get; set; }

        [JsonProperty("language_id")]
        public int LanguageId { get; set; }

        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "registered_by_userName must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("registered_by_userName")]
        public string RegisteredByUserName { get; set; }
    }
}