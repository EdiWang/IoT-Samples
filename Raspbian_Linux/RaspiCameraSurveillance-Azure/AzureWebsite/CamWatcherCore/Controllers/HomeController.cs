using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CamWatcherCore.Controllers
{
    public class HomeController : Controller
    {
        public static CloudBlobContainer BlobContainer { get; set; }

        private AppSettings AppSettings { get; set; }

        public HomeController(IOptions<AppSettings> settings)
        {
            AppSettings = settings.Value;
            BlobContainer = GetBlobContainer();
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetImages()
        {
            var blobs = await BlobContainer.ListBlobsSegmentedAsync(null);

            var listBlobProperties = (from item in blobs.Results
                                      where item.GetType() == typeof(CloudBlockBlob)
                                      select (CloudBlockBlob)item
                                      into blob
                                      select new Models.PirCamBlobProperties(blob.Properties.LastModified, blob.Uri))
                                     .OrderByDescending(p => p.DateModified)
                                     .ToList();

            return Json(listBlobProperties);
        }

        private CloudBlobContainer GetBlobContainer()
        {
            string connectionString = AppSettings.StorageConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container =
                blobClient.GetContainerReference(AppSettings.AzureStorageAccountContainer);
            return container;
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
