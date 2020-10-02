using System;

using Hook.Enums;

namespace Hook.Models
{
    public class MasterTransactionRecord
    {
        public long MasterTransactionRecordId { get; set; }
        public long PayerId { get; set; }
        public long PayerPaymentInstrumentId { get; set; }
        public long PayeeId { get; set; }
        public long PayeePaymentInstrumentId { get; set; }
        public string TransactionReference { get; set; }
        public string ShortDescription { get; set; }
        public long TransactionTypeId { get; set; }
        public long TransactionErrorCodeId { get; set; }
        public long Amount { get; set; }
        public long Fee { get; set; }
        public long Tax { get; set; }
        public bool IsBankingTransaction { get; set; }
        public string FITransactionCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public long CustomerTypeId { get; set; }
        public long? PayerBalanceBeforeTransaction { get; set; }
        public long? PayerBalanceAfterTransaction { get; set; }
        public long? PayeeBalanceBeforeTransaction { get; set; }
        public long? PayeeBalanceAfterTransaction { get; set; }
        public bool IsTestTransaction { get; set; }
        public long AccessChannelId { get; set; }
        public long ExternalApplicationId { get; set; }
        public string SourceUserName { get; set; }
        public string DestinationUserName { get; set; }
        public TransactionState? TransactionStatusId { get; set; }
        public string ThirdPartyTransactionId { get; set; }
        public long? ReversedTransactionOriginalTypeId { get; set; }
        public string Text { get; set; }
        public string Text2 { get; set; }
        public string Text3 { get; set; }
        public string Text4 { get; set; }
        public string Text5 { get; set; }
        public string Text6 { get; set; }
        public string Text7 { get; set; }
        public long RefererCustomerId { get; set; }
    }
}