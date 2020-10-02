using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class CumulativeCustomerTransactionAmount
    {
        public long CumulativeCustomerTransactionAmountId { get; set; }
        public long CumulativeDailyAmount { get; set; }
        public long CumulativeMonthlyAmount { get; set; }
        public long CustomerId { get; set; }
        public long PaymentInstrumentId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int TransactionMonth { get; set; }
    }
}