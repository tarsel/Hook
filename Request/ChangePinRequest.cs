using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Request
{
    /// <summary>
    /// Change Pin Request
    /// </summary>
    [DataContract]
    public class ChangePinRequest
    {
        [Required(ErrorMessage = "msisdn must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }

        [Required(ErrorMessage = "old_pin must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("old_pin")]
        public string OldPin { get; set; }

        [Required(ErrorMessage = "new_pin must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("new_pin")]
        public string NewPin { get; set; }
    }
}