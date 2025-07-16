using System.Text.Json.Serialization;

namespace MyMCPWebAPIServer.EmbeddingGenerator;

public class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = [];
}