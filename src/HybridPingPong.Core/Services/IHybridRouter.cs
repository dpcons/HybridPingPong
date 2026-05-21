using Microsoft.Extensions.AI;

namespace HybridPingPong.Core.Services;

/// <summary>
/// Defines a hybrid routing strategy that decides whether a chat message
/// should be processed by the local SLM or the cloud LLM.
/// </summary>
public interface IHybridRouter
{
    /// <summary>Gets the routing strategy this router implements.</summary>
    RouterStrategy Strategy { get; }

    /// <summary>
    /// Evaluates the user message and conversation history to produce a routing decision.
    /// </summary>
    /// <param name="userMessage">The current user message to route.</param>
    /// <param name="history">The conversation history so far.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="RoutingDecision"/> indicating where to send the request.</returns>
    Task<RoutingDecision> RouteAsync(string userMessage, IReadOnlyList<ChatMessage> history, CancellationToken ct = default);
}
