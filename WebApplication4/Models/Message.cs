using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string FromUserId { get; set; } = string.Empty;
    [Required]
    public string ToUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(FromUserId))]
    public ApplicationUser? FromUser { get; set; }

    [ForeignKey(nameof(ToUserId))]
    public ApplicationUser? ToUser { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
