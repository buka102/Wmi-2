using Microsoft.AspNetCore.Diagnostics;
using Wmi.Api.Models;

namespace Wmi.Api.Middleware;

public class CustomExceptionHandler(IHostEnvironment hostEnvironment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        Result<object> response;
        if (hostEnvironment.IsDevelopment())
        {
            response = new Result<object>(){ Error = exception.Message , ErrorDetails = exception.StackTrace };
        }
        else
        {
            response = new Result<object>(){ Error = "Something went wrong" };
        }
        
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}