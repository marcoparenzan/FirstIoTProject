using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubMonitoring.Models
{
    public class SetAlarmPointRequest
    {
        [JsonProperty("alarmPoint")]
        public decimal AlarmPoint { get; set; }
    }
}
