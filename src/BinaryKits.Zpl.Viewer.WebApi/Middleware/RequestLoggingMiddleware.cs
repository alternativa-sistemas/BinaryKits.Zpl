using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BinaryKits.Zpl.Viewer.WebApi.Middleware
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

        public async Task Invoke(HttpContext context)
        {
            var req = context.Request;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = req.Path + req.QueryString;
            _logger.LogInformation("HTTP {Method} {Path} from {RemoteIpAddress}", req.Method, path, ip);

            await _next(context);
        }
    }
}