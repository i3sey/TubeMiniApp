using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace TubeMiniApp.API.Middleware;

public class TelegramAuthorizationMiddleware
{
    private const string TelegramHeaderName = "X-Telegram-Init-Data";
    private readonly RequestDelegate _next;
    private readonly ILogger<TelegramAuthorizationMiddleware> _logger;
    private readonly string? _botToken;

    private static readonly HashSet<string> AllowedAnonymousPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/health",
        "/api/healthz",
        "/api/healthcheck",
        "/api/telegram/webhook"  // Вебхук от Telegram
    };

    public TelegramAuthorizationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<TelegramAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _botToken = configuration["Telegram:BotToken"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Разрешить доступ к Swagger
        if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (IsAnonymousPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var initData = context.Request.Headers[TelegramHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(initData))
        {
            _logger.LogWarning("Blocked request without Telegram init data. Path: {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Telegram init data required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_botToken))
        {
            _logger.LogError("Telegram Bot Token is not configured. Rejecting request.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Server configuration error.");
            return;
        }

        if (!ValidateTelegramInitData(initData, _botToken))
        {
            _logger.LogWarning("Blocked request with invalid Telegram init data. Path: {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid Telegram init data.");
            return;
        }

        await _next(context);
    }

    private static bool IsAnonymousPath(PathString path)
    {
        return AllowedAnonymousPaths.Contains(path.Value ?? string.Empty);
    }

    private static bool ValidateTelegramInitData(string initData, string botToken)
    {
        try
        {
            var parsed = QueryHelpers.ParseQuery(initData);

            if (!parsed.TryGetValue("hash", out var hashValues))
            {
                return false;
            }

            var receivedHashHex = hashValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(receivedHashHex))
            {
                return false;
            }

            var dataCheckList = parsed
                .Where(pair => !string.Equals(pair.Key, "hash", StringComparison.OrdinalIgnoreCase))
                .SelectMany(pair => pair.Value.Select(value => $"{pair.Key}={value}"))
                .OrderBy(entry => entry, StringComparer.Ordinal)
                .ToArray();

            var dataCheckString = string.Join("\n", dataCheckList);
            var computedHash = ComputeHash(dataCheckString, botToken);

            var receivedHash = Convert.FromHexString(receivedHashHex);

            return CryptographicOperations.FixedTimeEquals(computedHash, receivedHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ComputeHash(string dataCheckString, string botToken)
    {
        var secretKey = ComputeHmacSha256("WebAppData", botToken);
        return ComputeHmacSha256(secretKey, dataCheckString);
    }

    private static byte[] ComputeHmacSha256(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA256(keyBytes);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static byte[] ComputeHmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }
}
