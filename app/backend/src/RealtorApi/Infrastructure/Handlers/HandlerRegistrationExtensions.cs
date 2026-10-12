using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApi.Infrastructure.Handlers;

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        var handlerTypes = assembly.DefinedTypes
            .Where(type =>
                type.IsClass &&
                (type.IsPublic || type.IsNestedPublic) &&
                !type.IsAbstract &&
                !type.ContainsGenericParameters &&
                typeof(IHandler).IsAssignableFrom(type))
            .Select(type => type.AsType());

        foreach (var handlerType in handlerTypes)
        {
            var isRegistered = services.Any(descriptor =>
                descriptor.ServiceType == handlerType &&
                descriptor.ImplementationType == handlerType);

            if (!isRegistered)
            {
                services.AddScoped(handlerType);
            }
        }

        return services;
    }
}