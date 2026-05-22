using HybridPingPong.Web.Components;
using HybridPingPong.Core.Models;
using HybridPingPong.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Hybrid chat backends (Foundry Local + Azure Foundry)
builder.Services.AddHybridChatBackends(builder.Configuration);

// Routers: keyed by strategy enum so the UI can switch live
builder.Services.AddKeyedSingleton<IModelRouter, RuleBasedRouter>(RouterStrategy.RuleBased);
builder.Services.AddKeyedSingleton<IModelRouter, SlmFallbackRouter>(RouterStrategy.RuleBasedPlusSlm);
builder.Services.AddKeyedSingleton<IModelRouter, AlwaysCloudRouter>(RouterStrategy.AlwaysCloud);
builder.Services.AddKeyedSingleton<IModelRouter, AlwaysLocalRouter>(RouterStrategy.AlwaysLocal);

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
