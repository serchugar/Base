using Microsoft.OpenApi.Models;

namespace Serchugar.Base.Backend.Tests.Manual.Configuration;

public static class Swagger
{
    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddSwaggerGen(opts =>
        {
            opts.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Base.Backend.Tests.Manual",
                Version = "v0.1.0-preview.1"
            });
        });
        return services;
    }

    public static IApplicationBuilder UseSwaggerConfig(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            opts.SwaggerEndpoint("/swagger/v1/swagger.json", "Base.Backend.Tests.Manual API v1.0");
            opts.RoutePrefix = string.Empty;
        });
        return app;
    }
}