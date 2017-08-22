using Newtonsoft.Json;

namespace Mvc.Server.Helpers
{
    public class CustomResponseError
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}