using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServerlessSignalR.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string url = "https://sls-function.azurewebsites.net/api";

            var connection = new HubConnectionBuilder()
                             .WithUrl(url)
                             .WithAutomaticReconnect()
                             .ConfigureLogging(logging =>
                             {
                                 logging.AddConsole();
                                 logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Warning);
                             }).Build();

            connection.On<object>("TemperatureUpdate", m =>
            {
                Console.WriteLine(m);
            });

            await connection.StartAsync();

            var dice = new Random().Next(1, 6);

            string deviceName = "eventempgenerator";

            if (dice % 2 != 0)
            {
                deviceName = "oddtempgenerator";
            }

            await connection.InvokeAsync("SubscribeToDevice", deviceName);

            Console.WriteLine($"Connected to the {deviceName} Telemetry notification service! - Press any key to quit");

            Console.WriteLine("Telemetry Updates that are send are displayed here:");

            Console.ReadLine();
        }
    }
}
