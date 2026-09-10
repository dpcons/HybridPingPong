# HybridPingPong.Web

Blazor Server application (.NET 10) that provides the interactive front-end for the **HybridPingPong** demo. It exposes a single chat page where every message is automatically routed to either a **local** AI model (Foundry Local) or a **cloud** model (Azure AI Foundry), and every response is annotated with live telemetry so the routing decision is always visible and explainable.

---

## Goal

Show, live on stage, that a smart router can keep **sensitive or simple queries on-device** while offloading only complex tasks to the cloud — maximising privacy and minimising cost — with zero code change required from the end user.

---

## Project Structure

```
HybridPingPong.Web/
├── Components/
│   ├── Pages/
│   │   ├── Chat.razor          # Main chat page (route: /chat)
│   │   ├── Home.razor          # Root page (redirects to /chat)
│   │   ├── Error.razor         # Unhandled-error page
│   │   └── NotFound.razor      # 404 page
│   ├── Layout/
│   │   ├── MainLayout.razor    # Shell layout (renders @Body only)
│   │   └── ReconnectModal.*    # Blazor SignalR reconnect overlay
│   ├── App.razor               # Blazor application root
│   ├── Routes.razor            # Router configuration
│   └── _Imports.razor          # Global using directives
├── wwwroot/
│   └── app.css                 # Global styles
├── Program.cs                  # DI wiring and ASP.NET Core pipeline
└── appsettings.json            # Default configuration
```

---

## Chat Page — Key Features

### Live routing badge per response
Every assistant bubble shows a badge with:
- 🟢 **LOCAL** or 🔵 **CLOUD**
- Model name
- Total latency (ms) and time-to-first-token (ms)
- Estimated cloud cost ($ — shown only for cloud responses)
- *"why?"* — the human-readable reason for the routing decision

### Cumulative stats header
The header tracks the full session at a glance:

| Stat | Description |
|---|---|
| **Turns** | Total messages exchanged |
| **🟢 Local** | Count + % of turns served locally |
| **🔵 Cloud** | Count of turns sent to the cloud |
| **💰 Cloud cost** | Running total in USD |

### Switchable routing strategy
A dropdown (disabled while streaming) lets you switch strategy live without reloading:

| Strategy | Behaviour |
|---|---|
| **Rule-based** | Deterministic rules: PII patterns, complexity keywords, IBAN/CF/email regex. Fully explainable. |
| **Rule-based + SLM** | Hard rules run first; ambiguous cases are delegated to a local mini-classifier that emits `{target, reason}` JSON. |

### Streaming responses
Tokens are streamed token-by-token using `IAsyncEnumerable<StreamUpdate>` from the `ChatOrchestrator`. The UI renders each token as it arrives with a blinking caret.

---

## Dependency Injection (Program.cs)

```
AddHybridChatBackends()           ← registers IChatClient (local + cloud) as keyed singletons
AddKeyedSingleton<IModelRouter>  ← RuleBasedRouter        (key: RouterStrategy.RuleBased)
AddKeyedSingleton<IModelRouter>  ← SlmFallbackRouter      (key: RouterStrategy.RuleBasedPlusSlm)
AddSingleton<ChatOrchestrator>    ← orchestrates routing + streaming
AddRazorComponents()
  .AddInteractiveServerComponents ← Blazor Server render mode
```

---

## Configuration

`appsettings.json` (or user secrets / environment variables):

```json
{
  "FoundryLocal": {
    "Endpoint": "http://localhost:53274/v1",
    "Model": "Phi-4-mini-instruct-cuda-gpu:5",
    "ApiKey": "not-needed"
  },
  "AzureFoundry": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "AuthMode": "Key",
    "ApiKey": "",
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    "Deployment": "gpt-4o-mini",
    "InputPricePer1K": 0.00015,
    "OutputPricePer1K": 0.00060
  }
}
```

> **Note on `FoundryLocal` `Endpoint`** To retrieve the local endpoint used by Foundry Local, you can run the following command:

