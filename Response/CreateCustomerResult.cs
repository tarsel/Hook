using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class CreateCustomerResult
    {
        [JsonProperty("created_successfully")]
        public bool CreatedSuccessfully { get; set; }

        [JsonProperty("response_error")]
        public ResponseError ResponseError { get; set; }

        [JsonProperty("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
    }
}