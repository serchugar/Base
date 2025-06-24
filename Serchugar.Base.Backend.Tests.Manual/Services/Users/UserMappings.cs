using Riok.Mapperly.Abstractions;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Users;

[Mapper]
public static partial class UserMappings
{
    public static partial UserDTO Map(this User source);
    public static partial User Map(this UserDTO source);
    public static partial IEnumerable<UserDTO> Map(this IEnumerable<User> source);
    public static partial IEnumerable<User> Map(this IEnumerable<UserDTO> source);
    public static partial Response<UserDTO> Map(this Response<User> source);
    public static partial Response<IEnumerable<UserDTO>> Map(this Response<IEnumerable<User>> source);
}