using SignalRMonitor.Hubs;

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


app.Run();
