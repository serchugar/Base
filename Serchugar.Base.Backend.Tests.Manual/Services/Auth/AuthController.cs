using Microsoft.AspNetCore.Mvc;
using Serchugar.Base.Backend.Tests.Manual.Services.Users;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService service) : BaseController
{
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] UserDTO request) =>
        SetResponse(await service.RegisterAsync(request), true, typeof(UserController));

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody] UserDTO request) =>
        SetResponse(await service.LoginAsync(request));

}