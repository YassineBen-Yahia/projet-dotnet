using System.ComponentModel.DataAnnotations;

namespace WebApplication4.ViewModels;

public class MessageCreateViewModel
{
    [Required]
    [Display(Name = "Recipient Email")]
    public string ToUserEmail { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Subject")]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Message")]
    [StringLength(2000)]
    public string Body { get; set; } = string.Empty;
}


