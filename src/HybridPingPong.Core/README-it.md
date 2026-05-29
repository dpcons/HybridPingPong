# HybridPingPong.Core

Questa libreria contiene la logica di dominio del progetto **HybridPingPong**. Fornisce un motore di routing AI ibrido che decide, per ogni turno di chat, se inoltrare la richiesta a un modello **locale** (Foundry Local) oppure al **cloud** (Azure AI Foundry). Il routing può essere puramente rule-based (deterministico e spiegabile), arricchito con un classificatore SLM per i casi ambigui, oppure configurato per usare sempre il modello cloud o sempre il modello locale.

Il progetto è organizzato in tre livelli:

| Cartella | Responsabilità |
|---|---|
| `Models/` | Contratti dati, opzioni di configurazione, enum e view model |
| `Services/` | Interfaccia del router, implementazioni dei router e orchestrator |
| `Utilities/` | Caricamento delle regole JSON e helper per i pattern |

---

## Diagrammi delle classi

### Services

```mermaid
classDiagram
    class IModelRouter {
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
        -ResolveRouter(strategy) IModelRouter
        -EstimateTokens(s) int
    }

    IModelRouter <|.. RuleBasedRouter
    IModelRouter <|.. SlmFallbackRouter
    IModelRouter <|.. AlwaysCloudRouter
    IModelRouter <|.. AlwaysLocalRouter
    SlmFallbackRouter --> IModelRouter : incapsula (ruleRouter)
    ChatOrchestrator --> IModelRouter : risolve e utilizza
    ChatOrchestrator --> ChatBackends : risolve i backend da
    SlmFallbackRouter --> ChatBackends : utilizza LocalKey
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

    class AzureFoundryAuthMode {
        <<enumeration>>
        Key
        Identity
    }

    class AzureFoundryOptions {
        <<sealed>>
        +string Endpoint
        +AzureFoundryAuthMode AuthMode
        +string ApiKey
        +string TenantId
        +string ClientId
        +string ClientSecret
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
    AzureFoundryOptions --> AzureFoundryAuthMode : AuthMode
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
    RulesLoader ..> RuleData : produce
```
