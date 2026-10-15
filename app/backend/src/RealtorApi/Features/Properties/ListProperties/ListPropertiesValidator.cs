using System.Globalization;
using FluentValidation;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesValidator : AbstractValidator<ListPropertiesQuery>
{
    public ListPropertiesValidator()
    {
        RuleFor(query => query.Page)
            .Must(value => value is null || IsPositiveInteger(value))
            .WithMessage("page debe ser un entero positivo.")
            .OverridePropertyName("page");

        RuleFor(query => query.PageSize)
            .Must(value => value is null || IsPageSize(value))
            .WithMessage("pageSize debe ser un entero entre 1 y 50.")
            .OverridePropertyName("pageSize");
    }

    private static bool IsPositiveInteger(string value)
    {
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            && number > 0;
    }

    private static bool IsPageSize(string value)
    {
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
            && number is > 0 and <= 50;
    }
}