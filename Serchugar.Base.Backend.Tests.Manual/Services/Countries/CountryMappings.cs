using Riok.Mapperly.Abstractions;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Countries;

[Mapper]
public static partial class CountryMappings
{
    public static partial CountryDTO Map(this CountryModel source);
    public static partial CountryModel Map(this CountryDTO source);
    public static partial IEnumerable<CountryDTO> Map(this IEnumerable<CountryModel> source);
    public static partial IEnumerable<CountryModel> Map(this IEnumerable<CountryDTO> source);
    public static partial Response<CountryDTO> Map(this Response<CountryModel> source);
    public static partial Response<IEnumerable<CountryDTO>> Map(this Response<IEnumerable<CountryModel>> source);
}