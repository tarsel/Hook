using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    [DataContract]
    public class PaymentInstrumentRequest
    {
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("amount")]
        public long Amount { get; set; }
    }
}