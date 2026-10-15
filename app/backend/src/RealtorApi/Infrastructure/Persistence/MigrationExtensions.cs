using Microsoft.EntityFrameworkCore;

namespace RealtorApi.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static async Task MigrateAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
