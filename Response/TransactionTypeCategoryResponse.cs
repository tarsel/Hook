using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class TransactionTypeCategoryResponse
    {
        [JsonProperty("transaction_type_category_id")]
        public long TransactionTypeCategoryId { get; set; }
        [JsonProperty("transaction_type_id")]
        public long TransactionTypeId { get; set; }
        [JsonProperty("transaction_type_category_name")]
        public string TransactionTypeCategoryName { get; set; }
        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }
        [JsonProperty("amount")]
        public long Amount { get; set; }
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}