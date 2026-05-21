using System.Text.Json;
using System.Text.RegularExpressions;
using HybridPingPong.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HybridPingPong.Core.Services;

/// <summary>
/// A hybrid router that first applies hard rules (PII detection, complexity keywords)
/// and then delegates ambiguous cases to the local SLM for a classification decision.
/// Falls back to the rule-based result if the SLM call fails.
/// </summary>
public sealed class SlmFallbackRouter : IHybridRouter
{
    public RouterStrategy Strategy => RouterStrategy.RuleBasedPlusSlm;

    private readonly IChatClient _localClient;
    private readonly ILogger<SlmFallbackRouter> _log;

    private const string RouterSystemPrompt = """
        You are a routing classifier. Decide whether the user's query should be
        handled by a SMALL LOCAL model (cheap, fast, private) or a LARGE CLOUD
        model (powerful, expensive, slower).

        Respond with ONLY a single JSON object, no prose, no markdown:
        {"target":"local"|"cloud","reason":"<short reason, max 80 chars>"}

        Choose "local" for: greetings, chitchat, short Q&A, summarization of
        short text, anything mentioning private data, names, addresses,
        contracts, salaries, health.

        Choose "cloud" for: multi-step reasoning, code generation longer than a
        few lines, deep analysis, creative long-form writing, anything that
        clearly needs a frontier model.
        """;

    public SlmFallbackRouter(
        [FromKeyedServices(ChatBackends.LocalKey)] IChatClient localClient,
        ILogger<SlmFallbackRouter> log)
    {
        _localClient = localClient;
        _log = log;
    }

    public async Task<RoutingDecision> RouteAsync(string userMessage, IReadOnlyList<ChatMessage> history, CancellationToken ct = default)
    {
        var ruleDecision = RuleBasedRouter.Decide(userMessage, history);
        var reason = ruleDecision.Reason;
        var isHardRule =
            reason.StartsWith("IBAN") || reason.StartsWith("Italian fiscal")
            || reason.StartsWith("Credit-card") || reason.StartsWith("Email")
            || reason.StartsWith("Sensitive keyword") || reason.StartsWith("Long prompt")
            || reason.StartsWith("Complex task") || reason.StartsWith("Long conversation");

        if (isHardRule)
            return ruleDecision;

        try
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, RouterSystemPrompt),
                new(ChatRole.User, userMessage)
            };

            var options = new ChatOptions
            {
                Temperature = 0.0f,
                MaxOutputTokens = 80
            };

            var response = await _localClient.GetResponseAsync(messages, options, ct);
            var text = response.Text?.Trim() ?? "";
            var (target, slmReason) = ParseJson(text);
            return new RoutingDecision(target, slmReason, "slm");
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "SLM router failed, falling back to rule decision.");
            return ruleDecision with { DecidedBy = "fallback" };
        }
    }

    private static readonly Regex JsonExtractor = new(@"\{.*\}", RegexOptions.Singleline | RegexOptions.Compiled);

    private static (RouteTarget, string) ParseJson(string raw)
    {
        var m = JsonExtractor.Match(raw);
        if (!m.Success) return (RouteTarget.Local, "SLM produced no JSON, default local");

        try
        {
            using var doc = JsonDocument.Parse(m.Value);
            var root = doc.RootElement;
            var targetStr = root.TryGetProperty("target", out var t) ? t.GetString() ?? "local" : "local";
            var reason = root.TryGetProperty("reason", out var r) ? r.GetString() ?? "SLM decision" : "SLM decision";
            var target = targetStr.Equals("cloud", StringComparison.OrdinalIgnoreCase)
                ? RouteTarget.Cloud
                : RouteTarget.Local;
            return (target, reason);
        }
        catch
        {
            return (RouteTarget.Local, "SLM JSON parse failed, default local");
        }
    }
}
