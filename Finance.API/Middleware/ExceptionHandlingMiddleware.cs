using Finance.Application.Common.Exceptions;
using Finance.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "Server Error";
        object? errors = null;
        var detail = "An unexpected error occurred.";

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Validation Failed";
                detail = "One or more validation errors occurred.";
                errors = validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray());
                break;

            case DomainException:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Business Rule Violated";
                detail = exception.Message;
                break;

            case UnauthorizedException:
                statusCode = StatusCodes.Status401Unauthorized;
                title = "Unauthorized";
                detail = exception.Message;
                break;

            case ConflictException:
                statusCode = StatusCodes.Status409Conflict;
                title = "Conflict";
                detail = exception.Message;
                break;

            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                title = "Not Found";
                detail = exception.Message;
                break;
        }

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception.");
        }
        else
        {
            logger.LogWarning(exception, "Request failed with a handled exception.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
