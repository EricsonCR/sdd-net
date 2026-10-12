using RealtorApi.Infrastructure.Endpoints;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Validation;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.Services.RegisterSlices(assembly);
builder.Services.RegisterHandlers(assembly);
builder.Services.RegisterValidators(assembly);

var app = builder.Build();

app.MapSliceEndpoints();

app.Run();

public partial class Program { }
