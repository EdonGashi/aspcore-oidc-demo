using System.Collections.Generic;
using Newtonsoft.Json;

namespace AuthServer.V1.Models
{
    public class UserResult
    {
        [JsonProperty("sub")]
        public string Subject { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }
}
