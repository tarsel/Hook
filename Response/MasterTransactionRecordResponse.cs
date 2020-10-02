using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Hook.Enums;

namespace Hook.Response
{
    /// <summary>
    /// Master Transaction Record Response
    /// </summary>
    [DataContract]
    public class MasterTransactionRecordResponse
    {
        [JsonProperty("master_transaction_record_id")]
        public long MasterTransactionRecordId { get; set; }
        [JsonProperty("payer_id")]
        public long PayerId { get; set; }
        [JsonProperty("payer_payment_instrument_id")]
        public long PayerPaymentInstrumentId { get; set; }
        [JsonProperty("payee_id")]
        public long PayeeId { get; set; }
        [JsonProperty("payee_payment_instrument_id")]
        public long PayeePaymentInstrumentId { get; set; }
        [JsonProperty("transaction_reference")]
        public string TransactionReference { get; set; }
        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }
        [JsonProperty("transaction_type_id")]
        public long TransactionTypeId { get; set; }
        [JsonProperty("transaction_error_code_id")]
        public long TransactionErrorCodeId { get; set; }
        [JsonProperty("amount")]
        public long Amount { get; set; }
        [JsonProperty("fee")]
        public long Fee { get; set; }
        [JsonProperty("tax")]
        public long Tax { get; set; }
        [JsonProperty("is_banking_transaction")]
        public bool IsBankingTransaction { get; set; }
        [JsonProperty("fi_transaction_code")]
        public string FITransactionCode { get; set; }
        [JsonProperty("transaction_date")]
        public DateTime TransactionDate { get; set; }
        [JsonProperty("customer_type_id")]
        public long CustomerTypeId { get; set; }
        [JsonProperty("payer_balance_before_transaction")]
        public long? PayerBalanceBeforeTransaction { get; set; }
        [JsonProperty("payer_balance_after_transaction")]
        public long? PayerBalanceAfterTransaction { get; set; }
        [JsonProperty("payee_balance_before_transaction")]
        public long? PayeeBalanceBeforeTransaction { get; set; }
        [JsonProperty("payee_balance_after_transaction")]
        public long? PayeeBalanceAfterTransaction { get; set; }
        [JsonProperty("is_test_transaction")]
        public bool IsTestTransaction { get; set; }
        [JsonProperty("access_channel_id")]
        public long AccessChannelId { get; set; }
        [JsonProperty("external_application_id")]
        public long ExternalApplicationId { get; set; }
        [JsonProperty("source_user_name")]
        public string SourceUserName { get; set; }
        [JsonProperty("destination_user_name")]
        public string DestinationUserName { get; set; }
        [JsonProperty("transaction_status_id")]
        public TransactionState? TransactionStatusId { get; set; }
        [JsonProperty("third_party_transaction_id")]
        public string ThirdPartyTransactionId { get; set; }
        [JsonProperty("reversed_transaction_original_type_id")]
        public long? ReversedTransactionOriginalTypeId { get; set; }
        //[JsonProperty("status_code")]
        //public string Text { get; set; }
        //[JsonProperty("status_code")]
        //public string Text2 { get; set; }
        //[JsonProperty("status_code")]
        //public string Text3 { get; set; }
        //[JsonProperty("status_code")]
        //public string Text4 { get; set; }
        //[JsonProperty("status_code")]
        //public string Text5 { get; set; }
        //[JsonProperty("status_code")]
        //public string Text6 { get; set; }
        //public string Text7 { get; set; }
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}