using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Hook.Enums;

namespace Hook.Response
{
    [DataContract]
    public class MakeTransactionResponse
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
        [JsonProperty("source_username")]
        public string SourceUserName { get; set; }
        [JsonProperty("destination_username")]
        public string DestinationUserName { get; set; }
        [JsonProperty("transaction_status_id")]
        public TransactionState? TransactionStatusId { get; set; }
        [JsonProperty("third_party_transaction_id")]
        public string ThirdPartyTransactionId { get; set; }
        [JsonProperty("reversed_transaction_original_type_id")]
        public long? ReversedTransactionOriginalTypeId { get; set; }
        [JsonProperty("transaction_category_type_id")]
        public long TransactionTypeCategoryId { get; set; }
        [JsonProperty("business_registration_id")]
        public long BusinessRegistrationId { get; set; }
        [JsonProperty("town_id")]
        public long TownId { get; set; }
        [JsonProperty("sub_county_id")]
        public long SubcountyId { get; set; }
        [JsonProperty("key_identifier")]
        public string KeyIdentifier { get; set; }
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}