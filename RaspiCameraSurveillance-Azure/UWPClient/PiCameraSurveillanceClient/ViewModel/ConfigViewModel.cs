using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace PiCameraSurveillanceClient.ViewModel
{
    public class ConfigViewModel : ViewModelBase
    {
        private string _verifyMessage;
        private bool _isGoToMainEnabled;
        public AppSettings AppSettings { get; set; }

        public RelayCommand CommandVerify { get; set; }

        public CloudBlobContainer Container { get; set; }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(); }
        }

        public bool IsGoToMainEnabled
        {
            get { return _isGoToMainEnabled; }
            set { _isGoToMainEnabled = value; RaisePropertyChanged(); }
        }

        public string VerifyMessage
        {
            get { return _verifyMessage; }
            set
            {
                _verifyMessage = value;
                RaisePropertyChanged();
            }
        }

        public ConfigViewModel()
        {
            AppSettings = new AppSettings();
            CommandVerify = new RelayCommand(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(AppSettings.StorageAccountName) ||
                        !string.IsNullOrEmpty(AppSettings.StorageAccountKey) ||
                        !string.IsNullOrEmpty(AppSettings.StorageContainerName))
                    {
                        IsBusy = true;

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={AppSettings.StorageAccountName};AccountKey={AppSettings.StorageAccountKey}");
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        Container = blobClient.GetContainerReference($"{AppSettings.StorageContainerName}");

                        var permissions = await Container.GetPermissionsAsync();
                        VerifyMessage = $"Test Success. Permissions: {JsonConvert.SerializeObject(permissions)}";
                        IsGoToMainEnabled = true;

                        IsBusy = false;
                    }
                    else
                    {
                        VerifyMessage = $"Values can not be null or empty.";
                    }
                }
                catch (Exception e)
                {
                    IsGoToMainEnabled = false;
                    VerifyMessage = e.Message;
                }
            });
        }
    }
}
