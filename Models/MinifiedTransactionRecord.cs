using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class MinifiedTransactionRecord
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdNumber { get; set; }
        public string TransactionTypeName { get; set; }
        public string AmountPaid { get; set; }
        public string PayerBalanceBeforeTransaction { get; set; }
        public string PayerBalanceAfterTransaction { get; set; }
        public string TownName { get; set; }
        public string SubCountyName { get; set; }
        public string TransactionDate { get; set; }
        public string TransactionTypeCategoryName { get; set; }
    }
}