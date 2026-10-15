using Microsoft.AspNetCore.Mvc;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesQuery
{
    [FromQuery(Name = "page")]
    public string? Page { get; init; }

    [FromQuery(Name = "pageSize")]
    public string? PageSize { get; init; }
}

public sealed record ListPropertyResponse(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    string Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string ImageUrl);

public sealed record ListPropertiesResponse(
    IReadOnlyList<ListPropertyResponse> Items,
    int Page,
    int PageSize,
    long TotalItems,
    long TotalPages,
    bool HasNext,
    bool HasPrevious);

internal sealed record ListPropertyProjection(
    Guid Id,
    string Title,
    string Description,
    string Address,
    decimal Price,
    string Status,
    int BedroomCount,
    int BathroomCount,
    decimal AreaSquareMeters,
    string ImageUrl);