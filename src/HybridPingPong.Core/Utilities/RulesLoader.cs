using System.Text.Json;

namespace HybridPingPong.Core.Utilities;

/// <summary>
/// Provides methods to load a <see cref="RuleData"/> from a <c>Rules.json</c> file on disk.
/// </summary>
public static class RulesLoader
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Asynchronously reads and deserializes a <see cref="RuleData"/> from the specified path.</summary>
    /// <param name="filePath">Absolute or relative path to the <c>Rules.json</c> file.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The deserialized <see cref="RuleData"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be deserialized.</exception>
    public static async Task<RuleData> LoadAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<RuleData>(stream, _options, ct)
               ?? throw new InvalidOperationException($"Failed to deserialize rules from '{filePath}'.");
    }

    /// <summary>Synchronously reads and deserializes a <see cref="RuleData"/> from the specified path.</summary>
    /// <param name="filePath">Absolute or relative path to the <c>Rules.json</c> file.</param>
    /// <returns>The deserialized <see cref="RuleData"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be deserialized.</exception>
    public static RuleData Load(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return JsonSerializer.Deserialize<RuleData>(stream, _options)
               ?? throw new InvalidOperationException($"Failed to deserialize rules from '{filePath}'.");
    }
}

