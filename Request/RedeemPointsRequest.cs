using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Hook.Request
{
    /// <summary>
    /// Redeem Points Request
    /// </summary>
    [DataContract]
    public class RedeemPointsRequest
    {
        [JsonProperty("organization_id")]
        public int OrganizationId { get; set; }
        [JsonProperty("payment_instrument_id")]
        public long PaymentInstrumentId { get; set; }
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("points_to_redeem")]
        public long PointsToRedeem { get; set; }
    }
}