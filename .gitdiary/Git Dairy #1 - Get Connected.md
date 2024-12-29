WebSocketd has two methods (send & close) and sends 3 events (onopen, onclose, onmessage)

First we create a simple index.html

"""

<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8" />
    <title>WebSocket JavaScript Client</title>
  </head>
  <body>
    <h1>WebSocket JavaScript Client</h1>
    <p id="stateLebel">Ready to connect</p>
    <p id="connIDLabel">ConnID: N/a</p>
    <div>
      <label for="connectionUrl">WebSocket Server Url</label>
      <input id="connectionUrl" />
      <button id="connectButton" type="submit">Connect</button>
      <button id="closeButton" disabled>Close Socket</button>
    </div>
    <p></p>
    <div>
      <label for="sendMessage">Message:</label>
      <input id="sendMessage" disabled />
      <button id="sendButton" type="submit" disabled>Send</button>
    </div>
    <p></p>
    <div>
      <label for="recipients">Recipient ID:</label>
      <input id="recipients" disabled />
    </div>
    <p></p>
    <h2>Communication Log</h2>
    <table style="width: 800px">
      <thead>
        <tr>
          <td style="width: 100px">From</td>
          <td style="width: 100px">To</td>
          <td>Data</td>
        </tr>
      </thead>
      <tbody id="commsLog"></tbody>
    </table>
  </body>
</html>
"""

[Insert img here]

var connectionUrl = document.getElementById('connectionUrl');
var connectButton = document.getElementById('connectButton');
var stateLabel = document.getElementById('stateLabel');
var sendMessage = document.getElementById('sendMessage');
var sendButton = document.getElementById('sendButton');
var commsLog = document.getElementById('commsLog');
var closeButton = document.getElementById('closeButton');
var recipients = document.getElementById('recipients');
var connID = document.getElementById('connIDLabel');

```[Setting up these JS object from the HTML DOM]

connectionUrl.value = 'ws://localhost:5000'; // Server location

connectButton.onclick = function () {
  // Initialize WebSocket connection to server
  stateLabel.innerHTML = 'Attempting to connect...';
  socket = new WebSocket(connectionUrl.value);

  // Update UI State depending on the state of our connection
  updateState(socket);

  // Append to our Communication Log
  socket.onopen = function (event) {
    commesLog.innerHTML += '<tr><td colspan="3">Connection opened</td></tr>';
  };

  socket.onclose = function (event) {
    commsLog.innerHTML +=
      '<tr><td colspan="3">Connection closed. Code: ' +
      htmlFormat(event.code) +
      'Reason: ' +
      htmlFormat(event.reason) +
      '</td></tr>';
  };

  socket.onmessage = function (event) {
    commsLog.innerHTML +=
      '<tr>' +
      '<td>Server</td>' +
      '<td>Client</td>' +
      '<td>' +
      htmlFormat(event.data) +
      '</td>' +
      '</tr>';
  };
};

~~~ [When we click the Connect button. we attempt to initialize a WebSocket connection to the server.
     We create a WebSocket object and pass in the connection url (the location of our server). Depending on the state of WebSocket connection,
     we can update the state of our UI then we append to the communication log. Below we look at how the updateState() function updates our UI based on the state of our WebSocket connection. This UpdateState() function take just updates our HTML elements (e.g. buttons and labels) depending on the state of the websocket connection. ]


function updateState() {
  function disable() {
    sendMessage.disable = true;
    sendButton.disable = true;
    closeButton.disable = true;
    recipients.disable = true;
  }
  function enable() {
    sendMessage.disable = false;
    sendButton.disable = false;
    closeButton.disable = false;
    recipients.disable = false;
  }
  connectionUrl.disable = true;
  connectButton.disable = true;
  if (!socket) {
    disable();
  } else {
    switch (socket.readyState) {
      case WebSocket.CLOSED:
        stateLabel.innerHTML = 'Closed';
        connID.innerHTML = 'ConnID: N/a';
        disable();
        connectionUrl.disable = false;
        connectButton.disable = false;
        break;
      case WebSocket.CLOSING:
        stateLabel.innerHTML = 'Closing...';
        disable();
        break;
      case WebSocket.OPEN:
        stateLabel.innerHTML = 'Open';
        enable();
        break;
      default:
        stateLabel.innerHTML =
          'Unkown WebSocket State: ' + htmlFormat(socket.readyState);
    }
  }
}

~~~ [You may have also notice the use of a htmlFormat() function. We use this function when returning any messages to the html client. The function takes a string and replaces any literal string with the HMTL safe equivalent for better formatting on the browser. See below:]

function htmlFormat(str) {
  return str
    .toString()
    .replace(/&/g, '&amp;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}

```

