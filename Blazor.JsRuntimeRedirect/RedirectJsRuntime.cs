using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Blazor.JsRuntimeRedirect;

internal class RedirectJsRuntime : WebAssemblyJSRuntime
{
    readonly IOptions<RedirectJsRuntimeOptions> options;
    public RedirectJsRuntime(IOptions<RedirectJsRuntimeOptions> options)
    {
        this.options = options;
    }

    protected override void BeginInvokeJS(long asyncHandle, string identifier, string? argsJson, JSCallResultType resultType, long targetInstanceId)
    {
        var o = this.options.Value;

        if (o.BeginInvokeJsInterceptor is not null)
        {
            var values = new RedirectJsInvoke(
                asyncHandle, identifier, argsJson, resultType, targetInstanceId,
                false);

            o.BeginInvokeJsInterceptor(values);
            if (values.Canceled)
            {
                return;
            }

            asyncHandle = values.AsyncHandle;
            identifier = values.Identifier;
            argsJson = values.ArgsJson;
            resultType = values.ResultType;
            targetInstanceId = values.TargetInstanceId;
        }

        if (o.ShouldRedirect && argsJson is not null)
        {
            try
            {
                var arr = JsonSerializer.Deserialize<JsonArray>(argsJson);
                ArgumentNullException.ThrowIfNull(arr);

                var len = arr.Count;
                for (int i = 0; i < len; i++)
                {
                    var item = arr[i];

                    if (item is null
                        || item is not JsonValue jValue
                        || !jValue.TryGetValue<string>(out var value))
                    {
                        continue;
                    }

                    if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri)) { continue; }

                    var segments = uri.Segments.ToArray();
                    var shouldChange = false;
                    for (int j = 0; j < segments.Length; j++)
                    {
                        if (o.ContentBefore.Equals(segments[j], o.ContentComparison))
                        {
                            segments[j] = o.ContentAfter;
                        }

                        shouldChange = true;
                    }

                    if (shouldChange)
                    {
                        var builder = new UriBuilder(uri)
                        {
                            Path = string.Join("", segments)
                        };

                        arr[i] = builder.ToString();
                    }
                    
                }

                argsJson = arr.ToJsonString();
            }
            catch (Exception) { }
        }

        base.BeginInvokeJS(asyncHandle, identifier, argsJson, resultType, targetInstanceId);
    }

}
