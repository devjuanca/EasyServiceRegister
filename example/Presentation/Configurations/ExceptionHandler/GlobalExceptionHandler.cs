using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Application.Exceptions;

namespace Presentation.ExceptionHandler;
public class GlobalExceptionHandler
{
    private readonly IDictionary<Type, Func<Exception, HttpContext, Task>> _exceptionHandlers;
    private static IWebHostEnvironment _env;

    public GlobalExceptionHandler(IWebHostEnvironment env)
    {
        _env = env;

        _exceptionHandlers = new Dictionary<Type, Func<Exception, HttpContext, Task>>
            {
                {typeof(ValidationException), HandleValidationException},
                {typeof(NotFoundException), HandleNotFoundException},
                {typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException},
                {typeof(ForbiddenAccessException),  HandleForbiddenAccessException},
            };
    }

    public async Task HandleException(Exception exception, HttpContext context)
    {
        var response = new { error = exception.Message };
        context.Response.ContentType = "application/json";


        Type type = exception.GetType();
        if (_exceptionHandlers.ContainsKey(type))
        {
            await _exceptionHandlers[type].Invoke(exception, context);
            return;
        }

        await HandleUnknownException(exception, context);
    }

    private static async Task HandleUnknownException(Exception exception, HttpContext context)
    {
        ProblemDetails details;
        if (_env.EnvironmentName.ToLower() == "development")
        {
            details = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Detail = $"Message: {exception.Message} | StackTrace:{exception.StackTrace}",
                Title = "An error in development occurred while processing your request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
        }
        else
        {
            details = new ProblemDetails()
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(details), Encoding.UTF8);


    }

    private async Task HandleValidationException(Exception exception, HttpContext context)
    {
        var validationException = exception as ValidationException;

        var details = new ValidationProblemDetails(validationException.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        await context.Response.WriteAsync(JsonSerializer.Serialize(details), Encoding.UTF8);
    }

    private async Task HandleNotFoundException(Exception exception, HttpContext context)
    {
        var notFoundException = exception as NotFoundException;

        var details = new ProblemDetails()
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = notFoundException.Message
        };

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync(JsonSerializer.Serialize(details), Encoding.UTF8);
    }

    private async Task HandleUnauthorizedAccessException(Exception exception, HttpContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync(JsonSerializer.Serialize(details), Encoding.UTF8);
    }

    private async Task HandleForbiddenAccessException(Exception exception, HttpContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        };

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync(JsonSerializer.Serialize(details), Encoding.UTF8);
    }
}

