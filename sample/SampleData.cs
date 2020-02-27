using System.Text.Json.Serialization;

namespace Azure.Functions.Cosmos.Extension.Sample
{
    public class SampleData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        public string Discriminator => nameof(SampleData);
    }
}