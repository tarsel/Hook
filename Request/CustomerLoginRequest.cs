using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// Customer Login Request
    /// </summary>
    [DataContract]
    public class CustomerLoginRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "pin must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("pin")]
        public string Pin { get; set; }

        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }
    }
}