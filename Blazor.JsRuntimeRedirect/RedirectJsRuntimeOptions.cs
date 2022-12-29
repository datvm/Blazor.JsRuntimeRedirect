﻿using Microsoft.JSInterop;

namespace Blazor.JsRuntimeRedirect;
public class RedirectJsRuntimeOptions
{

    public bool ShouldRedirect { get; set; } = true;
    public string ContentBefore { get; set; } = "_content/";
    public string ContentAfter { get; set; } = "content/";
    public StringComparison ContentComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    public Action<RedirectJsInvoke>? BeginInvokeJsInterceptor { get; set; }

}

public record RedirectJsInvoke(
    long AsyncHandle,
    string Identifier,
    string? ArgsJson,
    JSCallResultType ResultType,
    long TargetInstanceId,
    bool Canceled
);