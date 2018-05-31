using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IoTHubMonitoring.Models
{
    public class Average
    {
        [Key]
        public long Id { get; set; }
        public string DeviceId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public decimal Value { get; set; }
    }   
}
