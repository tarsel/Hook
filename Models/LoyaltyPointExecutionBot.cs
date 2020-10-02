using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class LoyaltyPointExecutionBot
    {
        public long LoyaltyPointExecutionBotId { get; set; }
        public DateTime DatePosted { get; set; }
        public long? DestinationCustomerId { get; set; }
        public long? DestinationPaymentInstrumentId { get; set; }
        public long? DestinationPointBalanceAfterTransaction { get; set; }
        public long? DestinationPointBalanceBeforeTransaction { get; set; }
        public bool IsLoyaltyBasedOnAmountSchemeExecuted { get; set; }
        public bool IsLoyaltyBasedOnCumulativeAmountSchemeExecuted { get; set; }
        public bool IsLoyaltyBasedOnFrequencySchemeExecuted { get; set; }
        public long? LoyaltyRedemptionRateId { get; set; }
        public long? MasterTransactionRecordId { get; set; }
        public long OrganizationId { get; set; }
        public string ReversedTransactionParticulars { get; set; }
        public long? SourceCustomerId { get; set; }
        public long? SourcePaymentInstrumentId { get; set; }
        public long? SourcePointBalanceAfterTransaction { get; set; }
        public long? SourcePointBalanceBeforeTransaction { get; set; }
        public long TransactionAmount { get; set; }
        public long TransactionFee { get; set; }
        public string TransactionReference { get; set; }
        public long TransactionTypeId { get; set; }

    }
}