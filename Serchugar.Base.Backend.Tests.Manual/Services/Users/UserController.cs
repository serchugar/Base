using Microsoft.AspNetCore.Mvc;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Users;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserRepository users) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll() => SetResponse(await users.GetAllAsync());
    
    [HttpGet("{username}")]
    public async Task<ActionResult<User>> GetByNameExact(string username) => SetResponse(await users.GetByUsernameAsync(username));

    [HttpDelete("{username}")]
    public async Task<ActionResult<User>> Delete(string username) => SetResponse(await users.DeleteAsync(username));
}