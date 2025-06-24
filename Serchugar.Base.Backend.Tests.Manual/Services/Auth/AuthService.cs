using Microsoft.AspNetCore.Identity;
using Serchugar.Base.Backend.Tests.Manual.Services.Users;
using Serchugar.Base.Shared;

namespace Serchugar.Base.Backend.Tests.Manual.Services.Auth;

public class AuthService(UserRepository users)
{
    public async Task<Response<User>> RegisterAsync(UserDTO request)
    {
        Response<bool> result = await users.CheckIfUserExistsAsync(request.Username);
        if (result.Code.IsError()) result.MapErrorResponse<User>();

        bool userExists = result.Data;
        if (userExists) return Response<User>.FromError(ResponseCodes.Conflict, "Username already exists");

        User newUser = new();
        string hashedPassword = new PasswordHasher<User>().HashPassword(newUser, request.Password);
        
        newUser.Username = request.Username;
        newUser.PasswordHash = hashedPassword;
        
        return await users.CreateAsync(newUser);
    }

    public async Task<Response<string>> LoginAsync(UserDTO request)
    {
        Response<User> result = await users.GetByUsernameAsync(request.Username);
        if(result.Code == ResponseCodes.NotFound) return Response<string>.FromError(ResponseCodes.BadRequest, "Invalid username or password");
        if(result.Code.IsError()) result.MapErrorResponse<Response<string>>();
        
        User user = result.Data!;
        if(new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Response<string>.FromError(ResponseCodes.BadRequest, "Invalid username or password");

        return Response<string>.FromSuccess(ResponseCodes.Success, $"Login successful! Welcome back, {user.Username}");
    }
}