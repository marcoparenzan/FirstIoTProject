using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IoTHubEventProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var host = new EventProcessorHost(
                configuration["eventHubPath"],
                configuration["eventHubConsumerGroup"],
                configuration["eventHubConnectionString"],
                configuration["StorageConnectionString"],
                configuration["leaseContainerName"]
            );

            var factory = 
                new IoTHubProcessorFactory(
                    configuration["StorageConnectionString"]
                );

            await host.RegisterEventProcessorFactoryAsync(factory);

            Console.WriteLine("IoTHubEventProcessor is running...");
            Console.ReadLine();

        }
    }
}
