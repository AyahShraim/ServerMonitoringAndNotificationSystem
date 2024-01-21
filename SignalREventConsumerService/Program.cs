using Microsoft.Extensions.Logging;
using SignalREventConsumerService.Extensions;
using SignalREventConsumerService.Services;

var configuration = SignalRConfigHelper.GetConfiguration();

string SignalRUrl = configuration.GetSection("SignalRConfig:SignalRUrl").Value;
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = loggerFactory.CreateLogger("SignalRLogger");

var signalRConsumer = new SignalREventConsumer(SignalRUrl, logger);
await signalRConsumer.StartAsync();
signalRConsumer.SubscribeToEvents();
await Task.Delay(Timeout.Infinite);


