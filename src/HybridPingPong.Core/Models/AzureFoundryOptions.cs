namespace HybridPingPong.Core.Models;

/// <summary>
/// Configuration options for the Azure AI Foundry endpoint (cloud LLM).
/// </summary>
public sealed class AzureFoundryOptions
{
    /// <summary>The Azure OpenAI endpoint URL.</summary>
    public string Endpoint { get; set; } = "";

    /// <summary>The API key for authenticating with Azure OpenAI.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>The deployment name of the cloud model.</summary>
    public string Deployment { get; set; } = "gpt-4o-mini";

    /// <summary>Cost per 1 000 input tokens in USD.</summary>
    public decimal InputPricePer1K { get; set; } = 0.00015m;

    /// <summary>Cost per 1 000 output tokens in USD.</summary>
    public decimal OutputPricePer1K { get; set; } = 0.00060m;
}
