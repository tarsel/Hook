using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// Sub County Request
    /// </summary>
    [DataContract]
    public class SubCountyRequest
    {
        [JsonProperty("sub_county_id")]
        public long SubCountyId { get; set; }
        [JsonProperty("sub_county_name")]
        public string SubCountyName { get; set; }
        [JsonProperty("sub_county_description")]
        public string SubCountyDescription { get; set; }
        [JsonProperty("date_created")]
        public DateTime DateCreated { get; set; }
        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }
        [JsonProperty("date_updated")]
        public DateTime DateUpdated { get; set; }
        [JsonProperty("updated_by")]
        public string UpdatedBy { get; set; }
    }
}