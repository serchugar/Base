using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Serchugar.Base.Backend.Tests.Manual.Services.Teams;

namespace Serchugar.Base.Backend.Tests.Manual.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<TeamModel>
{
    public void Configure(EntityTypeBuilder<TeamModel> builder)
    {
        builder.HasIndex(t => new {t.Name, t.CountryId}).IsUnique();
        
        builder.Navigation(t => t.Country).AutoInclude();
    }
}