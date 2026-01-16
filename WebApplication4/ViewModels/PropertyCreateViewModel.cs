using System.ComponentModel.DataAnnotations;

namespace WebApplication4.ViewModels;

public class PropertyCreateViewModel
{
    [Required]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Required]
    [Display(Name = "Price")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Display(Name = "Bedrooms")]
    [Range(0, int.MaxValue)]
    public int Bedrooms { get; set; }

    [Required]
    [Display(Name = "Bathrooms")]
    [Range(0, int.MaxValue)]
    public int Bathrooms { get; set; }

    [Required]
    [Display(Name = "Area (sq ft)")]
    [Range(0, double.MaxValue)]
    public double Area { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Available";

    [Display(Name = "Property Images")]
    public List<IFormFile>? Images { get; set; }
}

