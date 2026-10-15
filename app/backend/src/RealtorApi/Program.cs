using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.HttpOverrides;
using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;
using RealtorApi.Infrastructure.Validation;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
var trustedProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
var allowedHosts = builder.Configuration["AllowedHosts"]?
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? [];

if (allowedHosts.Length == 0 || allowedHosts.Contains("*", StringComparer.Ordinal))
{
    throw new InvalidOperationException("AllowedHosts must contain explicit host names.");
}

builder.Configuration["Properties:AllowedHosts"] = string.Join(';', allowedHosts);
builder.Configuration["AllowedHosts"] = "*";

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

if (trustedProxies.Length > 0)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
        options.KnownProxies.Clear();
        options.KnownIPNetworks.Clear();
        foreach (var proxy in trustedProxies)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy));
        }

        options.ForwardLimit = 1;
    });
}

var app = builder.Build();

if (trustedProxies.Length > 0)
{
    app.UseForwardedHeaders();
}

app.Use(async (httpContext, next) =>
{
    if (!RealtorApi.Features.Properties.ListProperties.ListPropertiesMapping.IsAllowedHost(
            httpContext.Request.Host.Host,
            allowedHosts))
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Host no permitido",
                Detail = "La solicitud no puede generar enlaces para este host."
            },
            cancellationToken: httpContext.RequestAborted);
        return;
    }

    await next(httpContext);
});

app.MapSliceEndpoints();

if (hasRealConnection)
{
    await app.MigrateAsync();
}

await DatabaseSeeder.SyncRuntimeAssetsAsync();

var imageProvider = new PhysicalFileProvider(Path.Combine(AppContext.BaseDirectory, "wwwroot", "images"));

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = imageProvider,
    RequestPath = "/assets"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = imageProvider,
    RequestPath = "/images"
});

app.Run();

public partial class Program { }
