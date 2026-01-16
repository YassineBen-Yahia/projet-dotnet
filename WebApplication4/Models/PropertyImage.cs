namespace WebApplication4.Models;

public class PropertyImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Url { get; set; } = string.Empty;

    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
}
