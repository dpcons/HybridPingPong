using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace HybridPingPong.Core.Services;

public sealed class FoundryLocalOptions
{
    public string Endpoint { get; set; } = "http://localhost:5273/v1";
    public string Model { get; set; } = "phi-4-mini";
    public string ApiKey { get; set; } = "not-needed";
}

public sealed class AzureFoundryOptions
{
    public string Endpoint { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string Deployment { get; set; } = "gpt-4o-mini";
    public decimal InputPricePer1K { get; set; } = 0.00015m;
    public decimal OutputPricePer1K { get; set; } = 0.00060m;
}

public static class ChatBackends
{
    public const string LocalKey = "local";
    public const string CloudKey = "cloud";

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
