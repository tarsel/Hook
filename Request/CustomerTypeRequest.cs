﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// Customer Type Request
    /// </summary>
    [DataContract]
    public class CustomerTypeRequest
    {
        [JsonProperty("customer_type_id")]
        public long CustomerTypeId { get; set; }
        [JsonProperty("customer_type_name")]
        public string CustomerTypeName { get; set; }
        [JsonProperty("customer_type_description")]
        public string CustomerTypeDescription { get; set; }
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