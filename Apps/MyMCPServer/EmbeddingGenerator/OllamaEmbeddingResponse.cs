using System.Text.Json.Serialization;

namespace MyMCPServer.EmbeddingGenerator;

public class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = [];
}