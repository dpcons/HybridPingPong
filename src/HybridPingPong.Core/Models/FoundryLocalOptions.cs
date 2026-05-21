namespace HybridPingPong.Core.Models;

/// <summary>
/// Configuration options for the local Foundry endpoint (SLM).
/// </summary>
public sealed class FoundryLocalOptions
{
    /// <summary>The OpenAI-compatible endpoint URL for the local model.</summary>
    public string Endpoint { get; set; } = "http://localhost:5273/v1";

    /// <summary>The local model identifier.</summary>
    public string Model { get; set; } = "phi-4-mini";

    /// <summary>API key for the local endpoint (typically not required).</summary>
    public string ApiKey { get; set; } = "not-needed";
}
