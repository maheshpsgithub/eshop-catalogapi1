using CatalogAPI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogAPI.Helpers
{
    public class StorageAccountAzureHelper
    {
        public string storageConnectionString;
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;


        private string tableConnectionString;
        private CloudStorageAccount tableStorageAccount;


        public string StorageConnectionString { 
            get { return storageConnectionString; } 
            set {
                this.storageConnectionString = value;
                storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);
            } 
        }

        public string TableConnectionString {
            get { return tableConnectionString; }
            set { this.tableConnectionString = value;
                tableStorageAccount= CloudStorageAccount.Parse(this.tableConnectionString); 
            }
        
        }
        public StorageAccountAzureHelper()
        {

        }

        public async Task<string> UploadFileToBlobAsync(string filePath, string containerName) {
            blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            BlobContainerPermissions permissions = new BlobContainerPermissions() {
                PublicAccess = BlobContainerPublicAccessType.Container
            };
            await container.SetPermissionsAsync(permissions);
            await container.CreateIfNotExistsAsync();

            var fileName = Path.GetFileName(filePath);//FileInfo
            var blob = container.GetBlockBlobReference(fileName);

            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(filePath);

            return blob.Uri.AbsoluteUri;


        }
        public async Task<CatalogEntity> SaveToTableAsync(CatalogItem item) {
            CatalogEntity catalogEntity = new CatalogEntity(item.Name, item.Id)
            {
                ImageUrl = item.ImageUrl,
                ReorderLevel = item.ReorderLevel,
                Price = item.Price,
                Quantity = item.Quantity,
                ManufacturingDate = item.ManufacturingDate

            };
            tableClient = tableStorageAccount.CreateCloudTableClient();//storageAccount.CreateCloudTableClient();
            var catalogTable = tableClient.GetTableReference("catalog");
            await catalogTable.CreateIfNotExistsAsync();

            TableOperation operation = TableOperation.InsertOrMerge(catalogEntity);
            var tableResult = await catalogTable.ExecuteAsync(operation);
            return tableResult.Result as CatalogEntity;
        }
    }
}
