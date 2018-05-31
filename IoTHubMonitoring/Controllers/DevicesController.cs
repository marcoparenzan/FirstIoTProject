using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTHubMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTHubMonitoring.Controllers
{
    public class DevicesController : Controller
    {
        private IConfiguration _configuration;

        public DevicesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var rm = RegistryManager.CreateFromConnectionString(_configuration["IoTHubConnectionString"]);

            var devicesQuery = rm.CreateQuery("SELECT DeviceId FROM devices");
            var devices = await devicesQuery.GetNextAsJsonAsync();

            var list = new List<DeviceListDto>();

            foreach (var device in devices)
            {
                var json = JsonConvert.DeserializeObject<JObject>(device);
                var dto = new DeviceListDto
                {
                    DeviceId = json.Value<string>("DeviceId")
                };
                list.Add(dto);
            }

            return View(list);
        }

        public async Task<IActionResult> Details(string id)
        {
            var dto = new DeviceDetailsDto();
            dto.DeviceId = id;
            return View(dto);
        }

        public async Task<JsonResult> CurrentValue(string id)
        {
            var dto = new DeviceCurrentValueDto();
            dto.DeviceId = id; 

            var storageAccount = CloudStorageAccount
                .Parse(_configuration["StorageConnectionString"]);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var averagesContainer = blobClient.GetContainerReference("averages");

            var blob = averagesContainer.GetBlockBlobReference($"{id}.json");

            var json = await blob.DownloadTextAsync();

            var current = JsonConvert.DeserializeObject<JObject>(json);

            dto.CurrentValue = current.Value<decimal>("Average");
            dto.Timestamp = current.Value<DateTime>("Timestamp");

            return Json(dto);
        }


        public async Task<JsonResult> LastTenAverages(string id)
        {
            var dbContext = new IoTDbContext(_configuration["SqlConnectionString"]);
            var query = 
                dbContext
                .Averages
                .Where(xx => xx.DeviceId == id)
                .OrderByDescending(xx => xx.Timestamp)
                .Take(10)
                .Select(xx => new {
                    xx.Timestamp,
                    xx.Value
                })
                ;

            var dto = query.ToList();
            return Json(dto);
        }
    }
}