using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    [DataContract]
    public class TransactionTypeCategoryRequest
    {
        [JsonProperty("transaction_type_category_id")]
        public long TransactionTypeCategoryId { get; set; }
        [JsonProperty("transaction_type_id")]
        public long TransactionTypeId { get; set; }
        [JsonProperty("transaction_type_category_name")]
        public string TransactionTypeCategoryName { get; set; }
        [JsonProperty("amount")]
        public long Amount { get; set; }
    }
}