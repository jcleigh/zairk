using System.Text.Json.Serialization;

namespace ZAIrk.Models.Ollama;

/// <summary>
/// Request model for Ollama chat API
/// </summary>
public class OllamaRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";
    
    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; set; } = new();
    
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
    
    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }
}

/// <summary>
/// Message model for Ollama API
/// </summary>
public class OllamaMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

/// <summary>
/// Options for Ollama API
/// </summary>
public class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }
    
    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; set; }
}