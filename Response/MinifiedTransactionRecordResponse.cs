using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class MinifiedTransactionRecordResponse
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }
        [JsonProperty("transaction_type_name")]
        public string TransactionTypeName { get; set; }
        [JsonProperty("amount_paid")]
        public string AmountPaid { get; set; }
        [JsonProperty("payer_balance_before_transaction")]
        public string PayerBalanceBeforeTransaction { get; set; }
        [JsonProperty("payer_balance_after_transaction")]
        public string PayerBalanceAfterTransaction { get; set; }
        [JsonProperty("town_name")]
        public string TownName { get; set; }
        [JsonProperty("sub_county_name")]
        public string SubCountyName { get; set; }
        [JsonProperty("transaction_date")]
        public string TransactionDate { get; set; }
        [JsonProperty("transaction_type_category_name")]
        public string TransactionTypeCategoryName { get; set; }
    }
}