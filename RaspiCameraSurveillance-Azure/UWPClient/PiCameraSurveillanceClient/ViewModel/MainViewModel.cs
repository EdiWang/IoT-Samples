using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Edi.UWP.Helpers;
using Edi.UWP.Helpers.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PiCameraSurveillanceClient.Model;

namespace PiCameraSurveillanceClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private RelayCommand _commandRefresh;
        private string _containerDisplayName;

        private bool _isBusy;

        private bool _isContainerInited;
        private bool _isRefereshEnabled;

        private ObservableCollection<BlobImage> _listBlobItems;
        private ObservableCollection<BlobImage> _selectedImages;

        public MainViewModel()
        {
            AppSettings = new AppSettings();
            if (IsInDesignMode)
            {
                Container = new CloudBlobContainer(new Uri("http://some.net"));
                ContainerDisplayName = "Test Container";
            }
            else
            {
                if (!string.IsNullOrEmpty(AppSettings.StorageAccountName) ||
                    !string.IsNullOrEmpty(AppSettings.StorageAccountKey) ||
                    !string.IsNullOrEmpty(AppSettings.StorageContainerName))
                    RefreshContainer();
                else
                    ContainerDisplayName = "#Unconfigured";
            }

            CommandRefresh = new RelayCommand(async () => await GetImageList());
            CommandDelete = new RelayCommand(async () => await DeleteSelectedItems());
            SelectedImages = new ObservableCollection<BlobImage>();
        }

        public AppSettings AppSettings { get; set; }

        public CloudBlobContainer Container { get; set; }

        public RelayCommand CommandRefresh
        {
            get { return _commandRefresh; }
            set
            {
                _commandRefresh = value;
                RaisePropertyChanged();
            }
        }

        public string ContainerDisplayName
        {
            get { return _containerDisplayName; }
            set
            {
                _containerDisplayName = value;
                RaisePropertyChanged();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                IsRefereshEnabled = !value;
                RaisePropertyChanged();
            }
        }

        public bool IsRefereshEnabled
        {
            get { return _isRefereshEnabled; }
            set
            {
                _isRefereshEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDeleteButtonEnabled
        {
            get { return _isDeleteButtonEnabled; }
            set
            {
                _isDeleteButtonEnabled = value;
                RaisePropertyChanged();
            }
        }

        public BlobImage SelectedImage
        {
            get { return _selectedImage; }
            set { _selectedImage = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<BlobImage> ListBlobItems
        {
            get { return _listBlobItems; }
            set
            {
                _listBlobItems = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<BlobImage> SelectedImages
        {
            get { return _selectedImages; }
            set
            {
                _selectedImages = value;
                RaisePropertyChanged();
            }
        }


        private RelayCommand<IList<object>> _selectionChangedCommand;
        private bool _isDeleteButtonEnabled;
        private BlobImage _selectedImage;

        public RelayCommand<IList<object>> SelectionChangedCommand
        {
            get
            {
                return _selectionChangedCommand ?? (_selectionChangedCommand = new RelayCommand<IList<object>>(
                           items =>
                           {
                               SelectedImages.Clear();
                               foreach (object item in items)
                               {
                                   var img = item as BlobImage;
                                   if (null != img)
                                   {
                                       SelectedImages.Add(img);
                                   }
                               }
                               IsDeleteButtonEnabled = SelectedImages.Any();
                           }
                       ));
            }
        }

        public RelayCommand CommandDelete { get; set; }

        public async Task DeleteSingleItem(BlobImage img)
        {
            CloudBlockBlob blockBlob = Container.GetBlockBlobReference(img.FileName);
            await blockBlob.DeleteAsync();
            this.ListBlobItems.Remove(img);
            RaisePropertyChanged(nameof(ListBlobItems));
        }

        public async Task DeleteSelectedItems()
        {
            IsBusy = true;
            foreach (var item in SelectedImages)
            {
                CloudBlockBlob blockBlob = Container.GetBlockBlobReference(item.FileName);
                await blockBlob.DeleteAsync();
            }
            IsBusy = false;
            await GetImageList();
        }

        public async Task<KeyValuePair<bool, string>> UploadImage(IReadOnlyList<StorageFile> sFiles)
        {
            IsBusy = true;
            try
            {
                foreach (var storageFile in sFiles)
                {
                    CloudBlockBlob blockBlob = Container.GetBlockBlobReference(storageFile.Name);
                    await blockBlob.UploadFromFileAsync(storageFile);
                }

                IsBusy = false;
                return new KeyValuePair<bool, string>(true, string.Empty);
            }
            catch (Exception e)
            {
                IsBusy = false;
                return new KeyValuePair<bool, string>(false, e.Message);
            }
        }

        public void RefreshContainer()
        {
            var storageAccount =
                CloudStorageAccount.Parse(
                    $"DefaultEndpointsProtocol=https;AccountName={AppSettings.StorageAccountName};AccountKey={AppSettings.StorageAccountKey}");
            var blobClient = storageAccount.CreateCloudBlobClient();
            Container = blobClient.GetContainerReference($"{AppSettings.StorageContainerName}");

            ContainerDisplayName = Container.Name;
            _isContainerInited = true;
        }

        public async Task GetImageList()
        {
            if (!_isContainerInited)
                RefreshContainer();

            IsBusy = true;

            ListBlobItems = new ObservableCollection<BlobImage>();

            var blobs = await Container.ListBlobsSegmentedAsync(null);

            var listBlobProperties = (from item in blobs.Results
                                      where item.GetType() == typeof(CloudBlockBlob)
                                      select (CloudBlockBlob)item
                                      into blob
                                      select new BlobImage(blob.Properties.LastModified, blob.Uri)
                                      {
                                          FileName = blob.Name
                                      })
                .OrderByDescending(p => p.LastModified)
                .ToList();

            ListBlobItems = listBlobProperties.ToObservableCollection();

            IsBusy = false;
        }


    }
}