# HybridPingPong.Core

This library contains the domain logic for the **HybridPingPong** project. It provides a hybrid AI routing engine that decides, for each chat turn, whether to dispatch a request to a **local** model (Foundry Local) or to the **cloud** (Azure AI Foundry). The routing can be purely rule-based (deterministic, explainable), augmented with an SLM classifier for ambiguous cases, or configured to always use the cloud model or always use the local model.

The project is organised into three layers:

| Folder | Responsibility |
|---|---|
| `Models/` | Data contracts, options, enums, and view models |
| `Services/` | Router interface, router implementations, and the orchestrator |
| `Utilities/` | JSON rule loading and pattern helpers |

---

## Class Diagrams

### Services

```mermaid
classDiagram
    class IHybridRouter {
        <<interface>>
        +RouterStrategy Strategy
        +RouteAsync(userMessage, history, ct) Task~RoutingDecision~
    }

    class RuleBasedRouter {
        +RouterStrategy Strategy
        +RouteAsync(userMessage, history, ct) Task~RoutingDecision~
        -Decide(userMessage, history) RoutingDecision
    }

    class SlmFallbackRouter {
        +RouterStrategy Strategy
        +RouteAsync(userMessage, history, ct) Task~RoutingDecision~
        -ParseJson(raw) ValueTuple~RouteTarget, string~
    }

    class AlwaysCloudRouter {
        +RouterStrategy Strategy
        +RouteAsync(userMessage, history, ct) Task~RoutingDecision~
    }

    class AlwaysLocalRouter {
        +RouterStrategy Strategy
        +RouteAsync(userMessage, history, ct) Task~RoutingDecision~
    }

    class ChatBackends {
        <<static>>
        +string LocalKey$
        +string CloudKey$
        +AddHybridChatBackends(services, cfg) IServiceCollection
    }

    class ChatOrchestrator {
        +ChatAsync(userMessage, history, strategy, ct) IAsyncEnumerable~StreamUpdate~
        -ResolveRouter(strategy) IHybridRouter
        -EstimateTokens(s) int
    }

    IHybridRouter <|.. RuleBasedRouter
    IHybridRouter <|.. SlmFallbackRouter
    IHybridRouter <|.. AlwaysCloudRouter
    IHybridRouter <|.. AlwaysLocalRouter
    SlmFallbackRouter --> IHybridRouter : wraps (ruleRouter)
    ChatOrchestrator --> IHybridRouter : resolves and uses
    ChatOrchestrator --> ChatBackends : resolves backends from
    SlmFallbackRouter --> ChatBackends : uses LocalKey
```

---

### Models

```mermaid
classDiagram
    class RouteTarget {
        <<enumeration>>
        Local
        Cloud
    }

    class RouterStrategy {
        <<enumeration>>
        RuleBased
        RuleBasedPlusSlm
        AlwaysCloud
        AlwaysLocal
    }

    class RoutingDecision {
        <<record>>
        +RouteTarget Target
        +string Reason
        +string DecidedBy
    }

    class ChatTurnMetrics {
        <<record>>
        +RouteTarget Target
        +string Reason
        +string DecidedBy
        +string ModelName
        +long LatencyMs
        +int FirstTokenMs
        +int InputTokens
        +int OutputTokens
        +decimal EstimatedCostUsd
    }

    class StreamUpdate {
        <<sealed>>
        +string? Token
        +bool Done
        +ChatTurnMetrics? Metrics
        +string? Error
    }

    class ChatMessageVm {
        <<record>>
        +string Role
        +string Content
        +ChatTurnMetrics? Metrics
    }

    class AzureFoundryOptions {
        <<sealed>>
        +string Endpoint
        +string ApiKey
        +string Deployment
        +decimal InputPricePer1K
        +decimal OutputPricePer1K
    }

    class FoundryLocalOptions {
        <<sealed>>
        +string Endpoint
        +string Model
        +string ApiKey
    }

    RoutingDecision --> RouteTarget : Target
    ChatTurnMetrics --> RouteTarget : Target
    StreamUpdate --> ChatTurnMetrics : Metrics
    ChatMessageVm --> ChatTurnMetrics : Metrics
```

---

### Utilities

```mermaid
classDiagram
    class RegexPatternEntry {
        <<sealed>>
        +string Name
        +string Pattern
        +string Reason
        +string Flags
    }

    class RuleData {
        <<sealed>>
        +string[] PiiKeywords
        +string[] ComplexityKeywords
        +RegexPatternEntry[] RegexPatterns
    }

    class RulesLoader {
        <<static>>
        +LoadAsync(filePath, ct) Task~RuleData~
        +Load(filePath) RuleData
    }

    RuleData "1" *-- "0..*" RegexPatternEntry : RegexPatterns
    RulesLoader ..> RuleData : produces
```
