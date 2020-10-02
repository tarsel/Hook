using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Enums
{
    public enum TransactionTypes
    {
        AirtimeTopup = 1,
        AirtimeSale = 2,
        WithholdingTax = 3,
        CommissionToOwnAccount = 4,
        CommissionToAgent = 5,
        CommissionAdjustment = 6,
        LoyaltyPointRedemption = 7,
        LoyaltyPointTransfer = 8,
        LoyaltyPointBalanceEnquiry = 9,
        ReversalTransaction = 10,
        ChangePin = 11,
        BalanceEnquiry = 12,
        PinReset = 13,
        ChangeLanguage = 14,
        StatementRequest = 15,
        ResetSecurityCode = 16,
    }
}