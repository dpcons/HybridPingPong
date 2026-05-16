using HybridPingPong.Components;
using HybridPingPong.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Hybrid chat backends (Foundry Local + Azure Foundry)
builder.Services.AddHybridChatBackends(builder.Configuration);

// Routers: keyed by strategy enum so the UI can switch live
builder.Services.AddKeyedSingleton<IHybridRouter, RuleBasedRouter>(RouterStrategy.RuleBased);
builder.Services.AddKeyedSingleton<IHybridRouter, SlmFallbackRouter>(RouterStrategy.RuleBasedPlusSlm);

builder.Services.AddSingleton<ChatOrchestrator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
