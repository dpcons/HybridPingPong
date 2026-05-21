using System.Text.Json;
using System.Text.Json.Serialization;

namespace HybridPingPong.Core.Utilities;

/// <summary>
/// Represents the deserialized contents of <c>Rules.json</c>,
/// containing PII keywords, complexity keywords, and regex patterns
/// used by the routing engine.
/// </summary>
public sealed class RulesFile
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

    /// <summary>The <see cref="System.Text.RegularExpressions.RegexOptions"/> flags applied to the pattern (e.g. <c>IgnoreCase</c>, <c>None</c>).</summary>
    [JsonPropertyName("flags")]
    public string Flags { get; init; } = string.Empty;
}

/// <summary>
/// Provides methods to load a <see cref="RulesFile"/> from a <c>Rules.json</c> file on disk.
/// </summary>
public static class RulesLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Asynchronously reads and deserializes a <see cref="RulesFile"/> from the specified path.</summary>
    /// <param name="filePath">Absolute or relative path to the <c>Rules.json</c> file.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The deserialized <see cref="RulesFile"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be deserialized.</exception>
    public static async Task<RulesFile> LoadAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<RulesFile>(stream, _options, ct)
               ?? throw new InvalidOperationException($"Failed to deserialize rules from '{filePath}'.");
    }

    /// <summary>Synchronously reads and deserializes a <see cref="RulesFile"/> from the specified path.</summary>
    /// <param name="filePath">Absolute or relative path to the <c>Rules.json</c> file.</param>
    /// <returns>The deserialized <see cref="RulesFile"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be deserialized.</exception>
    public static RulesFile Load(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<RulesFile>(stream, _options)
               ?? throw new InvalidOperationException($"Failed to deserialize rules from '{filePath}'.");
    }
}
