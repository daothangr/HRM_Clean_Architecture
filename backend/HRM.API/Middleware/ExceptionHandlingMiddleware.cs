using System.Net;
using System.Text.Json;
using FluentValidation;
using HRM.Domain.Exceptions;

namespace HRM.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode status;
        object body;

        switch (ex)
        {
            case ValidationException ve:
                status = HttpStatusCode.BadRequest;
                body = new
                {
                    message = "Kiểm tra không hợp lệ. Vui lòng điền đúng thông tin.",
                    errors = ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                };
                break;
            case NotFoundException nf:
                status = HttpStatusCode.NotFound;
                body = new { message = nf.Message };
                break;
            case DomainException de:
                status = HttpStatusCode.BadRequest;
                body = new { message = de.Message };
                break;
            case UnauthorizedAccessException ua:
                status = HttpStatusCode.Unauthorized;
                body = new { message = string.IsNullOrEmpty(ua.Message) ? "Unauthorized" : ua.Message };
                break;
            case InvalidOperationException ie:
                status = HttpStatusCode.BadRequest;
                body = new { message = ie.Message };
                break;
            default:
                status = HttpStatusCode.InternalServerError;
                body = new { message = "An error occurred." };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
