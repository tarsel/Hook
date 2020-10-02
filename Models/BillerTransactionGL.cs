using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class BillerTransactionGL
    {
        public long BillerTransactionGLId { get; set; }
        public long BillerId { get; set; }
        public long MasterTransactionRecordId { get; set; }
        public long PaymentInstrumentId { get; set; }
        public DateTime TransactionDate { get; set; }
        public long Amount { get; set; }
        public long CommssionEarned { get; set; }
    }
}