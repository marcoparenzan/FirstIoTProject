using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTHubMonitoring.Models
{
    public class DeviceDetailsDto
    {
        public string DeviceId { get; set; }
        public decimal? SetPoint { get; set; }
        public decimal? AlarmPoint { get; set; }
    }
}
