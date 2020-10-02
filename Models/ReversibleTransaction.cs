using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class ReversibleTransaction
    {
        public int ReversibleTransactionId { get; set; }
        public int TransactionTypeId { get; set; }
        public bool IsReversible { get; set; }
        public int ReversiblePeriod { get; set; }
        public bool IsCumulativeAmountReversible { get; set; }
    }
}