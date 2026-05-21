using System.Text.RegularExpressions;
using HybridPingPong.Core.Models;
using HybridPingPong.Core.Utilities;
using Microsoft.Extensions.AI;

namespace HybridPingPong.Core.Services;

/// <summary>
/// A deterministic router that uses keyword matching and regex patterns to detect
/// PII, sensitive data, or complex prompts and route them accordingly.
/// Rules are loaded from <c>data/Rules.json</c> via <see cref="RulesLoader"/>.
/// Messages containing personal data are kept local; complex or long prompts go to the cloud.
/// </summary>
public class RuleBasedRouter : IHybridRouter
{
    public RouterStrategy Strategy => RouterStrategy.RuleBased;

    private static readonly RuleData _rules = RulesLoader.Load(
        Path.Combine(AppContext.BaseDirectory, "data", "Rules.json"));

    public Task<RoutingDecision> RouteAsync(string userMessage, IReadOnlyList<ChatMessage> history, CancellationToken ct = default)
        => Task.FromResult(Decide(userMessage, history));

    private static RoutingDecision Decide(string userMessage, IReadOnlyList<ChatMessage> history)
    {
        var lower = userMessage.ToLowerInvariant();

        foreach (var p in _rules.RegexPatterns)
        {
            var options = p.Flags.Equals("IgnoreCase", StringComparison.OrdinalIgnoreCase)
                ? RegexOptions.IgnoreCase
                : RegexOptions.None;
            if (Regex.IsMatch(userMessage, p.Pattern, options, TimeSpan.FromSeconds(1)))
                return new(RouteTarget.Local, p.Reason, "rule");
        }

        foreach (var k in _rules.PiiKeywords)
            if (lower.Contains(k))
                return new(RouteTarget.Local, $"Sensitive keyword: '{k}'", "rule");

        if (userMessage.Length > 400)
            return new(RouteTarget.Cloud, $"Long prompt ({userMessage.Length} chars)", "rule");

        foreach (var k in _rules.ComplexityKeywords)
            if (lower.Contains(k))
                return new(RouteTarget.Cloud, $"Complex task keyword: '{k}'", "rule");

        var totalCtx = history.Sum(m => (m.Text ?? string.Empty).Length);
        if (totalCtx > 3000)
            return new(RouteTarget.Cloud, $"Long conversation context ({totalCtx} chars)", "rule");

        return new(RouteTarget.Local, "Default: short/simple query", "rule");
    }
}
