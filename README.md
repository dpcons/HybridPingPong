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
 
> Nel caso in cui in Foundry Local si stia utilizzando un modello specifico per una configurazione hardwere, la configurazione `model` in `FoundryLocal` deve essere il nome completo del modello

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

## Prompt suggeriti per strategia

### 🔀 Rule-based

Strategia deterministica basata su keyword, regex e soglie. Prevedibile e spiegabile.

| # | Prompt | Risultato atteso |
|---|--------|-----------------|
| 1 | `Ciao, come stai?` | 🟢 LOCAL · query breve e semplice (default) |
| 2 | `Che tempo fa oggi a Milano?` | 🟢 LOCAL · query breve e semplice (default) |
| 3 | `Raccontami una barzelletta` | 🟢 LOCAL · query breve e semplice (default) |
| 4 | `Il mio codice fiscale è RSSMRA85M01H501Z, è corretto?` | 🟢 LOCAL · regex codice fiscale rilevato |
| 5 | `Il mio IBAN è IT60X0542811101000000123456, puoi verificarlo?` | 🟢 LOCAL · regex IBAN rilevato |
| 6 | `Invia la conferma a mario.rossi@example.com` | 🟢 LOCAL · regex email rilevata |
| 7 | `Lo stipendio netto del dipendente è di 2.500€` | 🟢 LOCAL · keyword sensibile: "stipendio" |
| 8 | `La diagnosi del paziente indica una frattura al polso` | 🟢 LOCAL · keyword sensibile: "diagnosi" / "paziente" |
| 9 | `Spiega passo passo come funziona il protocollo TCP/IP` | 🔵 CLOUD · keyword di complessità: "spiega passo passo" |
| 10 | `Scrivi un programma in Python che ordina una lista e gestisce le eccezioni, con test unitari e documentazione completa per ogni metodo, includendo anche un esempio di utilizzo e le istruzioni per il deploy su un server di produzione con Docker e Kubernetes` | 🔵 CLOUD · prompt lungo (>400 caratteri) + keyword "scrivi un programma" |

### 🔀🤖 Rule-based + SLM fallback

Le regole hard (PII, complessità, lunghezza) si applicano per prime; nei casi ambigui decide l'SLM locale con un JSON `{target, reason}`.

| # | Prompt | Risultato atteso |
|---|--------|-----------------|
| 1 | `Buongiorno!` | 🟢 LOCAL · SLM: chitchat/saluto semplice |
| 2 | `Cos'è il machine learning in due parole?` | 🟢 LOCAL · SLM: domanda breve e semplice |
| 3 | `Traduci "buongiorno" in giapponese` | 🟢 LOCAL · SLM: task semplice di traduzione |
| 4 | `Il contratto interno prevede un bonus del 10%` | 🟢 LOCAL · regola hard: keyword "contratto interno" |
| 5 | `La carta di credito 4111 1111 1111 1111 è stata bloccata` | 🟢 LOCAL · regola hard: keyword "carta di credito" + regex numero carta |
| 6 | `Analizza in dettaglio i pro e contro di microservizi vs monolite` | 🔵 CLOUD · regola hard: keyword "analizza in dettaglio" |
| 7 | `Inventami una favola di 200 parole ambientata nello spazio` | 🔵 CLOUD · SLM: scrittura creativa long-form |
| 8 | `Scrivi un'app web in React con autenticazione e CRUD completo` | 🔵 CLOUD · regola hard: keyword "scrivi un'app" |
| 9 | `Confronta le architetture ARM e x86 dal punto di vista energetico, prestazionale e di costo` | 🔵 CLOUD · regola hard: keyword "architettura" |
| 10 | `Quali sono i 3 linguaggi più usati nel 2025?` | 🟢 LOCAL · SLM: domanda fattuale breve |

### 🟢 Always Local

Tutte le richieste vengono inviate al modello locale (SLM), indipendentemente dal contenuto. Utile per massimizzare privacy e minimizzare latenza/costi.

| # | Prompt | Risultato atteso |
|---|--------|-----------------|
| 1 | `Ciao, presentati` | 🟢 LOCAL · sempre locale |
| 2 | `Scrivi un saggio di 1000 parole sull'intelligenza artificiale` | 🟢 LOCAL · sempre locale (anche se complesso) |
| 3 | `Spiega passo passo la teoria della relatività` | 🟢 LOCAL · sempre locale (ignora keyword complessità) |
| 4 | `Il mio IBAN è IT60X0542811101000000123456` | 🟢 LOCAL · sempre locale |
| 5 | `Confronta in dettaglio Python e JavaScript` | 🟢 LOCAL · sempre locale |

### 🔵 Always Cloud

Tutte le richieste vengono inviate al modello cloud (LLM), indipendentemente dal contenuto. Utile quando serve la massima capacità del modello.

| # | Prompt | Risultato atteso |
|---|--------|-----------------|
| 1 | `Ciao, come va?` | 🔵 CLOUD · sempre cloud |
| 2 | `Che ore sono?` | 🔵 CLOUD · sempre cloud (anche se banale) |
| 3 | `Il mio codice fiscale è RSSMRA85M01H501Z` | 🔵 CLOUD · sempre cloud (⚠️ PII inviato al cloud!) |
| 4 | `Scrivi un compilatore completo in Rust` | 🔵 CLOUD · sempre cloud |
| 5 | `Dimmi una curiosità` | 🔵 CLOUD · sempre cloud |

> **Nota:** Con la strategia *Always Cloud*, i dati sensibili (PII) **non vengono protetti** dal routing locale. Usare con cautela in contesti reali.

## Punti di estensione

- Cambia modello locale (`qwen2.5-3b`, `llama-3.2-3b`...) in `appsettings.json`.
- Tuning pricing per `gpt-4o`, `gpt-4o-mini` nelle proprietà `InputPricePer1K` / `OutputPricePer1K`.
- Aggiungi una terza route (es. modello specializzato code) come ulteriore `IChatClient` keyed e nuovo target del router.
