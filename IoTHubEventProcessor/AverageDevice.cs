using System;
using System.Collections.Generic;
using System.Text;

namespace IoTHubEventProcessor
{
    public class AverageDevice
    {
        public string DeviceId { get; set; }
        public int TimeFrame { get; set; }
        public decimal Sum { get; set; }
        public int Count { get; set; }

        internal void Add(decimal temperature)
        {
            Sum += temperature;
            Count++;
        }

        internal void Reset(int timeframe)
        {
            Sum = 0;
            Count = 0;
            TimeFrame = timeframe;
        }
    }
}
