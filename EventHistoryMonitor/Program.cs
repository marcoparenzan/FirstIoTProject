using EventHistoryMonitor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace EventHistoryMonitor
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var storageAccount = CloudStorageAccount.Parse(configuration["StorageConnectionString"]);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var averagesContainer = blobClient.GetContainerReference("averages");

            //var dbContext = new IoTDbContext(configuration["SqlConnectionString"]);

            while (true)
            {
                var blobs = await averagesContainer.ListBlobsSegmentedAsync(
                    null, true, BlobListingDetails.All, 100, null, null, null);
                foreach (var blob in blobs.Results)
                {
                    if (blob is CloudBlockBlob)
                    {
                        var blockBlob = (CloudBlockBlob)blob;
                        var json = await blockBlob.DownloadTextAsync();
                        var av = JsonConvert.DeserializeObject<JObject>(json);

                        var deviceId = av.Value<string>("DeviceId");
                        var timestamp = av.Value<DateTime>("Timestamp");
                        var value = av.Value<decimal>("Average");

                        //dbContext.Averages.Add(new Average
                        //{
                        //    DeviceId = deviceId,
                        //    Timestamp = timestamp,
                        //    Value = value
                        //});
                        //await dbContext.SaveChangesAsync();

                        using (var conn = new SqlConnection(configuration["SqlConnectionString"]))
                        {
                            conn.Open();
                            var cmd = conn.CreateCommand();
                            cmd.CommandText = 
                                "INSERT INTO Averages (Timestamp, DeviceId, Value) VALUES (@Timestamp, @DeviceId, @Value)";
                            cmd.Parameters.AddWithValue("Timestamp", timestamp);
                            cmd.Parameters.AddWithValue("DeviceId", deviceId);
                            cmd.Parameters.AddWithValue("Value", value);

                            cmd.ExecuteNonQuery();

                            conn.Close();
                        }

                    }
                }

                await Task.Delay(10000);
            }
        }
    }
}
