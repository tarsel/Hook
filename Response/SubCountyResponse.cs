using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    /// <summary>
    /// Sub County Response
    /// </summary>
    [DataContract]
    public class SubCountyResponse
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
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}