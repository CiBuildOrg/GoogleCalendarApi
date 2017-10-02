using Newtonsoft.Json;

namespace Mvc.Server.DataObjects.Response
{
    public class CustomResponseError
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}