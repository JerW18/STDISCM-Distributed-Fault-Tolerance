using System.Text.Json.Serialization;

namespace P4___Distributed_Fault_Tolerance.Models
{
    public class ProfModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }

}