```powershell
   foundry server status
```

> **Note on `FoundryLocal` `Model`** if Foundry Local is running a hardware-specific variant, use the full model ID as reported by the Foundry Local endpoint (e.g. `Phi-4-mini-instruct-cuda-gpu:5`).

### Authentication modes for Azure AI Foundry

The `AzureFoundry:AuthMode` property selects how the application authenticates against the cloud endpoint. Two modes are supported:

| AuthMode | Required properties | When to use |
|---|---|---|
| `Key` | `Endpoint`, `ApiKey` | Local development or simple shared-secret scenarios. |
| `Identity` | `Endpoint`, `TenantId`, `ClientId`, `ClientSecret` | Production / enterprise scenarios with an Entra ID service principal. The principal must have the *Cognitive Services OpenAI User* role on the resource. |

**Key example:**

```json
"AzureFoundry": {
  "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
  "AuthMode": "Key",
  "ApiKey": "<api-key>",
  "Deployment": "gpt-4o-mini"
}
```

**Entra ID (service principal) example:**

```json
"AzureFoundry": {
  "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
  "AuthMode": "Identity",
  "TenantId": "<tenant-guid>",
  "ClientId": "<application-guid>",
  "ClientSecret": "<client-secret>",
  "Deployment": "gpt-4o-mini"
}
```

### Granting the `Azure AI User` role to the service principal

When `AuthMode` is `Identity`, the Entra ID service principal must be authorised to call inference on the model deployed in the Azure AI Foundry project. The minimum required built-in role is:

- **`Azure AI User`** — grants data-plane permissions to invoke chat completions, embeddings and other inference APIs on models deployed in a Foundry project, **without** any management permissions.

> If the target resource is a *classic* Azure OpenAI account (kind `OpenAI`) instead of a Foundry project (kind `AIServices`), use **`Cognitive Services OpenAI User`** instead.

**Assign the role via Azure CLI:**

```powershell
az role assignment create `
  --assignee <application-guid> `
  --role "Azure AI User" `
  --scope /subscriptions/<subscription-id>/resourceGroups/<resource-group>/providers/Microsoft.CognitiveServices/accounts/<foundry-account-name>
```

The scope can be set at the Foundry account level (covers every project) or narrowed to a specific project / deployment if you need finer-grained access.

**Assign the role via Azure Portal:**

1. Open the Azure AI Foundry account (or the specific project) in the Azure Portal.
2. Go to **Access control (IAM) → Add → Add role assignment**.
3. Select the role **Azure AI User**.
4. Assign access to **User, group, or service principal** and pick the Entra ID application configured in `AzureFoundry:ClientId`.
5. Save the assignment — role propagation usually completes within a few minutes.

> **Tip:** use user secrets for the Azure API key or client secret to avoid committing credentials:
> ```powershell
> dotnet user-secrets set "AzureFoundry:ApiKey" "your-key-here"
> # or, for Entra ID:
> dotnet user-secrets set "AzureFoundry:AuthMode" "Identity"
> dotnet user-secrets set "AzureFoundry:TenantId" "<tenant-guid>"
> dotnet user-secrets set "AzureFoundry:ClientId" "<application-guid>"
> dotnet user-secrets set "AzureFoundry:ClientSecret" "<client-secret>"
> ```

---

## Running Locally

```powershell
# from the repo root
dotnet run --project src/HybridPingPong.Web
```

Then open `https://localhost:<port>/chat`. The root URL (`/`) redirects automatically to `/chat`.

### Prerequisites
1. **.NET 10 SDK**
2. **Foundry Local** running with a model loaded:
   ```powershell
   foundry server start
   foundry model load phi-4-mini
   ```

   > **Note on `model`** if Foundry Local is running a hardware-specific variant, use the full model ID as reported by the Foundry Local endpoint (e.g. `Phi-4-mini-instruct-cuda-gpu:5`).

3. *(Optional)* An **Azure AI Foundry / Azure OpenAI** deployment (e.g. `gpt-4o-mini`) for cloud routing.
