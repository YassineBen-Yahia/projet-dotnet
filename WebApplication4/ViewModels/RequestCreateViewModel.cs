using System.ComponentModel.DataAnnotations;

namespace WebApplication4.ViewModels;

public class RequestCreateViewModel
{
    [Required]
    public Guid PropertyId { get; set; }

    [Required]
    [Display(Name = "Notes")]
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    public string? PropertyTitle { get; set; }
}

