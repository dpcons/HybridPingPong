using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace HybridPingPong.Core.Services;

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

/// <summary>
/// Provides DI registration for the local and cloud <see cref="IChatClient"/> backends.
/// </summary>
public static class ChatBackends
{
    /// <summary>Keyed service name for the local chat client.</summary>
    public const string LocalKey = "local";

    /// <summary>Keyed service name for the cloud chat client.</summary>
    public const string CloudKey = "cloud";

    /// <summary>
    /// Registers the local and cloud <see cref="IChatClient"/> instances as keyed singletons,
    /// along with their configuration options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="cfg">The application configuration containing FoundryLocal and AzureFoundry sections.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHybridChatBackends(this IServiceCollection services, 
        IConfiguration cfg)
    {
        services.Configure<FoundryLocalOptions>(cfg.GetSection("FoundryLocal"));
        services.Configure<AzureFoundryOptions>(cfg.GetSection("AzureFoundry"));

        services.AddKeyedSingleton<IChatClient>(LocalKey, (_ , _) =>
        {
            var o = cfg.GetSection("FoundryLocal").Get<FoundryLocalOptions>() ?? new();

            var client = new OpenAIClient(
                new ApiKeyCredential(string.IsNullOrEmpty(o.ApiKey) ? "not-needed" : o.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(o.Endpoint) });
            return client.GetChatClient(o.Model).AsIChatClient();
        });

        services.AddKeyedSingleton<IChatClient>(CloudKey, (_, _) =>
        {
            var o = cfg.GetSection("AzureFoundry").Get<AzureFoundryOptions>() ?? new();
            if (string.IsNullOrWhiteSpace(o.Endpoint) || string.IsNullOrWhiteSpace(o.ApiKey))
                throw new InvalidOperationException(
                    "AzureFoundry:Endpoint and AzureFoundry:ApiKey must be configured in appsettings to use the cloud route.");
            var az = new AzureOpenAIClient(new Uri(o.Endpoint), new AzureKeyCredential(o.ApiKey));
            return az.GetChatClient(o.Deployment).AsIChatClient();
        });

        return services;
    }
}
