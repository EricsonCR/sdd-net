using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Infrastructure.Results;

public static class ProblemDetailsMapping
{
    public static ProblemDetails ToProblemDetails<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var error = result.Error ?? throw new ArgumentException(
            "Only failed results can be converted to ProblemDetails.",
            nameof(result));

        var problemDetails = new ProblemDetails
        {
            Title = error.Title,
            Detail = error.Detail,
            Status = error.Status
        };

        problemDetails.Extensions["code"] = error.Code;
        return problemDetails;
    }
}