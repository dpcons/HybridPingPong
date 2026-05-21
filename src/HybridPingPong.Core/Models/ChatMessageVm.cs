namespace HybridPingPong.Core.Models;

/// <summary>
/// View model representing a single message in the chat conversation.
/// </summary>
/// <param name="Role">The message role: "user" or "assistant".</param>
/// <param name="Content">The text content of the message.</param>
/// <param name="Metrics">Optional metrics associated with an assistant response.</param>
public sealed record ChatMessageVm(
    string Role,
    string Content,
    ChatTurnMetrics? Metrics = null
);
