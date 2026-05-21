using System.Text.Json.Serialization;

namespace HybridPingPong.Core.Utilities;

/// <summary>
/// Describes a single named regex pattern with its flags, as stored in <c>Rules.json</c>.
/// </summary>
public sealed class RegexPatternEntry
{
    /// <summary>Logical name identifying the pattern (e.g. <c>IbanRegex</c>).</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>The raw regex pattern string.</summary>
    [JsonPropertyName("pattern")]
    public string Pattern { get; init; } = string.Empty;

    /// <summary>Human-readable reason returned when this pattern matches a message.</summary>
    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;

    /// <summary>The <see cref="System.Text.RegularExpressions.RegexOptions"/> flags applied to the pattern (e.g. <c>IgnoreCase</c>, <c>None</c>).</summary>
    [JsonPropertyName("flags")]
    public string Flags { get; init; } = string.Empty;
}
