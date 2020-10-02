using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// User Type Request
    /// </summary>
    [DataContract]
    public class UserTypeRequest
    {
        [JsonProperty("user_type_id")]
        public long UserTypeId { get; set; }
        [JsonProperty("user_type_name")]
        public string UserTypeName { get; set; }
        [JsonProperty("user_type_description")]
        public string UserTypeDescription { get; set; }
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