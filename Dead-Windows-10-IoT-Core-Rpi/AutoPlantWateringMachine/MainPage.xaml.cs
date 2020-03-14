using System;
using System.Collections.Generic;
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
using AutoPlantWateringMachine.ViewModel;

namespace AutoPlantWateringMachine
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel MainViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            MainViewModel = DataContext as MainViewModel;

            this.Loaded += async (sender, args) =>
            {
                await MainViewModel.Start();
            };
        }
    }
}
