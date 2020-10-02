using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// Id Type Request
    /// </summary>
    [DataContract]
    public class IdTypeRequest
    {
        [JsonProperty("id_type_id")]
        public long IdTypeId { get; set; }
        [JsonProperty("id_type_name")]
        public string IdTypeName { get; set; }
        [JsonProperty("id_type_description")]
        public string IdTypeDescription { get; set; }
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