using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    [DataContract]
    public class MakeTransactionRequest
    {
        [Required(ErrorMessage = "customer_id must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }

        [Required(ErrorMessage = "transaction_type_id must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("transaction_type_id")]
        public long TransactionTypeId { get; set; }

        [Required(ErrorMessage = "transaction_type_category_id must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("transaction_type_category_id")]
        public long TransactionTypeCategoryId { get; set; }

        [Required(ErrorMessage = "transaction_amount must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("transaction_amount")]
        public long TransactionAmount { get; set; }

        [Required(ErrorMessage = "customer_type_id must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("customer_type_id")]
        public long CustomerTypeId { get; set; }

        [Required(ErrorMessage = "key_identifier must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("key_identifier")]
        public string KeyIdentifier { get; set; }

        [JsonProperty("sub_county_id")]
        public long SubCountyId { get; set; }

        [JsonProperty("town_id")]
        public long TownId { get; set; }

        [Required(ErrorMessage = "id_number must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }

        [Required(ErrorMessage = "email must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("zone_id")]
        public long ZoneId { get; set; }
    }
}