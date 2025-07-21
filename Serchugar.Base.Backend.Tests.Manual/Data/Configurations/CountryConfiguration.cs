using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Serchugar.Base.Backend.Tests.Manual.Services.Countries;

namespace Serchugar.Base.Backend.Tests.Manual.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<CountryModel>
{
    public void Configure(EntityTypeBuilder<CountryModel> builder)
    {
        builder.HasIndex(c => c.Name).IsUnique();
        
        builder.HasMany(c => c.Teams)
            .WithOne(t => t.Country)
            .HasForeignKey(t => t.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.Teams).AutoInclude();
    }
}