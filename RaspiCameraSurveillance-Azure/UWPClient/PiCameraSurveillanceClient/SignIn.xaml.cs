using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PiCameraSurveillanceClient
{
    public sealed partial class SignIn : Page
    {
        public SignIn()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!await MicrosoftPassportHelper.MicrosoftPassportAvailableCheckAsync())
            {
                BrdNotSetupMessage.Visibility = Visibility.Visible;
                BtnVerify.IsEnabled = false;
            }
        }

        private async void BtnVerify_OnClick(object sender, RoutedEventArgs e)
        {
            Prg.IsActive = true;
            BrdFailed.Visibility = Visibility.Collapsed;
            BtnVerify.IsEnabled = false;
            BtnVerify.Content = "Please Wait...";
            var result = await MicrosoftPassportHelper.CreatePassportKeyAsync("default");
            if (result)
            {
                Frame.Navigate(typeof(MainPage));
            }
            else
            {
                BtnVerify.Content = "Try Again";
                BrdFailed.Visibility = Visibility.Visible;
                BtnVerify.IsEnabled = true;
            }
            Prg.IsActive = false;
        }
    }
}
