using Microsoft.JSInterop;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Blazor.JsRuntimeRedirect;

public class RedirectJsRuntime : IJSRuntime
{
    readonly IJSRuntime original;
    readonly IOptions<RedirectJsRuntimeOptions> options;
    public RedirectJsRuntime(IOptions<RedirectJsRuntimeOptions> options, IJSRuntime original)
    {
        this.options = options;
        this.original = original;
    }

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, object?[]? args)
    {
        this.ModifyParameters(ref identifier, ref args);
        return this.original.InvokeAsync<TValue>(identifier, args);
    }

    public ValueTask<TValue> InvokeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        this.ModifyParameters(ref identifier, ref args);
        return this.original.InvokeAsync<TValue>(identifier, cancellationToken, args);
    }

    public string ResolveUrl(string url)
    {
        var o = this.options.Value;
        return ReplaceUrlPath(url, o.RedirectBefore, o.RedirectAfter);
    }

    void ModifyParameters(ref string identifier, ref object?[]? args)
    {
        var o = this.options.Value;

        if (o.BeginInvokeJsInterceptor is not null)
        {
            var values = new RedirectJsInvoke(identifier, args);
            o.BeginInvokeJsInterceptor(values);

            identifier = values.Identifier;
            args = values.Args;
        }

        if (o.ShouldRedirect && args is not null
            && (o.RedirectIdentifiers is null || o.RedirectIdentifiers.Contains(identifier)))
        {
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    var v = args[i];
                    if (v is not string s) { continue; }

                    if (!Uri.TryCreate(
                        s, UriKind.RelativeOrAbsolute,
                        out _)) { continue; }

                    var replaced = ReplaceUrlPath(s, o.RedirectBefore, o.RedirectAfter);
                    if (replaced != s)
                    {
                        args[i] = replaced;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }

    // Following this Stackoverflow article:
    // https://stackoverflow.com/questions/74961586/how-do-i-use-uribuilder-for-relative-uri-so-that-the-output-is-exactly-like-the
    static string ReplaceUrlPath(string input, string from, string to)
    {
        var firstAmp = input.IndexOf('&');
        var beforeQuery = firstAmp == -1 ? input : input[..firstAmp];

        var rg = new Regex($@"(^|/|//[^/]*/){from}/");
        var replaced = rg.Replace(beforeQuery, $"$1{to}/");

        return firstAmp == -1 ?
            replaced :
            replaced + input[firstAmp..];
    }

}
