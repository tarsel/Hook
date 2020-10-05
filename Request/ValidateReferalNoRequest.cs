using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Hook.Request
{
    /// <summary>
    /// Validate Referal No Request
    /// </summary>
    [DataContract]
    public class ValidateReferalNoRequest
    {
        [Required(ErrorMessage = "referer_ref_number must be provided")]
        [DataMember(IsRequired = true)]
        [JsonProperty("referer_ref_number")]
        public string RefererRefNumber { get; set; }
    }
}