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
        [JsonProperty("payment_instrument_id")]
        public long PaymentInstrumentId { get; set; }
    }
}