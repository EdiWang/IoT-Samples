using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PirCamWatcher.Controllers
{
    public class HomeController : Controller
    {
        public static CloudBlobContainer BlobContainer { get; set; }

        public HomeController()
        {
            BlobContainer = GetBlobContainer();
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetImages()
        {
            var listBlobProperties = (from item in BlobContainer.ListBlobsSegmented(null).Results
                                      where item.GetType() == typeof(CloudBlockBlob)
                                      select (CloudBlockBlob)item
                                      into blob
                                      select new Models.PirCamBlobProperties(blob.Properties.LastModified, blob.Uri))
                                     .OrderByDescending(p => p.DateModified)
                                     .ToList();

            return Json(listBlobProperties, JsonRequestBehavior.AllowGet);
        }

        private static CloudBlobContainer GetBlobContainer()
        {
            string connectionString = WebConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container =
                blobClient.GetContainerReference(WebConfigurationManager.AppSettings["AzureStorageAccountContainer"]);
            return container;
        }
    }
}