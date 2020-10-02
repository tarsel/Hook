using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Hook.Response
{
    [DataContract]
    public class GenericResponse
    {
        [JsonProperty("is_successful")]
        public bool IsSuccessful { get; set; }
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}