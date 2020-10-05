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
        [JsonProperty("amount")]
        public string Amount { get; set; }
        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }
    }
}