using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;

// Azure Device Explorer: https://aka.ms/aziotdevexp

// ID=ERPi4
// PrimaryKey=5uJDSegiI5RXJ5IV3bmz0K6aY6viX4tagr/WOkAb6u0=
// SecondaryKey=ZKdu7gdMHZtTgoh7SrmqDPK9bCxhurX1w2r6IVFqjKM=

// HostName=pornhub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=gsLhsaM1mzbT/uANTKKAUQ8W6kMUoe76j1W6DmP5aZU=
namespace AzureLightControl
{
    class Program
    {
        private const string IotHubUri = "pornhub.azure-devices.net";
        private const string DeviceKey = "5uJDSegiI5RXJ5IV3bmz0K6aY6viX4tagr/WOkAb6u0=";
        private const string DeviceId = "ERPi4";
        private const int Pin = 17;

        private static CancellationToken _ct;

        static async Task Main(string[] args)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine(" Azure IoT Hub Light Control");
            Console.WriteLine("------------------------------");

            var cts = new CancellationTokenSource();
            _ct = cts.Token;

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine($"{DateTime.Now} > Cancelling...");
                cts.Cancel();

                eventArgs.Cancel = true;
            };
            
            try
            {
                var t = Task.Run(Run, cts.Token);
                await t;
            }
            catch (IotHubCommunicationException)
            {
                Console.WriteLine($"{DateTime.Now} > Operation has been canceled.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"{DateTime.Now} > Operation has been canceled.");
            }
            finally
            {
                cts.Dispose();
            }

            Console.ReadKey();
        }

        private static async Task Run()
        {
            using var deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey));
            using var controller = new GpioController();
            controller.OpenPin(Pin, PinMode.Output);

            Console.WriteLine($"{DateTime.Now} > Connected to the best cloud on the planet.");
            Console.WriteLine($"Azure IoT Hub: {IotHubUri}");
            Console.WriteLine($"Device ID: {DeviceId}");

            Console.WriteLine($"{DateTime.Now} > GPIO pin enabled for use: {Pin}");

            while (!_ct.IsCancellationRequested)
            {
                Console.WriteLine($"{DateTime.Now} > Waiting new message from Azure...");
                var receivedMessage = await deviceClient.ReceiveAsync(_ct);
                if (receivedMessage == null) continue;

                var msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                Console.WriteLine($"{DateTime.Now} > Received message: {msg}");

                switch (msg)
                {
                    case "on":
                        Console.WriteLine($"{DateTime.Now} > Turn on the light.");
                        controller.Write(Pin, PinValue.High);
                        break;
                    case "off":
                        Console.WriteLine($"{DateTime.Now} > Turn off the light.");
                        controller.Write(Pin, PinValue.Low);
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {msg}");
                        break;
                }

                await deviceClient.CompleteAsync(receivedMessage, _ct);
            }
        }
    }
}
