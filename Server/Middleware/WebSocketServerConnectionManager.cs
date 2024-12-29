using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Server.Middleware;

public class WebSocketServerConnectionManager
{
	private readonly ConcurrentDictionary<string, WebSocket> _sockets = [];


	// Methods
	public ConcurrentDictionary<string, WebSocket> GetAllSockets() => _sockets;

	public string AddSocket(WebSocket socket)
	{
		string ConnID = Guid.NewGuid().ToString();
		_sockets.TryAdd(ConnID, socket);
		Console.WriteLine($"Connection Added: {ConnID}");


		return ConnID;
	}
}
