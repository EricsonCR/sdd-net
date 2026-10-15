namespace RealtorApi.Domain.Properties;

public class PropertyStatus
{
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
