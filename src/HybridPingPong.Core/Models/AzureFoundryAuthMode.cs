namespace HybridPingPong.Core.Models;

/// <summary>
/// Authentication mode used to connect to the Azure AI Foundry endpoint.
/// </summary>
public enum AzureFoundryAuthMode
{
    /// <summary>Authenticate with an API key (shared secret).</summary>
    Key,

    /// <summary>Authenticate with an Entra ID (Azure AD) service principal.</summary>
    Identity
}
