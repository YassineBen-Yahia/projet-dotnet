using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication4.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public List<Property> OwnedProperties { get; set; } = new();

    [InverseProperty("FromUser")]
    public List<Message> SentMessages { get; set; } = new();

    [InverseProperty("ToUser")]
    public List<Message> ReceivedMessages { get; set; } = new();

    public List<Request> Requests { get; set; } = new();
}
