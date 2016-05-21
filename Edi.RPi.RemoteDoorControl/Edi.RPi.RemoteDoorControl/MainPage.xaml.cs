using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Edi.RPi.RemoteDoorControl
{
    public sealed partial class MainPage : Page
    {
        public GpioController Controller { get; set; }

        public GpioPin PinVT { get; set; }

        public Uln2003Driver Uln2003Driver { get; set; }

        public CancellationTokenSource Cts { get; private set; }

        public bool IsBusy { get; set; }

        private DoorStatus Status { get; set; }

        enum DoorStatus
        {
            Open,
            Closed
        }

        public MainPage()
        {
            this.InitializeComponent();

            Controller = GpioController.GetDefault();
            if (null != Controller)
            {
                TxtMessage.Text += "[OK] GPIO Controller Initialized." + Environment.NewLine;

                Uln2003Driver = new Uln2003Driver(Controller, 5, 6, 13, 19);
                TxtMessage.Text += "[OK] Uln2003Driver Initialized on 5,6,13,19." + Environment.NewLine;

                Status = DoorStatus.Closed;

                PinVT = Controller.OpenPin(4);
                PinVT.SetDriveMode(GpioPinDriveMode.Input);
                TxtMessage.Text += "[OK] VT Initialized on GPIO 04." + Environment.NewLine;
                PinVT.ValueChanged += async (sender, args) =>
                {
                    if (!IsBusy && PinVT.Read() == GpioPinValue.High && Status != DoorStatus.Open)
                    {
                        IsBusy = true;
                        await LogMessageAsync("Remote Signal Received on VT.");
                        await OpenDoorAsync();
                        await Task.Delay(2000);
                        await CloseDoorAsync();
                        IsBusy = false;
                    }
                };
            }
        }

        private async Task OpenDoorAsync()
        {
            Cts = new CancellationTokenSource();

            await LogMessageAsync("Opening Door...");
            await Uln2003Driver.TurnAsync(90, TurnDirection.Left, Cts.Token);
            Status = DoorStatus.Open;
            await LogMessageAsync("Door is Open.");
        }

        private async Task CloseDoorAsync()
        {
            Cts = new CancellationTokenSource();

            await LogMessageAsync("Closing Door...");
            await Uln2003Driver.TurnAsync(90, TurnDirection.Right, Cts.Token);
            Status = DoorStatus.Closed;
            await LogMessageAsync("Door is Closed.");
        }

        private async Task LogMessageAsync(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtMessage.Text += $"\n{message}";
            });
        }
    }
}
