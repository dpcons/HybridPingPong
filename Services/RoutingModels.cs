namespace HybridPingPong.Services;

public enum RouteTarget { Local, Cloud }

public enum RouterStrategy
{
    RuleBased,
    RuleBasedPlusSlm
}

public sealed record RoutingDecision(
    RouteTarget Target,
    string Reason,
    string DecidedBy  // "rule" | "slm" | "fallback"
);

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

public sealed record ChatMessageVm(
    string Role,            // "user" | "assistant"
    string Content,
    ChatTurnMetrics? Metrics = null
);
