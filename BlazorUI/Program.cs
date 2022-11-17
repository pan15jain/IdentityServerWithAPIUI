using BlazorUI;
using BlazorUI.Handlers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("API", httpclient =>
{
    httpclient.BaseAddress = new Uri("https://api:7001");
}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = builder.Configuration["Authentication:Authority"];
    options.ProviderOptions.ClientId = builder.Configuration["Authentication:ClientId"];
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("https://www.trimark.com/api");
});
builder.Services.AddScoped<ApiAuthorizationMessageHandler>();
await builder.Build().RunAsync();
