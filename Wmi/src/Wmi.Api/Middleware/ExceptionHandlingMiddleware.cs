namespace Wmi.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = ex is UnauthorizedAccessException ? 401 : 500;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }
}