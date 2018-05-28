using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfDevice.ViewModels;
using WpfDevice.Views;

namespace WpfDevice
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var deviceId = ConfigurationManager.AppSettings["deviceId"];
            var authenticationMethod =
                new DeviceAuthenticationWithRegistrySymmetricKey(
                    deviceId,
                    ConfigurationManager.AppSettings["deviceKey"]
                )
            ;

            var transportType = TransportType.Mqtt;
            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["transportType"]))
            {
                transportType = (TransportType)
                    Enum.Parse(typeof(TransportType), 
                    ConfigurationManager.AppSettings["transportType"], true);

            }

            var client = DeviceClient.Create(
                ConfigurationManager.AppSettings["hostName"],
                authenticationMethod,
                transportType
            );


            var view = new DeviceView();
            var viewModel = new DeviceViewModel(deviceId,client);
            view.DataContext = viewModel;

            var app = new Application();
            app.Run(view);
        }
    }
}
