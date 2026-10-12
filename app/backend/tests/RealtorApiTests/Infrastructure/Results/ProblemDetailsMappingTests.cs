using Microsoft.AspNetCore.Http;
using RealtorApi.Infrastructure.Results;

namespace RealtorApiTests.Infrastructure.Results;

public sealed class ProblemDetailsMappingTests
{
    [Fact]
    public void ToProblemDetails_CopiesErrorStatusAndPayload()
    {
        var error = new Error("property.not-found", "Not found", "The property was not found.", StatusCodes.Status404NotFound);
        var result = Result<string>.Failure(error);

        var problem = ProblemDetailsMapping.ToProblemDetails(result);

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not found", problem.Title);
        Assert.Equal("The property was not found.", problem.Detail);
        Assert.Equal("property.not-found", problem.Extensions["code"]);
    }
}