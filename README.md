# Hybrid Ping-Pong 🏓

Demo Blazor Server in C# (.NET 10) che mostra una chat semplice il cui **router intelligente** decide a ogni turno se rispondere via **Foundry Local** o **Azure AI Foundry (cloud)**.

Pensata per la sessione *"The Foundry Forge: Crafting AI Solutions from Azure to the Edge"*.

## Cosa fa

- Una sola chat, due backend (entrambi OpenAI-compatible) esposti come `IChatClient` (Microsoft.Extensions.AI).
- **Router switchabile live dalla UI**:
  - **Rule-based** — regole esplicite (PII, IBAN, codice fiscale, email, complessità, contesto lungo). Deterministico e spiegabile sul palco.
  - **Rule-based + SLM fallback** — regole hard prima; nei casi grigi un mini-classifier su Foundry Local emette JSON `{target,reason}`.
- Badge live per ogni risposta: 🟢 LOCAL / 🔵 CLOUD · modello · latenza · time-to-first-token · costo stimato · *"why?"* del router.
- Counter cumulativo in alto: turns, % local, $ spesi sul cloud.

## Prerequisiti

1. **.NET 10 SDK** (la repo usa `net10.0`).
2. **Foundry Local** in esecuzione con un modello caricato, es.:
   ```powershell
   foundry model run phi-4-mini
   ```
   Verifica l'endpoint OpenAI-compatible su `http://localhost:5273/v1`.
3. (Opzionale ma consigliato per la demo completa) Una **Azure AI Foundry / Azure OpenAI** deployment, es. `gpt-4o-mini`.

## Configurazione

Modifica `appsettings.json` (o usa user-secrets / variabili d'ambiente):

```json
"FoundryLocal": {
  "Endpoint": "http://localhost:5273/v1",
  "Model": "phi-4-mini"
},
"AzureFoundry": {
  "Endpoint": "https://your-resource.openai.azure.com/",
  "ApiKey": "...",
  "Deployment": "gpt-4o-mini"
}
```

Con user-secrets:
```powershell
dotnet user-secrets init
dotnet user-secrets set "AzureFoundry:Endpoint" "https://..."
dotnet user-secrets set "AzureFoundry:ApiKey" "..."
```

## Run

```powershell
dotnet run
```

Apri `https://localhost:7xxx/chat`.

## Script demo (90 secondi)

| # | Prompt | Atteso |
|---|---|---|
| 1 | `Ciao, come stai?` | 🟢 LOCAL · default short query |
| 2 | `Il mio IBAN è IT60X0542811101000000123456, puoi spiegarmi come funziona?` | 🟢 LOCAL · **PII detected** |
| 3 | `Scrivimi un'analisi comparativa dettagliata tra REST e GraphQL con esempi di codice in 500 parole` | 🔵 CLOUD · long prompt / complex task |
| 4 | Switch a `Rule-based + SLM` e prova: `Inventami una favola di 200 parole` | 🔵 CLOUD (deciso dall'SLM) |
| 5 | Mostra il counter: *"abbiamo servito N% delle query in locale, risparmiando $X di cloud"* |

## Punti di estensione

- Cambia modello locale (`qwen2.5-3b`, `llama-3.2-3b`...) in `appsettings.json`.
- Tuning pricing per `gpt-4o`, `gpt-4o-mini` nelle proprietà `InputPricePer1K` / `OutputPricePer1K`.
- Aggiungi una terza route (es. modello specializzato code) come ulteriore `IChatClient` keyed e nuovo target del router.
