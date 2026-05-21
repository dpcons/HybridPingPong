using System.Text.RegularExpressions;
using HybridPingPong.Core.Models;
using Microsoft.Extensions.AI;

namespace HybridPingPong.Core.Services;

/// <summary>
/// A deterministic router that uses keyword matching and regex patterns to detect
/// PII, sensitive data, or complex prompts and route them accordingly.
/// Messages containing personal data are kept local; complex or long prompts go to the cloud.
/// </summary>
public partial class RuleBasedRouter : IHybridRouter
{
    public RouterStrategy Strategy => RouterStrategy.RuleBased;

    private static readonly string[] PiiKeywords =
    {
        "codice fiscale", "iban", "password", "stipendio", "salary",
        "diagnosi", "paziente", "patient", "ssn", "credit card",
        "carta di credito", "contratto interno", "private",
        "riservato", "confidenziale"
    };

    private static readonly string[] ComplexityKeywords =
    {
        "spiega passo passo", "step by step", "dimostra", "prove",
        "scrivi un'app", "scrivi un programma", "write a program",
        "analizza in dettaglio", "deep analysis", "confronta in dettaglio",
        "refactor", "architettura", "design pattern"
    };

    [GeneratedRegex(@"\b(?:IT\d{2}[A-Z]\d{10,22}|[A-Z]{2}\d{2}[A-Z0-9]{10,30})\b", RegexOptions.IgnoreCase)]
    private static partial Regex IbanRegex();

    [GeneratedRegex(@"\b[A-Z]{6}\d{2}[A-Z]\d{2}[A-Z]\d{3}[A-Z]\b", RegexOptions.IgnoreCase)]
    private static partial Regex CodiceFiscaleRegex();

    [GeneratedRegex(@"\b[\w.+-]+@[\w-]+\.[\w.-]+\b")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b(?:\d[ -]*?){13,19}\b")]
    private static partial Regex CreditCardRegex();

    public Task<RoutingDecision> RouteAsync(string userMessage, IReadOnlyList<ChatMessage> history, CancellationToken ct = default)
        => Task.FromResult(Decide(userMessage, history));

    internal static RoutingDecision Decide(string userMessage, IReadOnlyList<ChatMessage> history)
    {
        var lower = userMessage.ToLowerInvariant();

        if (IbanRegex().IsMatch(userMessage))
            return new(RouteTarget.Local, "IBAN detected in prompt", "rule");
        if (CodiceFiscaleRegex().IsMatch(userMessage))
            return new(RouteTarget.Local, "Italian fiscal code detected", "rule");
        if (CreditCardRegex().IsMatch(userMessage))
            return new(RouteTarget.Local, "Credit-card-like number detected", "rule");
        if (EmailRegex().IsMatch(userMessage))
            return new(RouteTarget.Local, "Email address detected", "rule");

        foreach (var k in PiiKeywords)
            if (lower.Contains(k))
                return new(RouteTarget.Local, $"Sensitive keyword: '{k}'", "rule");

        if (userMessage.Length > 400)
            return new(RouteTarget.Cloud, $"Long prompt ({userMessage.Length} chars)", "rule");

        foreach (var k in ComplexityKeywords)
            if (lower.Contains(k))
                return new(RouteTarget.Cloud, $"Complex task keyword: '{k}'", "rule");

        var totalCtx = history.Sum(m => (m.Text ?? string.Empty).Length);
        if (totalCtx > 3000)
            return new(RouteTarget.Cloud, $"Long conversation context ({totalCtx} chars)", "rule");

        return new(RouteTarget.Local, "Default: short/simple query", "rule");
    }
}
