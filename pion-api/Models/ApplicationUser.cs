using Microsoft.AspNetCore.Identity;

namespace pion_api.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}