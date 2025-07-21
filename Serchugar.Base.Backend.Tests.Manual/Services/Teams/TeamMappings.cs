using Riok.Mapperly.Abstractions;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Teams;

[Mapper]
public static partial class TeamMappings
{
    public static partial TeamDTO Map(this TeamModel source);
    public static partial TeamModel Map(this TeamDTO source);
    public static partial IEnumerable<TeamDTO> Map(this IEnumerable<TeamModel> source);
    public static partial IEnumerable<TeamModel> Map(this IEnumerable<TeamDTO> source);
    public static partial Response<TeamDTO> Map(this Response<TeamModel> source);
    public static partial Response<IEnumerable<TeamDTO>> Map(this Response<IEnumerable<TeamModel>> source);
}