using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// Generic Request
    /// </summary>
    [DataContract]
    public class GenericRequest
    {
        [JsonProperty("town_id")]
        public long TownId { get; set; }
        [JsonProperty("sub_county_id")]
        public long SubCountyId { get; set; }
    }
}