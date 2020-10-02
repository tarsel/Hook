using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Hook.Response
{
    /// <summary>
    /// Customer Loyalty Point Response
    /// </summary>
    [DataContract]
    public class CustomerLoyaltyPointResponse
    {
        [JsonProperty("customer_loyalty_point_id")]
        public long CustomerLoyaltyPointId { get; set; }
        [JsonProperty("cumulative_fee_amount")]
        public long CumulativeFeeAmount { get; set; }
        [JsonProperty("cumulative_points")]
        public long CumulativePoints { get; set; }
        [JsonProperty("cumulative_transaction_amount")]
        public long CumulativeTransactionAmount { get; set; }
        [JsonProperty("is_frozen")]
        public bool IsFrozen { get; set; }
        [JsonProperty("organization_id")]
        public int OrganizationId { get; set; }
        [JsonProperty("payment_instrument_id")]
        public long PaymentInstrumentId { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public String Message { get; set; }
    }
}