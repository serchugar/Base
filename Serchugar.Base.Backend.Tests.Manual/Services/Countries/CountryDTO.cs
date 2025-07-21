using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Countries;

[NotMapped]
public class CountryDTO
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;
}