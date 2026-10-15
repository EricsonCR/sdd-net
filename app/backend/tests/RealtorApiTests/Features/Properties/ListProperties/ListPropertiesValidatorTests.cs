using RealtorApi.Features.Properties.ListProperties;

namespace RealtorApiTests.Features.Properties.ListProperties;

public class ListPropertiesValidatorTests
{
    private readonly ListPropertiesValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_AcceptsMissingParametersAndMaximumPageSize()
    {
        var defaultResult = await _validator.ValidateAsync(new ListPropertiesQuery());
        var maximumResult = await _validator.ValidateAsync(new ListPropertiesQuery
        {
            Page = "1",
            PageSize = "50"
        });

        Assert.True(defaultResult.IsValid);
        Assert.True(maximumResult.IsValid);
    }

    [Theory]
    [InlineData("abc", null)]
    [InlineData("0", null)]
    [InlineData("-1", null)]
    [InlineData(null, "abc")]
    [InlineData(null, "0")]
    [InlineData(null, "51")]
    public async Task ValidateAsync_RejectsInvalidParameters(string? page, string? pageSize)
    {
        var result = await _validator.ValidateAsync(new ListPropertiesQuery
        {
            Page = page,
            PageSize = pageSize
        });

        Assert.False(result.IsValid);
    }
}