This project is an attempt to fix mingyaulee/Blazor.BrowserExtension#43 where Blazor for Chrome Extension cannot use Razor Class Library (RCL) assets due to `_content` being prohibited in Chrome Extension folders. It's also unlikely that there would be an official fix per dotnet/aspnetcore#45531.

You can also use this library as a custom interceptor for `IJsRuntime` that perform custom logic before calling `InvokeAsync`. It's recommended for Blazor WebAssembly only.

See [`Demo`](./Demo) folder for sample of a WebAssembly project as well as a Blazor Chrome Extension project using [mingyaulee/Blazor.BrowserExtension](https://github.com/mingyaulee/Blazor.BrowserExtension) and [KristofferStrube/Blazor.FileSystemAccess](https://github.com/KristofferStrube/Blazor.FileSystemAccess) RCL.

# Installation & Setup

## Nuget Package

Install this library through [Nuget package `Blazor.JsRuntimeRedirect`](https://www.nuget.org/packages/Blazor.JsRuntimeRedirect):

```
dotnet add package Blazor.JsRuntimeRedirect
```

## Setup Dependency Injection

Call `AddJsRuntimeRedirect` to replace current implementation of `IJsRuntime` to the library's wrapper:

```cs
builder.Services
    // Other services
    .AddJsRuntimeRedirect();
```

> **Warning**  
> `AddJsRuntimeRedirect` searches the current `IServiceCollection` for `IJsRuntime`'s `ImplementationInstance`. If there is none, exception would be thrown.

`AddJsRuntimeRedirect` wraps the current `ImplementationInstance` of `IJsRuntime` to perform additional logic before calling their original methods.

# Options

By default, the library attempt to redirect all `import` calls with `_content` in the path to `content`. This would solve mingyaulee/Blazor.BrowserExtension#43. You can override the settings using [Options pattern's `Configure`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-7.0) or simply configure it with `AddJsRuntimeRedirect` method:

```cs
builder.Services
    .AddJsRuntimeRedirect(options =>
    {
        options.RedirectAfter = "testcontent";
        options.RedirectIdentifiers = new(StringComparer.OrdinalIgnoreCase) { "import", };
    });
```

- `ShouldRedirect` (default `true`): Whether the Redirect logic should be executed. `RedirectBefore`, `RedirectAfter`, `RedirectIdentifiers` are ignored if this is set to `false`. 

- `RedirectBefore` (default `_content`): the name of the path segment that should be replaced.

- `RedirectAfter` (default `content`): the name of the path segment that should be replaced with.

- `RedirectIdentifiers` (default `{ "import" }`): if set (non-`null`), the redirect logic is only considered if the `identifier` (i.e. the calling method, for example `import` or `alert`) is a value in the list. If set to `null`, every path is considered and therefore is not recommended due to performance impact.

- `BeginInvokeJsInterceptor` (default `null`): A method that is called before any logic is executed. Here you can modify the values of `identifier` and `args`. If you set `Canceled` to `true`, the call would be terminated.

> **Note**  
> `BeginInvokeJsInterceptor` is called before `ShouldRedirect` logic. It's executed even if `ShouldRedirect` is `false`. However if you set `Canceled` to `true`, `ShouldRedirect` logic is not executed as the call is cancelled anyway.

From the demo project, an example of using `BeginInvokeJsInterceptor` which replaces all `prompt` JS call into `alert` and cancel the call altogether if the first argument is `CancelThis`:

```cs
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
```
