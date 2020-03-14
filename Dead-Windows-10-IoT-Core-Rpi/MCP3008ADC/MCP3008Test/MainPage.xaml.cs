using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using MCP3008Test.ViewModel;

namespace MCP3008Test
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
