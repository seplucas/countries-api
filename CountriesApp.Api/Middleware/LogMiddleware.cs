using System.Diagnostics;

namespace CountriesApp.Api.Middleware;

public class LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        var ip = context.Connection.RemoteIpAddress?.ToString();

        await next(context);

        stopwatch.Stop();

        if (context.Response.StatusCode >= 400)
        {
            logger.LogError(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms - CorrelationId: {CorrelationId}, IP: {IpAddress}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                ip);
        }
    }
}