using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Users;

[NotMapped]
public class UserDTO
{
    [Key]
    public Guid Id { get; set; }
    
    [Required, MaxLength(64)]
    public string Username { get; set; } = null!;
    
    [Required, MaxLength(128)]
    public string Password { get; set; } = null!;
}