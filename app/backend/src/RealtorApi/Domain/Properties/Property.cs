namespace RealtorApi.Domain.Properties;

public class Property
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public PropertyStatus PropertyStatus { get; set; } = null!;
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaSquareMeters { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
