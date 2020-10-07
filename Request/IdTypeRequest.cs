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
    }
}