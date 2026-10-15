using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApiTests.Infrastructure.Persistence;

public class PropertyImageAssetTests
{
    [Fact]
    public async Task SyncRuntimeAssetsAsync_CopiesImagesToRuntimeFolder()
    {
        await DatabaseSeeder.SyncRuntimeAssetsAsync();

        var runtimeFolder = Path.Combine(AppContext.BaseDirectory, "wwwroot", "images", "properties");
        Assert.True(Directory.Exists(runtimeFolder));
        Assert.Contains("1.png", Directory.GetFiles(runtimeFolder).Select(Path.GetFileName));
    }
}
