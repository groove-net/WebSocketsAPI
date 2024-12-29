Now, we have a central server as a hub and we can have multiple client connected to our server and they can indiviaully send messages to the server. The problem is the these clients are unaware of each other so what we need to do is add a Manager class to allows us to perform some routing and more especially identify the client endpoints to allow for that routing. We are going to introduce a manager and as part of that manager we are going to track our connections and give them a unique ID that we can then use to perform some kind of routing functionality.

using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Server.Middleware;

public class WebSocketServerConnectionManager
{
private readonly ConcurrentDictionary<string, WebSocket> \_sockets = [];

    public ConcurrentDictionary<string, WebSocket> GetAllSockets() => _sockets;

    public string AddSocket(WebSocket socket)
    {
    	string ConnID = Guid.NewGuid().ToString();
    	_sockets.TryAdd(ConnID, socket);
    	Console.WriteLine($"Connection Added: {ConnID}");


    	return ConnID;
    }

}

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Server.Middleware;

public class WebSocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
{
private readonly RequestDelegate \_next = next;
private readonly WebSocketServerConnectionManager \_manager = manager;

    public async Task InvokeAsync(HttpContext context)
    {
    	if (context.WebSockets.IsWebSocketRequest)
    	{
    		WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
    		Console.WriteLine("WebSocket Connected");

    		string ConnID = _manager.AddSocket(websocket); // Add this
    		await SendConnIDAsync(websocket, ConnID); // Add this

    		await RecieveMessage(websocket, async (result, buffer) =>
    		{
    			switch (result.MessageType)
    			{
    				case WebSocketMessageType.Text:
    					Console.WriteLine($"Message Received: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
    					break;
    				case WebSocketMessageType.Close:
    					Console.WriteLine("Received Close Message");
    					break;
    				default:
    					Console.WriteLine("Unknowen Message Type Recieved");
    					break;
    			}
    		});
    	}
    	else
    	{
    		await _next(context);
    	}
    }
    // Add this method
    private async Task SendConnIDAsync(WebSocket socket, string connID)
    {
    	var buffer = Encoding.UTF8.GetBytes($"ConnID: {connID}");
    	await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    //...

}

Then over in the client, let's update the UI if we get a connection ID by inserting the following code in the socket.onmessage event:

// When we recieve a message from the server the socket.onmessage event is fired and it the message is stored in event.data
socket.onmessage = function (event) {
commsLog.innerHTML +=
'<tr>' +
'<td>Server</td>' +
'<td>Client</td>' +
'<td>' +
htmlFormat(event.data) +
'</td>' +
'</tr>';

    // If we get a message beginning with 'ConnID:', we update the ConnID label in the UI with the rest of the message
    if (str.substring(0, 7) == 'ConnID:')
        connID.innerHTML = 'ConnID: ' + str.substring(8, str.length);

};
