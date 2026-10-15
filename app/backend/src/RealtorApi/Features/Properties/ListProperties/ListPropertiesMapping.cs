using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using RealtorApi.Infrastructure.Results;

namespace RealtorApi.Features.Properties.ListProperties;

public static class ListPropertiesMapping
{
    private const string ExistingImagePath = "/images/properties/";

    public static Result<string> CreateImageUrl(
        string persistedImageUrl,
        string scheme,
        HostString host,
        IReadOnlyCollection<string> allowedHosts)
    {
        if (!IsAllowedHost(host.Host, allowedHosts) || scheme is not ("http" or "https"))
        {
            return Result<string>.Failure(new Error(
                "properties.invalid_host",
                "Host no permitido",
                "La solicitud no puede generar enlaces para este host o esquema.",
                StatusCodes.Status400BadRequest));
        }

        var fileName = ExtractFileName(persistedImageUrl);
        if (fileName is null)
        {
            return Result<string>.Failure(new Error(
                "properties.invalid_image_path",
                "Imagen de propiedad inválida",
                "La propiedad no contiene un nombre de archivo de imagen utilizable.",
                StatusCodes.Status500InternalServerError));
        }

        var path = new PathString($"/assets/properties/{Uri.EscapeDataString(fileName)}");
        return Result<string>.Success(UriHelper.BuildAbsolute(scheme, host, path: path));
    }

    public static bool IsAllowedHost(string host, IReadOnlyCollection<string> allowedHosts)
    {
        return !string.IsNullOrWhiteSpace(host)
            && allowedHosts.Count > 0
            && !allowedHosts.Contains("*", StringComparer.Ordinal)
            && allowedHosts.Contains(host, StringComparer.OrdinalIgnoreCase);
    }

    private static string? ExtractFileName(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var fileName = imageUrl.StartsWith(ExistingImagePath, StringComparison.OrdinalIgnoreCase)
            ? imageUrl[ExistingImagePath.Length..]
            : imageUrl;

        if (string.IsNullOrWhiteSpace(fileName)
            || fileName is "." or ".."
            || fileName.Contains('/')
            || fileName.Contains('\\')
            || fileName.Contains(':')
            || !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
        {
            return null;
        }

        return fileName;
    }
}