namespace HybridPingPong.Web.Localization;

public static class Translations
{
    public static readonly IReadOnlyList<(string Code, string Label)> Supported = new[]
    {
        ("it", "Italiano"),
        ("en", "English"),
    };

    public static bool IsSupported(string code) =>
        Supported.Any(s => s.Code == code);

    private static readonly Dictionary<string, Dictionary<string, string>> Map = new()
    {
        ["it"] = new()
        {
            ["layout.errorOccurred"] = "Si è verificato un errore imprevisto.",
            ["layout.reload"] = "Ricarica",
            ["common.language"] = "Lingua",

            ["chat.pageTitle"] = "Hybrid Ping-Pong",
            ["chat.subtitle"] = "Foundry Local ↔ Azure AI Foundry — demo di instradamento intelligente",
            ["chat.routing"] = "Instradamento",
            ["chat.strategy.ruleBased"] = "Basato su regole",
            ["chat.strategy.ruleBasedPlusSlm"] = "Regole + SLM",
            ["chat.strategy.alwaysCloud"] = "Sempre Cloud",
            ["chat.strategy.alwaysLocal"] = "Sempre Locale",
            ["chat.newChat"] = "Nuova Chat",
            ["chat.stats.turns"] = "Turni",
            ["chat.stats.local"] = "🟢 Locale",
            ["chat.stats.cloud"] = "🔵 Cloud",
            ["chat.stats.cloudCost"] = "💰 Costo cloud",
            ["chat.empty.title"] = "Prova questi esempi 👇",
            ["chat.empty.example1"] = "Ciao, come stai?",
            ["chat.empty.example1.desc"] = "chat breve → 🟢 locale",
            ["chat.empty.example2"] = "Il mio IBAN è IT60X0542811101000000123456",
            ["chat.empty.example2.desc"] = "🟢 locale (dati sensibili)",
            ["chat.empty.example3"] = "Scrivimi un'analisi comparativa dettagliata tra REST e GraphQL con esempi di codice",
            ["chat.empty.example3.desc"] = "🔵 cloud",
            ["chat.badge.local"] = "🟢 LOCALE",
            ["chat.badge.cloud"] = "🔵 CLOUD",
            ["chat.badge.routedBy"] = "instradato da",
            ["chat.badge.why"] = "perché?",
            ["chat.badge.ttft"] = "ttft",
            ["chat.placeholder"] = "Scrivi il tuo messaggio... (Invio per inviare, Maiusc+Invio per andare a capo)",
            ["chat.send"] = "Invia",
            ["chat.sending"] = "...",

            ["error.pageTitle"] = "Errore",
            ["error.h1"] = "Errore.",
            ["error.h2"] = "Si è verificato un errore durante l'elaborazione della richiesta.",
            ["error.requestId"] = "ID richiesta:",
            ["error.devMode"] = "Modalità sviluppo",
            ["error.devModeP1"] = "Passando all'ambiente Development verranno visualizzate informazioni più dettagliate sull'errore verificatosi.",
            ["error.devModeP2.warn"] = "L'ambiente Development non dovrebbe essere abilitato per le applicazioni in produzione.",
            ["error.devModeP2.rest"] = "Potrebbe mostrare informazioni sensibili delle eccezioni agli utenti finali. Per il debug locale, abilitare l'ambiente Development impostando la variabile d'ambiente ASPNETCORE_ENVIRONMENT su Development e riavviando l'applicazione.",

            ["notfound.title"] = "Pagina non trovata",
            ["notfound.body"] = "Il contenuto che stai cercando non esiste.",
        },
        ["en"] = new()
        {
            ["layout.errorOccurred"] = "An unexpected error has occurred.",
            ["layout.reload"] = "Reload",
            ["common.language"] = "Language",

            ["chat.pageTitle"] = "Hybrid Ping-Pong",
            ["chat.subtitle"] = "Foundry Local ↔ Azure AI Foundry — intelligent routing demo",
            ["chat.routing"] = "Routing",
            ["chat.strategy.ruleBased"] = "Rule-based",
            ["chat.strategy.ruleBasedPlusSlm"] = "Rules + SLM",
            ["chat.strategy.alwaysCloud"] = "Always Cloud",
            ["chat.strategy.alwaysLocal"] = "Always Local",
            ["chat.newChat"] = "New Chat",
            ["chat.stats.turns"] = "Turns",
            ["chat.stats.local"] = "🟢 Local",
            ["chat.stats.cloud"] = "🔵 Cloud",
            ["chat.stats.cloudCost"] = "💰 Cloud cost",
            ["chat.empty.title"] = "Try these examples 👇",
            ["chat.empty.example1"] = "Hi, how are you?",
            ["chat.empty.example1.desc"] = "short chat → 🟢 local",
            ["chat.empty.example2"] = "My IBAN is IT60X0542811101000000123456",
            ["chat.empty.example2.desc"] = "🟢 local (sensitive data)",
            ["chat.empty.example3"] = "Write a detailed comparative analysis between REST and GraphQL with code examples",
            ["chat.empty.example3.desc"] = "🔵 cloud",
            ["chat.badge.local"] = "🟢 LOCAL",
            ["chat.badge.cloud"] = "🔵 CLOUD",
            ["chat.badge.routedBy"] = "routed by",
            ["chat.badge.why"] = "why?",
            ["chat.badge.ttft"] = "ttft",
            ["chat.placeholder"] = "Type your message... (Enter to send, Shift+Enter for newline)",
            ["chat.send"] = "Send",
            ["chat.sending"] = "...",

            ["error.pageTitle"] = "Error",
            ["error.h1"] = "Error.",
            ["error.h2"] = "An error occurred while processing your request.",
            ["error.requestId"] = "Request ID:",
            ["error.devMode"] = "Development Mode",
            ["error.devModeP1"] = "Swapping to the Development environment displays detailed information about the error that occurred.",
            ["error.devModeP2.warn"] = "The Development environment shouldn't be enabled for deployed applications.",
            ["error.devModeP2.rest"] = "It can result in displaying sensitive information from exceptions to end users. For local debugging, enable the Development environment by setting the ASPNETCORE_ENVIRONMENT environment variable to Development and restarting the app.",

            ["notfound.title"] = "Page not found",
            ["notfound.body"] = "The content you are looking for does not exist.",
        },
    };

    public static string Get(string language, string key)
    {
        if (Map.TryGetValue(language, out var dict) && dict.TryGetValue(key, out var value))
            return value;
        if (Map[LanguageService.DefaultLanguage].TryGetValue(key, out var fallback))
            return fallback;
        return key;
    }
}
