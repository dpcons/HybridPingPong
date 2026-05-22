using HybridPingPong.Core.Models;
using Microsoft.Extensions.AI;

namespace HybridPingPong.Core.Services;

/// <summary>
/// A router that always sends requests to the cloud LLM, bypassing any local processing.
/// Useful for scenarios where maximum model capability is required regardless of cost or privacy.
/// </summary>
public sealed class AlwaysCloudRouter : IModelRouter
{
    public RouterStrategy Strategy => RouterStrategy.AlwaysCloud;

    public Task<RoutingDecision> RouteAsync(string userMessage, IReadOnlyList<ChatMessage> history, CancellationToken ct = default)
        => Task.FromResult(new RoutingDecision(RouteTarget.Cloud, "Always cloud: all requests routed to cloud LLM", "always-cloud"));
}
