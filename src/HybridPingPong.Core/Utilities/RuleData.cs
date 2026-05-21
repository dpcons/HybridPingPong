using System.Text.Json.Serialization;

namespace HybridPingPong.Core.Utilities;

/// <summary>
/// Represents the deserialized contents of <c>Rules.json</c>,
/// containing PII keywords, complexity keywords, and regex patterns
/// used by the routing engine.
/// </summary>
public sealed class RuleData
{
    /// <summary>Keywords that indicate personally identifiable information (PII) in a message.</summary>
    [JsonPropertyName("piiKeywords")]
    public string[] PiiKeywords { get; init; } = [];

    /// <summary>Keywords that indicate a complex or high-effort prompt to be routed to the cloud.</summary>
    [JsonPropertyName("complexityKeywords")]
    public string[] ComplexityKeywords { get; init; } = [];

    /// <summary>Regex patterns used for structured PII detection (e.g. IBAN, fiscal code, email, credit card).</summary>
    [JsonPropertyName("regexPatterns")]
    public RegexPatternEntry[] RegexPatterns { get; init; } = [];
}
