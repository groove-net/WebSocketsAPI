[The WebSocket API exposes two methods: send() and close(). LEt's add functionality that calls these methods when we click on the [Close Connection] & [Send Message] buttons on the client];

// Close Connection
closeButton.onclick = function () {
// If you don't have a WebSocket Connection or the WebSocket Connection State is not OPEN...
if (!socket || socket.readyState !== WebSocket.OPEN) {
alert('Socket not connected.'); // then alert the user that there is no WebSocket Connection
}
// else close the Connection
socket.close(1000, 'Closing from client');
};

// Send Message
sendButton.onclick = function () {
// If you don't have a WebSocket Connection or the WebSocket Connection State is not OPEN...
if (!socket || socket.readyState !== WebSocket.OPEN) {
alert('Socket not connected.'); // then alert the user that there is no WebSocket Connection
}
// else get the data from the input and send it to the server
var data = sendMessage.value;
socket.send(data);
// update comms log
commsLog.innerHTML +=
'<tr>' +
'<td>Client</td>' +
'<td>Server</td>' +
'<td>' +
htmlFormat(data) +
'</td>' +
'</tr>';
};

[Now, in the server, we will write an asynchronous method to recieve any messages on our websockets. We do this using a while loop that exists under the condition of the WebSocket being open. Because it is an asynchronous method we can pass control back to other parts of the application so we are not blocking it. It is also going to resolve the immediate closure of our WebSocket.]

async Task RecieveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
{
// byte array to get message string within our WebSocket
var buffer = new byte[1024 * 4];

    // this while loop keeps the connection open
    // a singular request to the server starts
    // because this is asynchrous this while loop doesn't block other functionality
    // meaning you can still put http request into the request pipeline while the websocket connection is still running
    // you can have both http and websocket functionality
    while (socket.State == WebSocketState.Open)
    {
    	var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
    	handleMessage(result, buffer);
    }

}

// Implement our own custom middleware
app.Use(async (context, next) =>
{
if (context.WebSockets.IsWebSocketRequest)
{
WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
Console.WriteLine("WebSocket Connected");

    	await RecieveMessage(websocket, async (result, buffer) =>
    	{
    		switch (result.MessageType)
    		{
    			case WebSocketMessageType.Text:
    				Console.WriteLine("Message Received");
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
    	await next();
    }

});