[We then create a new ASP.NET Core Web App called Server. > "dotnet new web -n Server".
Open the Program.cs and add the WebSocket middeware to the request pipeline using the System.Net.WebSockets.]

```

using System.Net.WebSockets; // Add this

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseWebSockets(); // Add this

app.Run()

```

[Middleware (i.e. Request delegates) are items that you can add to your request pipeline.
A number of request delegates acutually make up your request pipeline. The delagates are chained together to form a pipeline and processes the request in a sequence and sometimes the ordering of this sequence matters especially if one delegate should depends on a successful completion of another delagate.
For example if you have an authorization middleware, you want this to process the request perferably after the request has successfully gone through an authentication middleware. The authentication middleware should give the request a check mark before it passes the request on to the next delegate. If the request doesn't make it through the authentication step, then you would want to stop the request pipeline process.
So, you can add mutiple middlewares or delagates, either buit-in or custom, to your pipeline that will intercept and deal with the request and perform some logic before passing the request to the next delegate
Next, we will add our our custom request delegate. On way we do that is with the app.Use() method.]

```
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseWebSockets();

// Implement our own custom middleware
app.Use(async (context, next) =>
{
	// Remember that WebSockets actaully starts with a HTTP request before it attempts to upgrade our connection to WebSocket.
	// Here we check to see if we have a WebScoketRequest
	if (context.WebSockets.IsWebSocketRequest)
	{
		// We use await for this method because we know that it is possible going to take a while to accept the websocket connection
		WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
		Console.WriteLine("WebSocket Connected");
	}
	// if it not a WebSocket request to go to the next middleware/request delegate
	else
	{
		// We use the next keyword to move on to the next middleware/request delegate in the request pipeline
		await next();
	}
});

app.Run()

```

[In the middleware we added above, we only move onto the next request delegate in the pipeline if it is not a Websocket request.
When a request comes in some logic is perform in the app.UseWebSockets middleware,
then it comes into our custom midleware delegate and we determin if it is a WebSocket request. If it is, then we accept the WebSocket connection and short-ciruit (break out of) the pipeline, propagating a response back through the request pipeline and to the client
if is isn't then it to goes on to the next (and last) middleware app.Run() which is the end of the pipeline.
We can test this out too. If we add a terminal middleware delegate to app.Run(), it will only be called when we send a normal requst using the resquest/response model instead of a WebScoket request. Lets try it out:

First lets add a terminal middleware delegate that should be called at the very end of the pipeline.]

```

// Note that since app.Run() signals the end of the request pipeline, we don't need to add the next delegate.
app.Run(async context =>
{
	Console.WriteLine("End of Request Pipeline"); // Writes to our Server console
	await context.Response.WriteAsync("End of Request Pipeline"); // Writes a message in the response of our HTTPContext which is send back to the client
});

```

[Now lets call our server in a new browser, instead of using the WebSocket Client we have built]
[Insert image here]
[You can see that it makes it all the way to the end of the pipeline and the message we wrote to the response in the HTTPContext is propagated back up the pipeline to be sent to the client.]

[If we instead send the requsting using our WebSocket Client, We see from our logs that the connection is indeed open breifly before immediately closing. A WebSocket connection was successful but we need a way to maintain it.]
