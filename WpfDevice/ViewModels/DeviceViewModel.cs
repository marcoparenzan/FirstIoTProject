using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfDevice.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private decimal _setPoint = 20;

        private decimal _alarmPoint = 20.20M;

        private DeviceClient _client;

        public bool Alarm => _temperature >= _alarmPoint;

        public DeviceViewModel(string deviceId, DeviceClient client)
        {
            Task.Run(async () =>
            {
                var twin = await client.GetTwinAsync();
                if (twin.Properties.Desired.Contains("SetPoint"))
                {
                    _setPoint = (decimal)(float)twin.Properties.Desired["SetPoint"];
                }
                if (twin.Properties.Desired.Contains("AlarmPoint"))
                {
                    _alarmPoint = (decimal)(float)twin.Properties.Desired["AlarmPoint"];
                }

                Temperature = _setPoint;

                await client.SetDesiredPropertyUpdateCallbackAsync(async (tc, oc) =>
                {
                    if (tc.Contains("SetPoint"))
                    {
                        _setPoint = (decimal)(float)tc["SetPoint"];
                        Temperature = _setPoint;
                    }
                    if (tc.Contains("AlarmPoint"))
                    {
                        _alarmPoint = (decimal)(float)tc["AlarmPoint"];
                    }
                }, null);

                var runningAlarm = false;

                var random = new Random();
                while (true)
                {
                    Temperature += Math.Round(((decimal)random.NextDouble()) - 0.5M, 2);

                    var sample = new
                    {
                        Timestamp = DateTime.Now.ToUniversalTime(),
                        DeviceId = deviceId,
                        SampleType = "temperature",
                        Value = Temperature
                    };
                    var json = JsonConvert.SerializeObject(sample);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    var message = new Message(bytes);
                    message.Properties["sampleType"] = "temperature";
                    client.SendEventAsync(message).Wait();

                    if (!runningAlarm && Alarm)
                    {
                        // notify Alarm
                        var tc = new TwinCollection();
                        tc["Alarm"] = true;
                        await client.UpdateReportedPropertiesAsync(tc);
                        runningAlarm = true;
                    }
                    else if (runningAlarm && !Alarm)
                    {
                        // remove Alarm
                        var tc = new TwinCollection();
                        tc["Alarm"] = false;
                        await client.UpdateReportedPropertiesAsync(tc);
                        runningAlarm = false;
                    }

                    Task.Delay(1000).Wait();

                }
            });

            _client = client;
        }

        private decimal _temperature;

        public decimal Temperature
        {
            get => _temperature;
            set {
                _temperature = value;
                Notify();
                Notify("Alarm");
            }
        }

        protected void Notify([CallerMemberName]string propertyName = null)
        {
            if (_propertyChanged == null) return;
            _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;
            }
        }
    }
}
