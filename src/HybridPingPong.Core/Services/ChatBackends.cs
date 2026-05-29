using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using HybridPingPong.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace HybridPingPong.Core.Services;

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
    /// <remarks>
    /// The cloud client honours the <see cref="AzureFoundryOptions.AuthMode"/> setting:
    /// <list type="bullet">
    ///   <item><description><see cref="AzureFoundryAuthMode.Key"/>: requires <c>Endpoint</c> and <c>ApiKey</c>.</description></item>
    ///   <item><description><see cref="AzureFoundryAuthMode.Identity"/>: requires <c>Endpoint</c>, <c>TenantId</c>, <c>ClientId</c> and <c>ClientSecret</c> for an Entra ID service principal.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="cfg">The application configuration containing the <c>FoundryLocal</c> and <c>AzureFoundry</c> sections.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown at resolution time when the <c>AzureFoundry</c> section is missing the properties required
    /// by the selected <see cref="AzureFoundryOptions.AuthMode"/>.
    /// </exception>
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
            if (string.IsNullOrWhiteSpace(o.Endpoint))
                throw new InvalidOperationException(
                    "AzureFoundry:Endpoint must be configured in appsettings to use the cloud route.");

            AzureOpenAIClient az = o.AuthMode switch
            {
                AzureFoundryAuthMode.Identity => BuildIdentityClient(o),
                AzureFoundryAuthMode.Key => BuildKeyClient(o),
                _ => throw new InvalidOperationException(
                    $"Unsupported AzureFoundry:AuthMode '{o.AuthMode}'. Valid values: Key, Identity.")
            };

            return az.GetChatClient(o.Deployment).AsIChatClient();
        });

        return services;
    }

    /// <summary>
    /// Builds an <see cref="AzureOpenAIClient"/> that authenticates using an API key
    /// (<see cref="AzureFoundryAuthMode.Key"/>).
    /// </summary>
    /// <param name="o">The Azure Foundry options; <see cref="AzureFoundryOptions.ApiKey"/> must be set.</param>
    /// <returns>A configured <see cref="AzureOpenAIClient"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="AzureFoundryOptions.ApiKey"/> is empty.</exception>
    private static AzureOpenAIClient BuildKeyClient(AzureFoundryOptions o)
    {
        if (string.IsNullOrWhiteSpace(o.ApiKey))
            throw new InvalidOperationException(
                "AzureFoundry:ApiKey must be configured when AzureFoundry:AuthMode is 'Key'.");
        return new AzureOpenAIClient(new Uri(o.Endpoint), new AzureKeyCredential(o.ApiKey));
    }

    /// <summary>
    /// Builds an <see cref="AzureOpenAIClient"/> that authenticates using an Entra ID service principal
    /// (<see cref="AzureFoundryAuthMode.Identity"/>) via <see cref="ClientSecretCredential"/>.
    /// </summary>
    /// <param name="o">
    /// The Azure Foundry options; <see cref="AzureFoundryOptions.TenantId"/>,
    /// <see cref="AzureFoundryOptions.ClientId"/> and <see cref="AzureFoundryOptions.ClientSecret"/> must all be set.
    /// The service principal needs the <c>Cognitive Services OpenAI User</c> role on the target resource.
    /// </param>
    /// <returns>A configured <see cref="AzureOpenAIClient"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when any of <see cref="AzureFoundryOptions.TenantId"/>, <see cref="AzureFoundryOptions.ClientId"/>
    /// or <see cref="AzureFoundryOptions.ClientSecret"/> is empty.
    /// </exception>
    private static AzureOpenAIClient BuildIdentityClient(AzureFoundryOptions o)
    {
        if (string.IsNullOrWhiteSpace(o.TenantId) ||
            string.IsNullOrWhiteSpace(o.ClientId) ||
            string.IsNullOrWhiteSpace(o.ClientSecret))
            throw new InvalidOperationException(
                "AzureFoundry:TenantId, AzureFoundry:ClientId and AzureFoundry:ClientSecret " +
                "must all be configured when AzureFoundry:AuthMode is 'Identity'.");

        var credential = new ClientSecretCredential(o.TenantId, o.ClientId, o.ClientSecret);
        return new AzureOpenAIClient(new Uri(o.Endpoint), credential);
    }
}
