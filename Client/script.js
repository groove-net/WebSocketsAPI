var connectionUrl = document.getElementById('connectionUrl');
var connectButton = document.getElementById('connectButton');
var stateLabel = document.getElementById('stateLabel');
var sendMessage = document.getElementById('sendMessage');
var sendButton = document.getElementById('sendButton');
var commsLog = document.getElementById('commsLog');
var closeButton = document.getElementById('closeButton');
var recipients = document.getElementById('recipients');
var connID = document.getElementById('connIDLabel');

connectionUrl.value = 'http://localhost:5199'; // Server location

// Connect Button
//------------------------------------------------------------
connectButton.onclick = function () {
  // Initialize WebSocket connection to server
  stateLabel.innerHTML = 'Attempting to connect...';
  socket = new WebSocket(connectionUrl.value);

  socket.onopen = OnOpen;
  socket.onclose = OnClose;
  socket.onmessage = OnMessage;
};

// Update UI State & Append to our Communications Log
OnOpen = function (event) {
  // Update UI State depending on the state of our connection
  updateState();
  commsLog.innerHTML += '<tr><td colspan="3">Connection opened</td></tr>';
};

// Close Connection Button
//------------------------------------------------------------
closeButton.onclick = function () {
  // If you don't have a WebSocket Connection or the WebSocket Connection State is not OPEN...
  if (!socket || socket.readyState !== WebSocket.OPEN) {
    alert('Socket not connected.'); // then alert the user that there is no WebSocket Connection
  }
  // else close the Connection
  socket.close(1000, 'Closing from client');
};

OnClose = function (event) {
  // Update UI State depending on the state of our connection
  updateState();
  commsLog.innerHTML += '<tr><td colspan="3">Connection closed</td></tr>';
};

// Send Message Button
//------------------------------------------------------------
sendButton.onclick = function () {
  // If you don't have a WebSocket Connection or the WebSocket Connection State is not OPEN...
  if (!socket || socket.readyState !== WebSocket.OPEN) {
    alert('Socket not connected.'); // then alert the user that there is no WebSocket Connection
  }
  // else get the data from the input and send it to the server
  var data = constructJSON();
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

// Receive Message Event
//------------------------------------------------------------
// When we recieve a message from the server the socket.onmessage event is fired and it the message is stored in event.data
OnMessage = function (event) {
  commsLog.innerHTML +=
    '<tr>' +
    '<td>Server</td>' +
    '<td>Client</td>' +
    '<td>' +
    htmlFormat(event.data) +
    '</td>' +
    '</tr>';

  // If we get a message beginning with 'ConnID:', we update the ConnID label in the UI with the rest of the message
  if (event.data.substring(0, 7) == 'ConnID:')
    connID.innerHTML = 'ConnID: ' + event.data.substring(8, event.data.length);
};

function constructJSON() {
  return JSON.stringify({
    From: connID.innerHTML.substring(8, connID.innerHTML.length),
    To: recipients.value,
    Message: sendMessage.value,
  });
}

function htmlFormat(str) {
  return str
    .toString()
    .replace(/&/g, '&amp;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}

function updateState() {
  function disable() {
    sendMessage.disabled = true;
    sendButton.disabled = true;
    closeButton.disabled = true;
    recipients.disabled = true;
  }
  function enable() {
    sendMessage.disabled = false;
    sendButton.disabled = false;
    closeButton.disabled = false;
    recipients.disabled = false;
  }
  connectionUrl.disabled = true;
  connectButton.disabled = true;
  if (!socket) {
    disable();
  } else {
    switch (socket.readyState) {
      case WebSocket.CLOSED:
        stateLabel.innerHTML = 'Closed';
        connID.innerHTML = 'ConnID: N/a';
        disable();
        connectionUrl.disabled = false;
        connectButton.disabled = false;
        break;
      case WebSocket.CLOSING:
        stateLabel.innerHTML = 'Closing...';
        disable();
        break;
      case WebSocket.CONNECTING:
        stateLabel.innerHTML = 'Connecting...';
        disable();
        break;
      case WebSocket.OPEN:
        stateLabel.innerHTML = 'Open';
        enable();
        break;
      default:
        stateLabel.innerHTML =
          'Unkown WebSocket State: ' + htmlFormat(socket.readyState);
        disable();
        break;
    }
  }
}
