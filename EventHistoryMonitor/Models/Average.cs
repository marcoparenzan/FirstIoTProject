using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EventHistoryMonitor.Models
{
    public class Average
    {
        [Key]
        public long Id { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
    }   
}
