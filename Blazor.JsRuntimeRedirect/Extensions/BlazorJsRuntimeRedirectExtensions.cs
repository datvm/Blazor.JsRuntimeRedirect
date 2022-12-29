using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;

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

        foreach (var item in services.ToList())
        {
            var type = item.ServiceType;
            if (type.IsAssignableTo(baseType))
            {
                services.RemoveAll(type);
                services.AddSingleton(type, implementationType);
            }
        }

        return services;
    }

}
