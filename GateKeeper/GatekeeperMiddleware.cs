using Microsoft.Extensions.Primitives;

namespace GateKeeper;

public class GatekeeperMiddleware(RequestDelegate next)
{
    private const string GatekeeperTokenHeader = "X-Gatekeeper-Token";
    private const string ValidToken = "your-secret-token";

    public async Task Invoke(HttpContext context)
    {
        if (!(context.User.Identity?.IsAuthenticated ?? true))
        {
            context.Abort();
            return;
        }

        if (!context.Request.Headers.TryGetValue(GatekeeperTokenHeader, out StringValues token) || token != ValidToken)
        {
            context.Abort();
            return;
        }

        if (!IsValidDevice(context.Request.Headers))
        {
            context.Abort();
            return;
        }

        if (context.Request.Path == "/process" && context.Request.Method == "POST")
        {
            ProcessRequest payload = await DecryptPayload(context.Request.Body);
            if (payload == null || payload.Action != "register" || string.IsNullOrWhiteSpace(payload.Username) || string.IsNullOrWhiteSpace(payload.Password))
            {
                context.Abort();
                return;
            }

            payload.Username = WebUtility.HtmlEncode(payload.Username);
            payload.Password = WebUtility.HtmlEncode(payload.Password);

            context.Request.Path = "/register";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new RegisterRequest
            (
                Username: payload.Username,
                Password: payload.Password
            ))));
            context.Request.ContentType = "application/json";
        }

        await next(context);
    }

    private bool IsValidDevice(IHeaderDictionary headers)
    {
        // Validate custom headers
        if (!headers.TryGetValue("X-Device-ID", out StringValues deviceId) || string.IsNullOrEmpty(deviceId) ||
            !headers.TryGetValue("X-Device-Name", out StringValues deviceName) || string.IsNullOrEmpty(deviceName) ||
            !headers.TryGetValue("X-API-Key", out StringValues apiKey) || apiKey != "your-secure-api-key")
        {
            return false;
        }

        // Additional validation logic for device ID and name can be added here
        return true;
    }

    private async Task<ProcessRequest> DecryptPayload(Stream body)
    {
        using StreamReader reader = new StreamReader(body);
        string encryptedPayload = await reader.ReadToEndAsync();
        string decryptedPayload = Decrypt(encryptedPayload);
        return JsonSerializer.Deserialize<ProcessRequest>(decryptedPayload);
    }

    private string Decrypt(string encryptedPayload)
    {
        // Implement decryption logic
        return encryptedPayload; // Replace with actual decryption logic
    }
}