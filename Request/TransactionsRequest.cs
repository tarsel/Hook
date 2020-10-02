using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    [DataContract]
    public class TransactionsRequest
    {
        [JsonProperty("msisdn")]
        public long Msisdn { get; set; }
        [JsonProperty("id_number")]
        public string IdNumber { get; set; }
        [JsonProperty("transaction_type_id")]
        public long TransactionTypeId { get; set; }
        [JsonProperty("ward_id")]
        public long WardId { get; set; }
        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }
        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }
    }
}