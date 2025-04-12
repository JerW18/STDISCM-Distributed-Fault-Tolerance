using System.Text.Json.Serialization;

namespace P4___Distributed_Fault_Tolerance.Models
{
    public class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("idNumber")]
        public string IdNumber { get; set; }
    }
}