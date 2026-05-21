using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HybridPingPong.Core.Services;

public sealed class StreamUpdate
{
    public string? Token { get; init; }
    public bool Done { get; init; }
    public ChatTurnMetrics? Metrics { get; init; }
    public string? Error { get; init; }
}

public sealed class ChatOrchestrator
{
    private readonly IServiceProvider _sp;
    private readonly IOptions<FoundryLocalOptions> _localOpts;
    private readonly IOptions<AzureFoundryOptions> _cloudOpts;

    public ChatOrchestrator(
        IServiceProvider sp,
        IOptions<FoundryLocalOptions> localOpts,
        IOptions<AzureFoundryOptions> cloudOpts)
    {
        _sp = sp;
        _localOpts = localOpts;
        _cloudOpts = cloudOpts;
    }

    private IHybridRouter ResolveRouter(RouterStrategy strategy) =>
        _sp.GetRequiredKeyedService<IHybridRouter>(strategy);

    public async IAsyncEnumerable<StreamUpdate> ChatAsync(
        string userMessage,
        List<ChatMessage> history,
        RouterStrategy strategy,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var router = ResolveRouter(strategy);
        var decision = await router.RouteAsync(userMessage, history, ct);

        history.Add(new ChatMessage(ChatRole.User, userMessage));

        IChatClient? client = null;
        string? resolveError = null;
        try
        {
            client = _sp.GetRequiredKeyedService<IChatClient>(
                decision.Target == RouteTarget.Local ? ChatBackends.LocalKey : ChatBackends.CloudKey);
        }
        catch (Exception ex)
        {
            resolveError = ex.Message;
        }
        if (client is null)
        {
            yield return new StreamUpdate { Done = true, Error = resolveError ?? "Failed to resolve chat client." };
            yield break;
        }

        var modelName = decision.Target == RouteTarget.Local
            ? _localOpts.Value.Model
            : _cloudOpts.Value.Deployment;

        var sw = Stopwatch.StartNew();
        int firstTokenMs = 0;
        var assistant = new System.Text.StringBuilder();
        int inputTokens = 0, outputTokens = 0;

        IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
        string? startError = null;
        try
        {
            enumerator = client.GetStreamingResponseAsync(history, cancellationToken: ct).GetAsyncEnumerator(ct);
        }
        catch (Exception ex)
        {
            startError = ex.Message;
        }
        if (enumerator is null)
        {
            yield return new StreamUpdate { Done = true, Error = startError ?? "Failed to start stream." };
            yield break;
        }

        Exception? failure = null;
        try
        {
            while (true)
            {
                bool hasNext;
                try
                {
                    hasNext = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    failure = ex;
                    break;
                }
                if (!hasNext) break;

                var update = enumerator.Current;
                var text = update.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    if (firstTokenMs == 0) firstTokenMs = (int)sw.ElapsedMilliseconds;
                    assistant.Append(text);
                    yield return new StreamUpdate { Token = text };
                }
                if (update.Contents is { Count: > 0 })
                {
                    foreach (var c in update.Contents)
                    {
                        if (c is UsageContent uc && uc.Details is not null)
                        {
                            inputTokens = (int)(uc.Details.InputTokenCount ?? 0);
                            outputTokens = (int)(uc.Details.OutputTokenCount ?? 0);
                        }
                    }
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        sw.Stop();

        if (failure is not null)
        {
            yield return new StreamUpdate { Done = true, Error = failure.Message };
            yield break;
        }

        var full = assistant.ToString();
        history.Add(new ChatMessage(ChatRole.Assistant, full));

        if (inputTokens == 0)
            inputTokens = EstimateTokens(string.Concat(history.Where(m => m.Role != ChatRole.Assistant).Select(m => m.Text)));
        if (outputTokens == 0)
            outputTokens = EstimateTokens(full);

        decimal cost = 0m;
        if (decision.Target == RouteTarget.Cloud)
        {
            var o = _cloudOpts.Value;
            cost = (inputTokens / 1000m) * o.InputPricePer1K
                 + (outputTokens / 1000m) * o.OutputPricePer1K;
        }

        yield return new StreamUpdate
        {
            Done = true,
            Metrics = new ChatTurnMetrics(
                decision.Target, decision.Reason, decision.DecidedBy,
                modelName, sw.ElapsedMilliseconds, firstTokenMs,
                inputTokens, outputTokens, cost)
        };
    }

    private static int EstimateTokens(string s) =>
        string.IsNullOrEmpty(s) ? 0 : Math.Max(1, s.Length / 4);
}
