using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApi.Infrastructure.Endpoints;

public static class SliceRegistrationExtensions
{
    public static IServiceCollection RegisterSlices(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        var sliceTypes = assembly.DefinedTypes
            .Where(type =>
                type.IsClass &&
                (type.IsPublic || type.IsNestedPublic) &&
                !type.IsAbstract &&
                !type.ContainsGenericParameters &&
                typeof(ISlice).IsAssignableFrom(type))
            .Select(type => type.AsType());

        foreach (var sliceType in sliceTypes)
        {
            var isRegistered = services.Any(descriptor =>
                descriptor.ServiceType == typeof(ISlice) &&
                descriptor.ImplementationType == sliceType);

            if (!isRegistered)
            {
                services.AddSingleton(typeof(ISlice), sliceType);
            }
        }

        return services;
    }
}