using Server.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Configure Services
builder.Services.AddSingleton<WebSocketServerConnectionManager>();

var app = builder.Build();

// Configre Middleware
app.MapGet("/", () => "Hello World!");
app.UseWebSockets(); // Add WebSockets Middleware to the request pipeline
app.UseWebSocketServer(); // Implement our own custom middleware
app.Run(async context =>
{
	Console.WriteLine("End of Request Pipeline"); // Writes to our Server console
	await context.Response.WriteAsync("End of Request Pipeline"); // Writes a message in the response of our HTTPContext which is send back to the client
});

// Run app
app.Run();