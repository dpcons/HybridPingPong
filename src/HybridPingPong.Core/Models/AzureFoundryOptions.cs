namespace HybridPingPong.Core.Models;

/// <summary>
/// Configuration options for the Azure AI Foundry endpoint (cloud LLM).
/// </summary>
public sealed class AzureFoundryOptions
{
    /// <summary>The Azure OpenAI endpoint URL.</summary>
    public string Endpoint { get; set; } = "";

    /// <summary>
    /// The authentication mode used to connect to the endpoint.
    /// Use <see cref="AzureFoundryAuthMode.Key"/> for API-key auth,
    /// or <see cref="AzureFoundryAuthMode.Identity"/> for Entra ID (service principal) auth.
    /// </summary>
    public AzureFoundryAuthMode AuthMode { get; set; } = AzureFoundryAuthMode.Key;

    /// <summary>The API key for authenticating with Azure OpenAI (used when <see cref="AuthMode"/> is <see cref="AzureFoundryAuthMode.Key"/>).</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>Entra ID tenant ID (used when <see cref="AuthMode"/> is <see cref="AzureFoundryAuthMode.Identity"/>).</summary>
    public string TenantId { get; set; } = "";

    /// <summary>Entra ID application (client) ID (used when <see cref="AuthMode"/> is <see cref="AzureFoundryAuthMode.Identity"/>).</summary>
    public string ClientId { get; set; } = "";

    /// <summary>Entra ID client secret (used when <see cref="AuthMode"/> is <see cref="AzureFoundryAuthMode.Identity"/>).</summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>The deployment name of the cloud model.</summary>
    public string Deployment { get; set; } = "gpt-4o-mini";

    /// <summary>Cost per 1 000 input tokens in USD.</summary>
    public decimal InputPricePer1K { get; set; } = 0.00015m;

    /// <summary>Cost per 1 000 output tokens in USD.</summary>
    public decimal OutputPricePer1K { get; set; } = 0.00060m;
}
