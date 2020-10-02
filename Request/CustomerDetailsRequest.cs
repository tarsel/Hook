using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    [DataContract]
    public class CustomerDetailsRequest
    {
        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("id_number")]
        public string IdNumber { get; set; }

        [JsonProperty("msisdn")]
        public long Msisdn { get; set; }
    }
}