using System.Text.Json.Serialization;

namespace MyMCPWebAPIServer.Models;

public class OllamaCompletionResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public List<int> Context { get; set; } = [];
}