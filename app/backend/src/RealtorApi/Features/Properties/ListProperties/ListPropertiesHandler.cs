using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.ListProperties;

public sealed class ListPropertiesHandler(
    AppDbContext context,
    IConfiguration configuration,
    ILogger<ListPropertiesHandler> logger) : IHandler
{
    public async Task<Result<ListPropertiesResponse>> HandleAsync(
        ListPropertiesQuery request,
        string scheme,
        HostString host,
        CancellationToken cancellationToken)
    {
        var allowedHosts = configuration["Properties:AllowedHosts"]?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        if (!ListPropertiesMapping.IsAllowedHost(host.Host, allowedHosts))
        {
            logger.LogWarning("Rejected properties listing for unallowed host {Host}", host.Host);
            return Result<ListPropertiesResponse>.Failure(new Error(
                "properties.invalid_host",
                "Host no permitido",
                "La solicitud no puede generar enlaces para este host.",
                StatusCodes.Status400BadRequest));
        }

        var page = request.Page is null
            ? 1
            : int.Parse(request.Page, NumberStyles.None, CultureInfo.InvariantCulture);
        var pageSize = request.PageSize is null
            ? 6
            : int.Parse(request.PageSize, NumberStyles.None, CultureInfo.InvariantCulture);

        var totalItems = await context.Properties.LongCountAsync(cancellationToken);
        var totalPages = totalItems == 0
            ? 0
            : totalItems / pageSize + (totalItems % pageSize == 0 ? 0 : 1);

        var projections = new List<ListPropertyProjection>();
        var offset = ((long)page - 1) * pageSize;
        if (page <= totalPages && offset <= int.MaxValue)
        {
            projections = await context.Properties
                .AsNoTracking()
                .OrderBy(property => property.Title)
                .ThenBy(property => property.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .Select(property => new ListPropertyProjection(
                    property.Id,
                    property.Title,
                    property.Description,
                    property.Address,
                    property.Price,
                    property.PropertyStatus.Value,
                    property.BedroomCount,
                    property.BathroomCount,
                    property.AreaSquareMeters,
                    property.ImageUrl))
                .ToListAsync(cancellationToken);
        }

        var items = new List<ListPropertyResponse>(projections.Count);
        foreach (var property in projections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var imageUrl = ListPropertiesMapping.CreateImageUrl(
                property.ImageUrl,
                scheme,
                host,
                allowedHosts);

            if (imageUrl.IsFailure)
            {
                logger.LogError(
                    "Property {PropertyId} has an invalid persisted image path",
                    property.Id);
                return Result<ListPropertiesResponse>.Failure(imageUrl.Error!);
            }

            items.Add(new ListPropertyResponse(
                property.Id,
                property.Title,
                property.Description,
                property.Address,
                property.Price,
                property.Status,
                property.BedroomCount,
                property.BathroomCount,
                property.AreaSquareMeters,
                imageUrl.Value!));
        }

        return Result<ListPropertiesResponse>.Success(new ListPropertiesResponse(
            items,
            page,
            pageSize,
            totalItems,
            totalPages,
            page < totalPages,
            page > 1));
    }
}