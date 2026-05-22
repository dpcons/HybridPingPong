using HybridPingPong.Core.Models;
using Microsoft.Extensions.AI;

namespace HybridPingPong.Core.Services;

/// <summary>
/// A router that always sends requests to the local SLM, bypassing any cloud processing.
/// Useful for scenarios where privacy and low latency are prioritised over model capability.
/// </summary>
public sealed class AlwaysLocalRouter : IModelRouter
{
    public RouterStrategy Strategy => RouterStrategy.AlwaysLocal;

    public Task<RoutingDecision> RouteAsync(string userMessage, IReadOnlyList<ChatMessage> history, CancellationToken ct = default)
        => Task.FromResult(new RoutingDecision(RouteTarget.Local, "Always local: all requests routed to local SLM", "always-local"));
}
