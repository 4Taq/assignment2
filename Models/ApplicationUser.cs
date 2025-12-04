using Microsoft.AspNetCore.Identity;

namespace COMP_2139_Assignment_1.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePicture { get; set; } 
}