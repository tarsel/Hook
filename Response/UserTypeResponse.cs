using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    /// <summary>
    /// User Type Response
    /// </summary>
    [DataContract]
    public class UserTypeResponse
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
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}