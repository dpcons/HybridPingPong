namespace HybridPingPong.Core.Models;

/// <summary>
/// Represents the outcome of a routing decision, including the chosen target and reasoning.
/// </summary>
/// <param name="Target">The selected route target (local or cloud).</param>
/// <param name="Reason">A human-readable explanation for the routing decision.</param>
/// <param name="DecidedBy">The mechanism that made the decision: "rule", "slm", or "fallback".</param>
public sealed record RoutingDecision(
    RouteTarget Target,
    string Reason,
    string DecidedBy
);
