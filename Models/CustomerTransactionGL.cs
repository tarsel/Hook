using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class CustomerTransactionGL
    {
        public int CustomerTransactionGLId { get; set; }
        public long CustomerId { get; set; }
        public long MasterTransactionRecordId { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}