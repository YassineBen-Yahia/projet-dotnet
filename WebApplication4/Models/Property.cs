using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models;

public class Property
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public decimal Price { get; set; }

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public double Area { get; set; }

    public string Status { get; set; } = "Available";

    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public List<PropertyImage> Images { get; set; } = new();
}
