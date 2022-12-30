namespace Blazor.JsRuntimeRedirect;
public class RedirectJsRuntimeOptions
{

    public bool ShouldRedirect { get; set; } = true;
    public string RedirectBefore { get; set; } = "_content";
    public string RedirectAfter { get; set; } = "content";
    public HashSet<string>? RedirectIdentifiers { get; set; }

    public StringComparison ContentComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    public Action<RedirectJsInvoke>? BeginInvokeJsInterceptor { get; set; }

}

public record RedirectJsInvoke(
    string Identifier,
    object?[]? Args,
    bool Canceled = false
);