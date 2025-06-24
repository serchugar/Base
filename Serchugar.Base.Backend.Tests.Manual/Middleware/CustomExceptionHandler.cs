namespace Serchugar.Base.Backend.Tests.Manual.Middleware;

public class CustomExceptionHandler(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<CustomExceptionHandler> logger)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await SetHttpResponseAsync(context, ex.Message, StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task SetHttpResponseAsync(HttpContext context, string message, int httpStatusCode)
    {
        context.Response.ContentType = "plain/text";
        context.Response.StatusCode = httpStatusCode;
        await context.Response.WriteAsync(message);
    }
}

public static class CustomExceptionHandlerExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app) =>
        app.UseMiddleware<CustomExceptionHandler>();
}