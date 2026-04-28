using System.Text;
using Serilog;

namespace HRM.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private const int MaxBodyLength = 4096;
    private static readonly Serilog.ILogger RequestLogger = Log.ForContext<RequestResponseLoggingMiddleware>();
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestBody = await ReadRequestBodyAsync(context.Request);

        var originalResponseBody = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            var responseBody = await ReadResponseBodyAsync(context.Response);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await context.Response.Body.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            RequestLogger.Information(
                "HTTP {Method} {Path} responded {StatusCode}. RequestBody: {RequestBody}. ResponseBody: {ResponseBody}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                requestBody,
                responseBody);
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0 || !IsTextBasedContentType(request.ContentType))
        {
            return "<empty-or-non-text>";
        }

        request.EnableBuffering();
        request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Seek(0, SeekOrigin.Begin);
        return Truncate(body);
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
    {
        if (!IsTextBasedContentType(response.ContentType))
        {
            return "<empty-or-non-text>";
        }

        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return Truncate(body);
    }

    private static string Truncate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "<empty>";
        }

        return input.Length <= MaxBodyLength
            ? input
            : $"{input[..MaxBodyLength]}...(truncated)";
    }

    private static bool IsTextBasedContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("text/", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);
    }
}
