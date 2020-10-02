using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Hook.Enums;

namespace Hook.Request
{
    [DataContract]
    public class PaymentInstrumentTypeRequest
    {
        [JsonProperty("payment_instrument_type_id")]
        public long PaymentInstrumentTypeId { get; set; }
        [JsonProperty("payment_instrument_type_name")]
        public string PaymentInstrumentTypeName { get; set; }
    }
}