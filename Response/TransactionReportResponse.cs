using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class TransactionReportResponse
    {
        [JsonProperty("master_transaction_record_id")]
        public long MasterTransactionRecordId { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }
        [JsonProperty("msisdn")]
        public long Msisdn { get; set; }
        [JsonProperty("transaction_reference")]
        public string TransactionReference { get; set; }
        [JsonProperty("transaction_type_name")]
        public string TransactionTypeName { get; set; }
        [JsonProperty("transaction_date")]
        public DateTime TransactionDate { get; set; }
        [JsonProperty("payer_balance_before_transaction")]
        public long PayerBalanceBeforeTransaction { get; set; }
        [JsonProperty("payer_balance_after_transaction")]
        public long PayerBalanceAfterTransaction { get; set; }
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}