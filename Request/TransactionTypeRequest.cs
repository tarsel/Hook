using System.Runtime.Serialization;

using Newtonsoft.Json;


namespace Hook.Request
{
    [DataContract]
    public class TransactionTypeRequest
    {
        [JsonProperty("transaction_type_id")]
        public long TransactionTypeId { get; set; }
        [JsonProperty("transaction_type_name")]
        public string TransactionTypeName { get; set; }
        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }
        [JsonProperty("amount")]
        public long Amount { get; set; }
    }
}