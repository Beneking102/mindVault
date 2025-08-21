using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MindVault.Client;
using MindVault.Client.Services;
using System.Net.Http;

// Standard Blazor Host
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to your API (adjust URL/port if needed)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });

// Register ApiService (it will receive HttpClient and IJSRuntime via DI)
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
