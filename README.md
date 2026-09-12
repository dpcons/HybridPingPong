# Hybrid Ping-Pong 🏓

Blazor Server demo in C# (.NET 10) that shows a simple chat whose **smart router** decides, at every turn, whether to reply via **Foundry Local** or **Azure AI Foundry (cloud)**.

Designed for the session *"The Foundry Forge: Crafting AI Solutions from Azure to the Edge"*.

## What it does

- A single chat, two backends (both OpenAI-compatible) exposed as `IChatClient` (Microsoft.Extensions.AI).
- **Router switchable live from the UI**:
  - **Rule-based** — explicit rules (PII, IBAN, Italian fiscal code, email, complexity, long context). Deterministic and explainable on stage.
  - **Rule-based + SLM fallback** — hard rules first; on grey-area cases a mini-classifier on Foundry Local emits a `{target,reason}` JSON.
- Live badge for every reply: 🟢 LOCAL / 🔵 CLOUD · model · latency · time-to-first-token · estimated cost · router *"why?"*.
- Cumulative counter at the top: turns, % local, $ spent on cloud.

## Prerequisites

1. **.NET 10 SDK** (the repo targets `net10.0`).
2. **Foundry Local** running with a loaded model, e.g.:
   ```powershell
   foundry model run phi-4-mini
   ```
   Verify the OpenAI-compatible endpoint at `http://localhost:5273/v1`.
3. (Optional but recommended for the full demo) An **Azure AI Foundry / Azure OpenAI** deployment, e.g. `gpt-4o-mini`.

## Configuration

Edit `appsettings.json` (or use user-secrets / environment variables):

```json
"FoundryLocal": {
  "Endpoint": "http://localhost:5273/v1",
  "Model": "phi-4-mini"
},
"AzureFoundry": {
  "Endpoint": "https://your-resource.openai.azure.com/",
  "AuthMode": "Key",
  "ApiKey": "...",
  "Deployment": "gpt-4o-mini"
}
```

### Authentication modes for Azure AI Foundry

The `AzureFoundry:AuthMode` property selects how the application authenticates against the cloud endpoint. Two modes are supported:

| AuthMode | Required properties | When to use |
|---|---|---|
| `Key` | `Endpoint`, `ApiKey` | Local development or simple shared-secret scenarios. |
| `Identity` | `Endpoint`, `TenantId`, `ClientId`, `ClientSecret` | Production / enterprise scenarios with an Entra ID service principal. The principal must hold the *Cognitive Services OpenAI User* role on the resource. |

**Key example:**

```json
"AzureFoundry": {
  "Endpoint": "https://your-resource.openai.azure.com/",
  "AuthMode": "Key",
  "ApiKey": "<api-key>",
  "Deployment": "gpt-4o-mini"
}
```

**Entra ID (service principal) example:**

```json
"AzureFoundry": {
  "Endpoint": "https://your-resource.openai.azure.com/",
  "AuthMode": "Identity",
  "TenantId": "<tenant-guid>",
  "ClientId": "<application-guid>",
  "ClientSecret": "<client-secret>",
  "Deployment": "gpt-4o-mini"
}
```
 
> If Foundry Local is running a hardware-specific variant of a model, the `Model` property under `FoundryLocal` must be set to the full model name.

With user-secrets:
```powershell
dotnet user-secrets init
dotnet user-secrets set "AzureFoundry:Endpoint" "https://..."
dotnet user-secrets set "AzureFoundry:ApiKey" "..."
# or, for Entra ID:
dotnet user-secrets set "AzureFoundry:AuthMode" "Identity"
dotnet user-secrets set "AzureFoundry:TenantId" "<tenant-guid>"
dotnet user-secrets set "AzureFoundry:ClientId" "<application-guid>"
dotnet user-secrets set "AzureFoundry:ClientSecret" "<client-secret>"
```

## Run

```powershell
dotnet run
```

Open `https://localhost:7xxx/chat`.

## Demo script (90 seconds)

| # | Prompt | Expected |
| --- | --- | --- |
| 1 | `Hi, how are you?` | 🟢 LOCAL · default short query |
| 2 | `My IBAN is IT60X0542811101000000123456, can you explain how it works?` | 🟢 LOCAL · **PII detected** |
| 3 | `Write me a detailed comparative analysis between REST and GraphQL with code examples in 500 words` | 🔵 CLOUD · long prompt / complex task |
| 4 | Switch to `Rule-based + SLM` and try: `Invent a 200-word fairy tale for me` | 🔵 CLOUD (decided by the SLM) |
| 5 | Show the counter: *"we served N% of the queries locally, saving $X of cloud"* |

## Suggested prompts by strategy

### 🔀 Rule-based

Deterministic strategy based on keywords, regex and thresholds. Predictable and explainable.

