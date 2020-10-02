using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Hook.Request
{
    /// <summary>
    /// Buy Airtime Request
    /// </summary>
    /// 
    [DataContract]
    public class BuyAirtimeRequest
    {
        [JsonProperty("organization_id")]
        public int OrganizationId { get; set; }
        [JsonProperty("payment_instrument_id")]
        public long PaymentInstrumentId { get; set; }
        [JsonProperty("amount")]
        public long Amount { get; set; }
        [JsonProperty("msisdn")]
        public long Msisdn { get; set; }
    }
}