using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class CustomerLoyaltyPoint
    {
        public long CustomerLoyaltyPointId { get; set; }
        public long CumulativeFeeAmount { get; set; }
        public long CumulativePoints { get; set; }
        public long CumulativeTransactionAmount { get; set; }
        public bool IsFrozen { get; set; }
        public int OrganizationId { get; set; }
        public long PaymentInstrumentId { get; set; }
        public long CustomerId { get; set; }
    }
}