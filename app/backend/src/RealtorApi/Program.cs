var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "RealtorApi",
    timestamp = DateTimeOffset.UtcNow
}));

app.Run();

public partial class Program { }
