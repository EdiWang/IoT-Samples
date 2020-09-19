using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.Storage.Blob;
using PiCameraSurveillanceClient.Model;
using PiCameraSurveillanceClient.ViewModel;

namespace PiCameraSurveillanceClient
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel MainViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            MainViewModel = this.DataContext as MainViewModel;

            if (MainViewModel != null && (string.IsNullOrEmpty(MainViewModel.AppSettings.StorageAccountName) ||
                                          string.IsNullOrEmpty(MainViewModel.AppSettings.StorageAccountKey) ||
                                          string.IsNullOrEmpty(MainViewModel.AppSettings.StorageContainerName)))
            {
                BrdFirstUse.Visibility = Visibility.Visible;
                GrdResults.Visibility = Visibility.Collapsed;
                BtnConfig.Click += BtnConfig_Click;
            }
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ConfigPage));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!string.IsNullOrEmpty(MainViewModel.AppSettings.StorageAccountName) ||
                !string.IsNullOrEmpty(MainViewModel.AppSettings.StorageAccountKey) ||
                !string.IsNullOrEmpty(MainViewModel.AppSettings.StorageContainerName))
            {
                MainViewModel.RefreshContainer();
                await MainViewModel.GetImageList();
            }
        }

        private void BtnGoToConfig_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ConfigPage));
        }

        private void BtnSelect_OnChecked(object sender, RoutedEventArgs e)
        {
            GrdResults.SelectionMode = ListViewSelectionMode.Multiple;
        }

        private void BtnSelect_OnUnchecked(object sender, RoutedEventArgs e)
        {
            GrdResults.SelectionMode = ListViewSelectionMode.Single;
        }

        private async void BtnAbout_OnClick(object sender, RoutedEventArgs e)
        {
            await DigAbout.ShowAsync();
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.IsBusy = true;
            var frameworkElement = e.OriginalSource as FrameworkElement;
            var datacontext = frameworkElement?.DataContext as BlobImage;
            if (datacontext != null)
            {
                var pk = new FileSavePicker()
                {
                    CommitButtonText = "Select",
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                };

                pk.FileTypeChoices.Add("Image File", new List<string>() { ".png", ".jpg", ".gif", ".jpeg", ".bmp" });
                pk.SuggestedFileName = datacontext.FileName;

                var sFile = await pk.PickSaveFileAsync();

                CloudBlockBlob blockBlob = MainViewModel.Container.GetBlockBlobReference(datacontext.FileName);
                await blockBlob.DownloadToFileAsync(sFile);
            }

            MainViewModel.IsBusy = false;
        }

        private void BrdImg_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void BtnPrivacy_OnClick(object sender, RoutedEventArgs e)
        {
            await DigPp.ShowAsync();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.IsBusy = true;
            var frameworkElement = e.OriginalSource as FrameworkElement;
            var datacontext = frameworkElement?.DataContext as BlobImage;
            if (datacontext != null)
            {
                await MainViewModel.DeleteSingleItem(datacontext);
            }

            MainViewModel.IsBusy = false;
        }
    }
}