| # | Prompt | Expected result |
|---|--------|-----------------|
| 1 | `Hi, how are you?` | 🟢 LOCAL · short and simple query (default) |
| 2 | `What's the weather in Milan today?` | 🟢 LOCAL · short and simple query (default) |
| 3 | `Tell me a joke` | 🟢 LOCAL · short and simple query (default) |
| 4 | `My fiscal code is RSSMRA85M01H501Z, is it correct?` | 🟢 LOCAL · fiscal-code regex detected |
| 5 | `My IBAN is IT60X0542811101000000123456, can you verify it?` | 🟢 LOCAL · IBAN regex detected |
| 6 | `Send the confirmation to mario.rossi@example.com` | 🟢 LOCAL · email regex detected |
| 7 | `The employee's net salary is €2,500` | 🟢 LOCAL · sensitive keyword: "salary" |
| 8 | `The patient's diagnosis indicates a wrist fracture` | 🟢 LOCAL · sensitive keywords: "diagnosis" / "patient" |
| 9 | `Explain step by step how the TCP/IP protocol works` | 🔵 CLOUD · complexity keyword: "explain step by step" |
| 10 | `Write a program in Python that sorts a list and handles exceptions, with unit tests and full documentation for each method, including a usage example and instructions to deploy it on a production server with Docker and Kubernetes` | 🔵 CLOUD · complexity keyword: "write a program" |

### 🔀🤖 Rule-based + SLM fallback

Hard rules (PII, complexity, length) apply first; ambiguous cases are decided by the local SLM via a `{target, reason}` JSON.

| # | Prompt | Expected result |
|---|--------|-----------------|
| 1 | `Good morning!` | 🟢 LOCAL · SLM: chitchat/simple greeting |
| 2 | `What is machine learning in two words?` | 🟢 LOCAL · SLM: short and simple question |
| 3 | `Translate "good morning" into Japanese` | 🟢 LOCAL · SLM: simple translation task |
| 4 | `The internal contract provides for a 10% bonus` | 🟢 LOCAL · hard rule: keyword "internal contract" |
| 5 | `Credit card 4111 1111 1111 1111 has been blocked` | 🟢 LOCAL · hard rule: keyword "credit card" + card-number regex |
| 6 | `Provide a deep analysis of the pros and cons of microservices vs monolith` | 🔵 CLOUD · hard rule: keyword "deep analysis" |
| 7 | `Invent a 200-word fairy tale set in space` | 🔵 CLOUD · SLM: long-form creative writing |
| 8 | `Write a program for a React web app with authentication and full CRUD` | 🔵 CLOUD · hard rule: keyword "write a program" |
| 9 | `Compare the ARM and x86 design patterns from the energy, performance and cost perspectives` | 🔵 CLOUD · hard rule: keyword "design pattern" |
| 10 | `What are the 3 most used languages in 2025?` | 🟢 LOCAL · SLM: short factual question |

### 🟢 Always Local

All requests are sent to the local model (SLM), regardless of content. Useful to maximise privacy and minimise latency/cost.

| # | Prompt | Expected result |
|---|--------|-----------------|
| 1 | `Hi, introduce yourself` | 🟢 LOCAL · always local |
| 2 | `Write a 1000-word essay on artificial intelligence` | 🟢 LOCAL · always local (even if complex) |
| 3 | `Explain step by step the theory of relativity` | 🟢 LOCAL · always local (ignores complexity keywords) |
| 4 | `My IBAN is IT60X0542811101000000123456` | 🟢 LOCAL · always local |
| 5 | `Compare Python and JavaScript in detail` | 🟢 LOCAL · always local |

### 🔵 Always Cloud

All requests are sent to the cloud model (LLM), regardless of content. Useful when maximum model capability is required.

| # | Prompt | Expected result |
|---|--------|-----------------|
| 1 | `Hi, how's it going?` | 🔵 CLOUD · always cloud |
| 2 | `What time is it?` | 🔵 CLOUD · always cloud (even if trivial) |
| 3 | `My fiscal code is RSSMRA85M01H501Z` | 🔵 CLOUD · always cloud (⚠️ PII sent to the cloud!) |
| 4 | `Write a complete compiler in Rust` | 🔵 CLOUD · always cloud |
| 5 | `Tell me a fun fact` | 🔵 CLOUD · always cloud |

> **Note:** With the *Always Cloud* strategy, sensitive data (PII) is **not protected** by local routing. Use with caution in real-world contexts.

## Extension points

- Change the local model (`qwen2.5-3b`, `llama-3.2-3b`...) in `appsettings.json`.
- Tune pricing for `gpt-4o`, `gpt-4o-mini` via the `InputPricePer1K` / `OutputPricePer1K` properties.
- Add a third route (e.g. a code-specialised model) as an additional keyed `IChatClient` and a new router target.
