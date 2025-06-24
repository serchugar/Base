using Microsoft.EntityFrameworkCore;
using Serchugar.Base.Backend.Tests.Manual.Data;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Users;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context, "User", "Users")
{
    public async Task<Response<IEnumerable<User>>> GetAllAsync() =>
        await base.GetAllAsync();
    
    public async Task<Response<User>> GetByUsernameAsync(string username) =>
        await base.GetFirstByFilterAsync(u => EF.Functions.ILike(u.Username, username));

    public async Task<Response<User>> CreateAsync(User user) =>
        await base.CreateAsync(user);

    public async Task<Response<User>> DeleteAsync(string username)
    {
        var result = await GetByUsernameAsync(username);
        if (result.Code.IsError()) return result;

        return await base.DeleteAsync(result.Data!);
    }
    
    public async Task<Response<bool>> CheckIfUserExistsAsync(string username) =>
        await base.CheckIfEntityExistsAsync(u => EF.Functions.ILike(u.Username, username));
}