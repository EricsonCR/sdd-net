using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Validation;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var hasRealConnection = !string.IsNullOrWhiteSpace(connectionString)
    && !connectionString.Contains("[YOUR-PASSWORD]", StringComparison.OrdinalIgnoreCase)
    && !builder.Environment.IsEnvironment("Testing");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (hasRealConnection)
    {
        options.UseNpgsql(connectionString)
            .UseSeeding((context, _) => DatabaseSeeder.Seed(context))
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
                await DatabaseSeeder.SeedAsync(context, cancellationToken));
        return;
    }

    options.UseNpgsql("Host=localhost;Port=5432;Database=RealtorApi_Design;Username=postgres;Password=postgres")
        .UseSeeding((context, _) => DatabaseSeeder.Seed(context))
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
            await DatabaseSeeder.SeedAsync(context, cancellationToken));
});

builder.Services.RegisterSlices(assembly);
builder.Services.RegisterHandlers(assembly);
builder.Services.RegisterValidators(assembly);

var app = builder.Build();

app.MapSliceEndpoints();

if (hasRealConnection)
{
    await app.MigrateAsync();
}

await DatabaseSeeder.SyncRuntimeAssetsAsync();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot", "images")),
    RequestPath = "/images"
});

app.Run();

public partial class Program { }
