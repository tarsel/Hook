using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class TransactionTypeCategory
    {
        public long TransactionTypeCategoryId { get; set; }
        public long TransactionTypeId { get; set; }
        public string TransactionTypeCategoryName { get; set; }
        public string FriendlyName { get; set; }
        public long Amount { get; set; }
        public bool IsActive { get; set; }
    }
}