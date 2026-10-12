using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApi.Infrastructure.Validation;

public static class ValidationServiceCollectionExtensions
{
    public static IServiceCollection RegisterValidators(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        return services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped);
    }
}