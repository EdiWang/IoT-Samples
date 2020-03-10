using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GY30LightSensor
{
    public sealed partial class MainPage : Page
    {
        public GY30LightSensor Gy30LightSensor { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            Gy30LightSensor = new GY30LightSensor();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            await Gy30LightSensor.InitLightSensorAsync();
            Gy30LightSensor.Reading += (o, args) =>
            {
                var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    TxtLux.Text = args.Lux + " lx";
                });
            };
        }
    }
}
