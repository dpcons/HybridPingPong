namespace HybridPingPong.Core.Services;

/// <summary>
/// Indicates whether a chat request should be handled locally or in the cloud.
/// </summary>
public enum RouteTarget { Local, Cloud }

/// <summary>
/// Available routing strategies for deciding where to send a chat request.
/// </summary>
public enum RouterStrategy
{
    /// <summary>Deterministic routing based on keyword and pattern rules.</summary>
    RuleBased,
    /// <summary>Rule-based routing with SLM fallback for ambiguous cases.</summary>
    RuleBasedPlusSlm
}

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

/// <summary>
/// Performance and cost metrics collected for a single chat turn.
/// </summary>
/// <param name="Target">The route target used for this turn.</param>
/// <param name="Reason">Why this target was chosen.</param>
/// <param name="DecidedBy">The mechanism that made the routing decision.</param>
/// <param name="ModelName">The model name or deployment used.</param>
/// <param name="LatencyMs">Total end-to-end latency in milliseconds.</param>
/// <param name="FirstTokenMs">Time to first token in milliseconds.</param>
/// <param name="InputTokens">Number of input tokens consumed.</param>
/// <param name="OutputTokens">Number of output tokens generated.</param>
/// <param name="EstimatedCostUsd">Estimated cost in USD for cloud requests.</param>
public sealed record ChatTurnMetrics(
    RouteTarget Target,
    string Reason,
    string DecidedBy,
    string ModelName,
    long LatencyMs,
    int FirstTokenMs,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCostUsd
);

/// <summary>
/// View model representing a single message in the chat conversation.
/// </summary>
/// <param name="Role">The message role: "user" or "assistant".</param>
/// <param name="Content">The text content of the message.</param>
/// <param name="Metrics">Optional metrics associated with an assistant response.</param>
public sealed record ChatMessageVm(
    string Role,
    string Content,
    ChatTurnMetrics? Metrics = null
);
