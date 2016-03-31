using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using StorageSharedAccessSignatureLab.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StorageSharedAccessSignatureLab.Controllers
{
    //https://azure.microsoft.com/zh-tw/documentation/articles/storage-dotnet-shared-access-signature-part-1/
    //http://gauravmantri.com/2013/12/01/windows-azure-storage-and-cors-lets-have-some-fun/
    //https://github.com/orcame/jquery-blobuploader

    public class HomeController : Controller
    {
        string containerUrl = "https://xxx.blob.core.windows.net/kimtest";
        // GET: Home
        public ActionResult Index()
        {
            IndexModel model = new IndexModel();
            model.ContainerUrl = containerUrl;
            model.SasToken = GetSASToken();
            return View(model);
        }

        public ActionResult SetCorsRules()
        {
            AddCorsRuleStorageClientLibrary();
            return RedirectToAction("Index");

        }

        static void AddCorsRuleStorageClientLibrary()
        {
            //Add a new rule.
            var corsRule = new CorsRule()
            {
                AllowedHeaders = new List<string> { "x-ms-*", "content-type", "accept" },
                AllowedMethods = CorsHttpMethods.Post| CorsHttpMethods.Put,
                AllowedOrigins = new List<string> { "*" },
               // MaxAgeInSeconds = 1 * 60 * 60,//Let the browswer cache it for an hour
            };

            //First get the service properties from storage to ensure we're not adding the same CORS rule again.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var serviceProperties = client.GetServiceProperties();
            var corsSettings = serviceProperties.Cors;
            corsSettings.CorsRules.Clear();
            corsSettings.CorsRules.Add(corsRule);
            client.SetServiceProperties(serviceProperties);
        }



        static string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxx";

        static CloudBlobContainer GetContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("kimtest");
            container.CreateIfNotExists();

            return container;
        }

        /// <summary>
        ///產生Share Acceess Token
        /// </summary>
        /// <returns></returns>
        static string GetSASToken()
        {
            //1.取得Container
            var container = GetContainer();
            //2.設定共用存取原則
            BlobContainerPermissions blobPermissions = container.GetPermissions();
            blobPermissions.SharedAccessPolicies.Clear();
            blobPermissions.SharedAccessPolicies.Add("mypolicy", new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read
                | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Add

            });
            blobPermissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            container.SetPermissions(blobPermissions);

            //3.取得Token
            string sasToken = container.GetSharedAccessSignature(new SharedAccessBlobPolicy(), "mypolicy");
            return sasToken;
        }

    }
}