namespace Server.Middleware;

public static class WebSocketServerMiddlewareExtensions
{
	public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder) => builder.UseMiddleware<WebSocketServerMiddleware>();
}
