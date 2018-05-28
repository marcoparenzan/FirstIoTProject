using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubEventProcessor
{
    public class IoTHubProcessor : IEventProcessor
    {
        private CloudStorageAccount _account;

        private CloudQueueClient _queueClient;
        private CloudQueue _alarmQueue;

        private CloudBlobClient _client;
        private CloudBlobContainer _container;

        private decimal _alarmThreshold = 20;
        private DateTime? _alarmed;

        public IoTHubProcessor(string connectionString)
        {
            _account = CloudStorageAccount.Parse(connectionString);
            _client = _account.CreateCloudBlobClient();
            _container = _client.GetContainerReference("averages");
            _container.CreateIfNotExistsAsync().Wait();

            _queueClient = _account.CreateCloudQueueClient();
            _alarmQueue = _queueClient.GetQueueReference("alarms");
            _alarmQueue.CreateIfNotExistsAsync().Wait();
        }

        Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            return Task.CompletedTask;
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"Started partition {context.PartitionId}");
            return Task.CompletedTask;
        }

        Task IEventProcessor.ProcessErrorAsync(PartitionContext context, Exception error)
        {
            return Task.CompletedTask;
        }

        private Dictionary<string, AverageDevice> _devices = 
            new Dictionary<string, AverageDevice>();

        Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var message in messages)
            {
                var json = Encoding.UTF8.GetString(message.Body.Array);
                //Console.WriteLine($"{context.PartitionId}");
                //Console.WriteLine($"EnqueueTime: {message.SystemProperties.EnqueuedTimeUtc}");
                //Console.WriteLine($"LocalTime {DateTime.Now.ToUniversalTime()}");
                Console.WriteLine($"{json}");
                var ev = JsonConvert.DeserializeObject<JObject>(json);
                var temperature = ev.Value<decimal>("Value");
                var deviceId = ev.Value<string>("DeviceId");

                AverageDevice av = null;
                if (_devices.ContainsKey(deviceId))
                {
                    av = _devices[deviceId];
                }
                else
                {
                    av = new AverageDevice();
                    _devices.Add(deviceId, av);
                }

                var timeframe = (message.SystemProperties.EnqueuedTimeUtc.Second / 10) * 10;
                if (timeframe != av.TimeFrame)
                {
                    if (av.Count > 0)
                    {
                        var average = av.Sum / av.Count;
                        Console.WriteLine($"Average {timeframe}: {average} [{av.Count}]");

                        var blob = _container.GetBlockBlobReference($"{deviceId}.json");

                        var deviceState = new
                        {
                            DeviceId = deviceId,
                            Average = average,
                            Timestamp = DateTime.Now.ToUniversalTime()
                        };

                        var deviceStateJson = JsonConvert.SerializeObject(deviceState);

                        blob.UploadTextAsync(deviceStateJson).Wait();

                        if (temperature >= _alarmThreshold)
                        {
                            if (_alarmed == null)
                            {
                                // signal
                                var alarm = new
                                {
                                    Type = "averagethreshold",
                                    Threshold = _alarmThreshold,
                                    deviceState.DeviceId,
                                    deviceState.Average,
                                    deviceState.Timestamp
                                };

                                var alarmMessage = new CloudQueueMessage(
                                        JsonConvert.SerializeObject(alarm)
                                    );

                                _alarmQueue.AddMessageAsync(alarmMessage);
                                _alarmed = alarm.Timestamp;
                            }
                        }
                        else
                        {
                            _alarmed = null;
                        }
                    }
                    av.Reset(timeframe);
                }
                av.Add(temperature);

            }
            context.CheckpointAsync().Wait();
            return Task.CompletedTask;
        }
    }
}
