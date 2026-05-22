namespace HybridPingPong.Core.Models;

/// <summary>
/// Available routing strategies for deciding where to send a chat request.
/// </summary>
public enum RouterStrategy
{
    /// <summary>Deterministic routing based on keyword and pattern rules.</summary>
    RuleBased,
    /// <summary>Rule-based routing with SLM fallback for ambiguous cases.</summary>
    RuleBasedPlusSlm,
    /// <summary>Always routes to the cloud LLM regardless of content.</summary>
    AlwaysCloud,
    /// <summary>Always routes to the local SLM regardless of content.</summary>
    AlwaysLocal
}
