using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// Check Points Balance Request
    /// </summary>
    [DataContract]
    public class CheckPointsBalanceRequest
    {
        [JsonProperty("organization_id")]
        public int OrganizationId { get; set; }
        [JsonProperty("payment_instrument_id")]
        public long PaymentInstrumentId { get; set; }
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
    }
}