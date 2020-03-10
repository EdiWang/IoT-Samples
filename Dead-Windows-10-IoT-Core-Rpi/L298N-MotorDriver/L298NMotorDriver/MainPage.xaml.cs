using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace L298NMotorDriver
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Initialize Motor Driver Pins
            L298NDriver.InitializePins(
                L298NDriver.AvailableGpioPin.GpioPin_5, 
                L298NDriver.AvailableGpioPin.GpioPin_6, 
                L298NDriver.AvailableGpioPin.GpioPin_13, 
                L298NDriver.AvailableGpioPin.GpioPin_26);
        }

        private void BtnFw_OnClick(object sender, RoutedEventArgs e)
        {
            L298NDriver.MoveForward();
        }

        private void BtnRv_OnClick(object sender, RoutedEventArgs e)
        {
            L298NDriver.MoveReverse();
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            L298NDriver.Stop();
        }
    }
}
