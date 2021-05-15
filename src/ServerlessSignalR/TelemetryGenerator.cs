using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ServerlessSignalR
{
    public static class TelemetryGenerator
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
            [SignalRConnectionInfo(ConnectionStringSetting = "TelemetrySignalR_ConnectionString", HubName = "telemetry")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("SubscribeToDevice")]
        public static async Task SubscribeToDeviceAsync(
            [SignalRTrigger("telemetry",
                            "messages",
                            "SubscribeToDevice",
                            parameterNames: new []{ "device" }
                            // Note that the ConnectionStringSetting propertyname is commented out.
                            // This is done because the SignalRTrigger doesn't seem to work
                            // when specifying a custom ConnectionStringSetting.
                            // That's why we're falling back for this one to the default setting-name
                            // which is AzureSignalRConnectionString.
                            // (That's also the reason why we duplicate both settings in the Azure FunctionApp's settings.
                            // GH Issue: https://github.com/Azure/azure-functions-signalrservice-extension/issues/207
                            , ConnectionStringSetting = "TelemetrySignalR_ConnectionString"
            )] InvocationContext invocationContext,
            string device,
            ILogger logger)
        {
            logger.LogInformation("User = " + invocationContext.UserId);

            var groupManager = await invocationContext.GetGroupsAsync();

            await groupManager.AddToGroupAsync(invocationContext.ConnectionId, device);

            logger.LogInformation("Subscribed to " + device);
        }

        private static readonly Random tempGenerator = new Random();

        [FunctionName(nameof(TelemetryGenerator))]
        public static async Task Run(
            [TimerTrigger("*/5 * * * * *")] TimerInfo timer,
            [SignalR(ConnectionStringSetting = "TelemetrySignalR_ConnectionString", HubName = "telemetry")] IAsyncCollector<SignalRMessage> outputMessages,
            ILogger log)
        {

            var temperature = tempGenerator.Next(-15, 40);

            var message = $"\"temperature\": {temperature}";

            await outputMessages.AddAsync(new SignalRMessage
            {
                Target = "TemperatureUpdate",
                Arguments = new object[] { message },
                GroupName = (temperature % 2 == 0) ? "eventempgenerator" : "oddtempgenerator"
            });

        }
    }
}

