using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Serchugar.Base.Backend.Tests.Manual.ValueObjects;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Users;

[Table("users", Schema = "base")]
public class User
{
    [Column("id"), Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("username"), Required, MaxLength(64)]
    public string Username { get; set; } = null!;
    
    [Column("password_hash"), Required, MaxLength(84)]
    public string PasswordHash { get; set; } = null!;
    
    // Owned tables
    public AuditInfo AuditInfo { get; set; } = new();
}