namespace HybridPingPong.Core.Models;

/// <summary>
/// Represents a single incremental update emitted during a streaming chat response.
/// </summary>
public sealed class StreamUpdate
{
    /// <summary>A text token fragment from the model response, or <c>null</c> if not a content update.</summary>
    public string? Token { get; init; }

    /// <summary>Indicates whether the stream has completed.</summary>
    public bool Done { get; init; }

    /// <summary>Final metrics for the completed turn, available when <see cref="Done"/> is <c>true</c>.</summary>
    public ChatTurnMetrics? Metrics { get; init; }

    /// <summary>An error message if the stream failed, or <c>null</c> on success.</summary>
    public string? Error { get; init; }
}
