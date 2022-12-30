using Microsoft.JSInterop;

namespace Blazor.JsRuntimeRedirect.Demo.Pages;

partial class Index
{

    string modulePath = "/_content/interop.js";
    string redirectedPath = "";

    protected override void OnInitialized()
    {
        this.OnModulePathChanged(this.modulePath);
    }

    void OnModulePathChanged(string value)
    {
        this.modulePath = value;

        var js = this.Js as RedirectJsRuntime;
        ArgumentNullException.ThrowIfNull(js);

        this.redirectedPath = js.ResolveUrl(this.modulePath);
    }

    async Task ShowMessageAsync()
    {
        

        var mod = await this.Js.InvokeAsync<IJSObjectReference>(
            "import", this.modulePath);
        await mod.InvokeVoidAsync("sayHello");
    }

}
