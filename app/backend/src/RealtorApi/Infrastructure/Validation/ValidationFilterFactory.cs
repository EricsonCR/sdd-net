using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace RealtorApi.Infrastructure.Validation;

public static class ValidationFilterFactory
{
    private delegate ValueTask<ValidationResult> ValidatorExecution(
        object argument,
        IServiceProvider services,
        CancellationToken cancellationToken);

    private sealed record ParameterValidator(int Index, ValidatorExecution Execute);

    public static EndpointFilterDelegate Create(
        EndpointFilterFactoryContext factoryContext,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(factoryContext);
        ArgumentNullException.ThrowIfNull(next);

        var parameters = factoryContext.MethodInfo.GetParameters();
        var serviceProbe = factoryContext.ApplicationServices.GetService<IServiceProviderIsService>();
        var parameterValidators = parameters
            .Select((parameter, index) => new
            {
                parameter.ParameterType,
                Index = index
            })
            .Where(parameter =>
                !parameter.ParameterType.IsByRef &&
                !parameter.ParameterType.ContainsGenericParameters &&
                serviceProbe?.IsService(typeof(IValidator<>).MakeGenericType(parameter.ParameterType)) == true)
            .Select(parameter => new ParameterValidator(
                parameter.Index,
                CreateValidatorExecution(parameter.ParameterType)))
            .ToArray();

        return async invocationContext =>
        {
            foreach (var parameter in parameterValidators)
            {
                var argument = invocationContext.Arguments[parameter.Index];
                if (argument is null)
                {
                    continue;
                }

                var validationResult = await parameter.Execute(
                    argument,
                    invocationContext.HttpContext.RequestServices,
                    invocationContext.HttpContext.RequestAborted);

                var problem = CreateValidationProblem(validationResult);
                if (problem is not null)
                {
                    return problem;
                }
            }

            return await next(invocationContext);
        };
    }

    public static async ValueTask<IResult?> ValidateArgumentAsync(
        object? argument,
        Type argumentType,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(argumentType);
        ArgumentNullException.ThrowIfNull(services);

        if (argument is null)
        {
            return null;
        }

        var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
        var serviceProbe = services.GetService<IServiceProviderIsService>();
        if (serviceProbe?.IsService(validatorType) != true)
        {
            return null;
        }

        var result = await CreateValidatorExecution(argumentType)(argument, services, cancellationToken);
        return CreateValidationProblem(result);
    }

    private static IResult? CreateValidationProblem(ValidationResult result)
    {
        if (result.IsValid)
        {
            return null;
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return Microsoft.AspNetCore.Http.Results.ValidationProblem(
            errors,
            statusCode: StatusCodes.Status400BadRequest);
    }

    private static ValidatorExecution CreateValidatorExecution(Type argumentType)
    {
        var factory = typeof(ValidationFilterFactory).GetMethod(
            nameof(CreateTypedValidatorExecution),
            BindingFlags.NonPublic | BindingFlags.Static)!;

        return (ValidatorExecution)factory.MakeGenericMethod(argumentType).Invoke(null, null)!;
    }

    private static ValidatorExecution CreateTypedValidatorExecution<T>()
    {
        return async (argument, services, cancellationToken) =>
        {
            var validator = services.GetRequiredService<IValidator<T>>();
            return await validator.ValidateAsync((T)argument, cancellationToken);
        };
    }
}