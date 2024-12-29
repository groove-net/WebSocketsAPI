using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Server.Middleware;

public class WebSocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
{
	private readonly RequestDelegate _next = next;
	private readonly WebSocketServerConnectionManager _manager = manager;

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.WebSockets.IsWebSocketRequest)
		{
			WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
			Console.WriteLine("WebSocket Connected");

			string ConnID = _manager.AddSocket(websocket);
			await SendConnIDAsync(websocket, ConnID);

			await RecieveMessage(websocket, async (result, buffer) =>
			{
				string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

				switch (result.MessageType)
				{
					case WebSocketMessageType.Text:
						Console.WriteLine($"Message Received: {message}");
						await RouteJSONMessageAsync(message);
						break;

					case WebSocketMessageType.Close:
						string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == websocket).Key;
						Console.WriteLine($"Terminating Connection ID: {id}");

						if (_manager.GetAllSockets().TryRemove(id, out WebSocket? socket))
							await socket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
						break;

					default:
						Console.WriteLine("Unknowen Message Type Recieved");
						break;
				}
			});
		}
		else
			await _next(context);
	}

	public record RouteObj(string From, string To, string Message);
	public async Task RouteJSONMessageAsync(string message)
	{

		RouteObj? routeObj = JsonSerializer.Deserialize<RouteObj>(message);
		routeObj = (routeObj is not null) ? routeObj : new RouteObj("Unknown", "Unknown", "");

		if (!string.IsNullOrWhiteSpace(routeObj.To))
		{
			Console.WriteLine($"Routing Message To {routeObj.To}...");
			if (Guid.TryParse(routeObj.To, out Guid guidOutput) && _manager.GetAllSockets().TryGetValue(routeObj.To, out WebSocket? recipient))
			{
				if (recipient.State == WebSocketState.Open)
				{
					await recipient.SendAsync(Encoding.UTF8.GetBytes(routeObj.Message.ToString()),
							WebSocketMessageType.Text, true, CancellationToken.None);
				}
			}
			else Console.WriteLine("Invalid Recipient");
		}
		else
		{
			Console.WriteLine("Broadcasting...");
			foreach (var socket in _manager.GetAllSockets())
			{
				if (socket.Value.State == WebSocketState.Open)
				{
					await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeObj.Message.ToString()),
					WebSocketMessageType.Text, true, CancellationToken.None);
				}
			}
		}
	}



	private static async Task SendConnIDAsync(WebSocket socket, string connID)
	{
		var buffer = Encoding.UTF8.GetBytes($"ConnID: {connID}");
		await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
	}

	private static async Task RecieveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
	{
		// byte array to get message string within our WebSocket
		var buffer = new byte[1024 * 4];

		while (socket.State == WebSocketState.Open)
		{
			var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
			handleMessage(result, buffer);
		}
	}

}
