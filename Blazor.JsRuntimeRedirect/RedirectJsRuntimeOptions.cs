namespace Blazor.JsRuntimeRedirect;
public class RedirectJsRuntimeOptions
{

    public bool ShouldRedirect { get; set; } = true;
    public string RedirectBefore { get; set; } = "_content";
    public string RedirectAfter { get; set; } = "content";
    public HashSet<string>? RedirectIdentifiers { get; set; } = new() { "import" };
    
    public Action<RedirectJsInvoke>? BeginInvokeJsInterceptor { get; set; }

}

public class RedirectJsInvoke
{
    public RedirectJsInvoke(string identifier, object?[]? args, bool canceled = false)
    {
        this.Identifier = identifier;
        this.Args = args;
        this.Canceled = canceled;
    }

    public string Identifier { get; set; }
    public object?[]? Args { get; set; }
    public bool Canceled { get; set; }
}
