using System.Runtime.Serialization;

namespace Hook.Enums
{
    [DataContract]
    public enum ReverseTransactionResult
    {
        [EnumMember(Value = "1")]
        TransactionBookedForReversal = 1,
        [EnumMember(Value = "2")]
        TransactionNotReversible = 2,
        [EnumMember(Value = "3")]
        NoSuchTransaction = 3,
        [EnumMember(Value = "4")]
        ReversalTimespanExceeded = 4,
        [EnumMember(Value = "5")]
        TransactionReversalDisabled = 5,
        [EnumMember(Value = "6")]
        TransactionCannotBeReversedTwice = 6,
        [EnumMember(Value = "7")]
        NotABankingTransaction = 7,
        [EnumMember(Value = "8")]
        TransactionWasNotSuccessful = 8,
    }
}
