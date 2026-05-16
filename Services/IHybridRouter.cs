using Microsoft.Extensions.AI;

namespace HybridPingPong.Services;

public interface IHybridRouter
{
    RouterStrategy Strategy { get; }
    Task<RoutingDecision> RouteAsync(string userMessage, IReadOnlyList<ChatMessage> history, CancellationToken ct = default);
}
