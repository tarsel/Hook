using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    [DataContract]
    public class PinRequest
    {
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }

        [JsonProperty("pin")]
        public string Pin { get; set; }
    }
}