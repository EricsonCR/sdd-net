namespace RealtorApi.Infrastructure.Results;

public sealed record Error(string Code, string Title, string Detail, int Status);