using Blazor.JsRuntimeRedirect.Demo;
using Blazor.JsRuntimeRedirect.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
    .AddJsRuntimeRedirect(options =>
    {
        options.RedirectAfter = "testcontent";
        options.RedirectIdentifiers = new(StringComparer.OrdinalIgnoreCase) { "import", };
    });

await builder.Build().RunAsync();
