namespace HybridPingPong.Core.Models;

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
