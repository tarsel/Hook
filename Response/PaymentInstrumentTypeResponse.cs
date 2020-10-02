using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class PaymentInstrumentTypeResponse
    {
        [JsonProperty("payment_instrument_type_id")]
        public long PaymentInstrumentTypeId { get; set; }
        [JsonProperty("payment_instrument_type_name")]
        public string PaymentInstrumentTypeName { get; set; }
        [JsonProperty("is_wallet")]
        public bool IsWallet { get; set; }
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}