using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTHubEventProcessor
{
    public class IoTHubProcessorFactory : IEventProcessorFactory
    {
        private string _connectionString;

        public IoTHubProcessorFactory(string connectionString) =>
            (_connectionString) = (connectionString);

        IEventProcessor IEventProcessorFactory.CreateEventProcessor(PartitionContext context)
        {
            return new IoTHubProcessor(_connectionString);
        }
    }
}
