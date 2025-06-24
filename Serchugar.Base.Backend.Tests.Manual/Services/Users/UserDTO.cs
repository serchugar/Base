using System.ComponentModel.DataAnnotations;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Users;

public class UserDTO
{
    [Required, MaxLength(64)]
    public string Username { get; set; } = null!;
    
    [Required, MaxLength(128)]
    public string Password { get; set; } = null!;
}