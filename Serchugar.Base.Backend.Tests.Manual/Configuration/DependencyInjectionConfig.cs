using Serchugar.Base.Backend.Tests.Manual.Services.Auth;
using Serchugar.Base.Backend.Tests.Manual.Services.Users;

namespace Serchugar.Base.Backend.Tests.Manual.Configuration;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddDependencyInjectionConfig(this IServiceCollection services)
    {
        services.AddScoped<UserRepository>();
        services.AddScoped<AuthService>();
        return services;
    }
}