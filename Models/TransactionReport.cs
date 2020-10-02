using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class TransactionReport
    {
        public long MasterTransactionRecordId { get; set; }
        public string FirstName { get; set; }
        public string IdNumber { get; set; }
        public long Msisdn { get; set; }
        public string TransactionReference { get; set; }
        public string TransactionTypeName { get; set; }
        public DateTime TransactionDate { get; set; }
        public long PayerBalanceBeforeTransaction { get; set; }
        public long PayerBalanceAfterTransaction { get; set; }
    }
}