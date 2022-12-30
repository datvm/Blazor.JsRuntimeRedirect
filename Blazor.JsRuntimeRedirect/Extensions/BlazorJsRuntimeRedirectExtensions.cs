using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;

namespace Blazor.JsRuntimeRedirect.Extensions;

public static class BlazorJsRuntimeRedirectExtensions
{

    public static IServiceCollection AddJsRuntimeRedirect(this IServiceCollection services)
        => AddJsRuntimeRedirect(services, null);


    public static IServiceCollection AddJsRuntimeRedirect(this IServiceCollection services, Action<RedirectJsRuntimeOptions>? options)
    {
        if (options is not null)
        {
            services.Configure(options);
        }

        var baseType = typeof(IJSRuntime);
        var implementationType = typeof(RedirectJsRuntime);

        var jsRuntime = services
            .FirstOrDefault(q => q.ServiceType == typeof(IJSRuntime));
        if (jsRuntime is null)
        {
            throw new KeyNotFoundException(
                $"{nameof(IJSRuntime)} is not found in the Service Collection");
        }

        if (jsRuntime.ImplementationInstance 
            is not IJSRuntime originalInstance)
        {
            throw new NullReferenceException(
                $"There is no implementation of {nameof(IJSRuntime)}");
        }

        services.AddSingleton<IJSRuntime>(s => new RedirectJsRuntime(
            s.GetRequiredService<IOptions<RedirectJsRuntimeOptions>>(),
            originalInstance));

        var descendantTypes = services
            .Where(q => 
                q.ServiceType != baseType 
                && q.ServiceType.IsAssignableTo(baseType))
            .ToList();

        foreach (var item in descendantTypes)
        {
            var type = item.ServiceType;

            services.RemoveAll(type);
            services.AddSingleton(type, s => s.GetRequiredService<IJSRuntime>());
        }

        return services;
    }

}
