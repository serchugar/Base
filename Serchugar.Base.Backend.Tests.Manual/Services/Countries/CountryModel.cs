using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Serchugar.Base.Backend.Tests.Manual.Services.Teams;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Countries;

// TODO: Añadir controller y repo de countries y team para testear el postgres exception de cuando se intenta borrar un registro que tiene dependencias (delete restricted)
// Habrá que modificar el base repository, el deleteAsync
[Table("countries", Schema = "base")]
public class CountryModel
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("name"), Required, MaxLength(64)]
    public string Name { get; set; } = null!;
    
    
    // Nav props
    public virtual ICollection<TeamModel> Teams { get; set; } = new List<TeamModel>();
    
    // Readonly props
    public int TeamCount => Teams.Count;
}