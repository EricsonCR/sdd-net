using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static void Seed(DbContext context)
    {
        SeedAsync(context).GetAwaiter().GetResult();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (context is not AppDbContext appContext)
        {
            return;
        }

        var manifest = await LoadManifestAsync(cancellationToken);
        await SyncRuntimeAssetsAsync(manifest, cancellationToken);

        var propertySeedItems = await LoadPropertiesAsync(manifest, cancellationToken);

        foreach (var seedItem in propertySeedItems)
        {
            var exists = await appContext.Properties.AnyAsync(property => property.Id == seedItem.Id, cancellationToken);
            if (exists)
            {
                continue;
            }

            var status = Enum.TryParse<PropertyStatus>(seedItem.Status, ignoreCase: true, out var parsedStatus)
                ? parsedStatus
                : PropertyStatus.Available;

            var property = new Property
            {
                Id = seedItem.Id,
                Title = seedItem.Title,
                Description = seedItem.Description,
                Address = seedItem.Address,
                Price = seedItem.Price,
                Status = status,
                BedroomCount = seedItem.BedroomCount,
                BathroomCount = seedItem.BathroomCount,
                AreaSquareMeters = seedItem.AreaSquareMeters,
                ImageUrl = BuildPublicImageUrl(manifest, seedItem.ImageUrl),
                CreatedAt = seedItem.CreatedAt,
                UpdatedAt = seedItem.UpdatedAt
            };

            appContext.Properties.Add(property);
        }

        await appContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task SyncRuntimeAssetsAsync(CancellationToken cancellationToken = default)
    {
        var manifest = await LoadManifestAsync(cancellationToken);
        await SyncRuntimeAssetsAsync(manifest, cancellationToken);
    }

    private static async Task SyncRuntimeAssetsAsync(SeedManifest manifest, CancellationToken cancellationToken)
    {
        var sourceFolder = ResolveSeedFilePath(manifest.ImageSourceFolder);
        var runtimeFolder = ResolveRuntimeFilePath(manifest.ImageDestinationFolder);

        Directory.CreateDirectory(runtimeFolder);

        if (!Directory.Exists(sourceFolder))
        {
            return;
        }

        foreach (var sourceFile in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(sourceFolder, sourceFile);
            var destinationFile = Path.Combine(runtimeFolder, relativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationFile);

            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(sourceFile, destinationFile, overwrite: true);
        }
    }

    private static async Task<SeedManifest> LoadManifestAsync(CancellationToken cancellationToken)
    {
        var manifestPath = ResolveSeedFilePath("seed-manifest.json");
        var text = await File.ReadAllTextAsync(manifestPath, cancellationToken);

        return JsonSerializer.Deserialize<SeedManifest>(text, JsonOptions())
            ?? throw new InvalidOperationException("The seed manifest could not be loaded");
    }

    private static async Task<List<SeedPropertyItem>> LoadPropertiesAsync(SeedManifest manifest, CancellationToken cancellationToken)
    {
        var propertiesPath = ResolveSeedFilePath(manifest.PropertiesFile);
        var text = await File.ReadAllTextAsync(propertiesPath, cancellationToken);

        return JsonSerializer.Deserialize<List<SeedPropertyItem>>(text, JsonOptions())
            ?? new List<SeedPropertyItem>();
    }

    private static string ResolveSeedFilePath(string relativePath)
    {
        var root = ResolveSeedRoot();
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(root, normalized.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    }

    private static string ResolveRuntimeFilePath(string relativePath)
    {
        var root = AppContext.BaseDirectory;
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(root, normalized.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    }

    private static string ResolveSeedRoot()
    {
        var candidate = AppContext.BaseDirectory;

        while (!string.IsNullOrWhiteSpace(candidate))
        {
            var seedPath = Path.Combine(candidate, "support", "seed-data");
            if (Directory.Exists(seedPath))
            {
                return seedPath;
            }

            var parent = Directory.GetParent(candidate);
            if (parent is null)
            {
                break;
            }

            candidate = parent.FullName;
        }

        return Path.Combine(AppContext.BaseDirectory, "support", "seed-data");
    }

    private static string BuildPublicImageUrl(SeedManifest manifest, string imageFileName)
    {
        var basePath = manifest.PublicBasePath.TrimEnd('/');
        var fileName = imageFileName.TrimStart('/');
        return $"{basePath}/{fileName}";
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private sealed class SeedManifest
    {
        public string SourceRoot { get; set; } = string.Empty;
        public string PropertyStatusesFile { get; set; } = string.Empty;
        public string PropertiesFile { get; set; } = string.Empty;
        public string ImageSourceFolder { get; set; } = string.Empty;
        public string ImageDestinationFolder { get; set; } = string.Empty;
        public string PublicBasePath { get; set; } = "/images/properties";
    }

    private sealed class SeedPropertyItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public int BedroomCount { get; set; }
        public int BathroomCount { get; set; }
        public decimal AreaSquareMeters { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
