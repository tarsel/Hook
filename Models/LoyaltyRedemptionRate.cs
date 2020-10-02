using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hook.Models
{
    public class LoyaltyRedemptionRate
    {
        public int LoyaltyRedemptionRateId { get; set; }
        public string CreatedByUsername { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int OrganizationId { get; set; }
        public int PaymentInstrumentTypeId { get; set; }
        public long? PointFrequency { get; set; }
        public long? RedemptionAmountEquivalent { get; set; }
        public decimal? RedemptionPercentageEquivalent { get; set; }
        public string UpdatedByUsername { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}