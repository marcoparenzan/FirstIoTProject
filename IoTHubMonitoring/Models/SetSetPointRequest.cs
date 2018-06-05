using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubMonitoring.Models
{
    public class SetSetPointRequest
    {
        [JsonProperty("setPoint")]
        public decimal SetPoint { get; set; }
    }
}
