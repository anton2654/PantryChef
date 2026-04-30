using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PantryChef.Web.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            var headers = request.Headers.ToDictionary(header => header.Key, header => header.Value.ToString());
            var requestBody = await ReadRequestBodyAsync(request);
            var userId = GetCurrentUserId(context);

            _logger.LogInformation(
                "HTTP Request: {Method} {Url}; IP: {IpAddress}; UserId: {UserId}; Headers: {@Headers}; Body: {Body}",
                request.Method,
                url,
                ipAddress,
                userId,
                headers,
                requestBody);

            await _next(context);
        }

        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength is null || request.ContentLength == 0)
            {
                return string.Empty;
            }

            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return body;
        }

        private static string GetCurrentUserId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return "anonymous";
            }

            return context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.Identity?.Name
                ?? "authenticated-user";
        }
    }
}