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

        options.BeginInvokeJsInterceptor = values =>
        {
            if (values.Identifier == "prompt")
            {
                if (values.Args?.FirstOrDefault() as string == "CancelThis")
                {
                    values.Canceled = true;
                }
                else
                {
                    values.Identifier = "alert";
                }
            }
        };
    });

await builder.Build().RunAsync();
