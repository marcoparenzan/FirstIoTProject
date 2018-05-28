using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTHubMonitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
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
    }
}