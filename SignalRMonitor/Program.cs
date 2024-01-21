using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using SignalRMonitor;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddSignalR();
builder.Services.AddScoped<SignalRAlertHub>();

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<SignalRAlertHub>("/alerthub");
    endpoints.MapGet("/", context => context.Response.WriteAsync("Hello World!"));
});

var configuration = builder.Configuration;
var signalRConsumer = SignalREventConsumer.Create(configuration);
signalRConsumer.SubscribeToEvents();


app.Run();
