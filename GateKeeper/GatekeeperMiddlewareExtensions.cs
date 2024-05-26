namespace GateKeeper;

public static class GatekeeperMiddlewareExtensions
{
    public static IApplicationBuilder UseGatekeeper(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GatekeeperMiddleware>();
    }
}