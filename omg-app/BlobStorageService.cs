namespace omg_app
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Models;

    public class BlobStorageService
    {
        private static string BlobContainerName = "data";

        private readonly CloudBlobClient client;
        public readonly SharedAccessBlobPolicy policy;

        public BlobStorageService(BlobStorageInfo info)
        {
            var account = new CloudStorageAccount(new StorageCredentials(info.AccountName, info.Key), true);
            this.client = account.CreateCloudBlobClient();
            this.policy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(5)
            };
        }

        public async Task<List<BlobMetaInfo>> GetALLAsync()
        {
            var sourceContainerInstance = this.client.GetContainerReference(BlobContainerName.ToLower());
            await sourceContainerInstance.CreateIfNotExistsAsync();
        
            var resultSegment = await sourceContainerInstance.ListBlobsSegmentedAsync(null);

            var blobNames = new List<BlobMetaInfo>();

            foreach (var item in resultSegment.Results)
            {
                var blob = (CloudBlockBlob)item;
                if (await blob.ExistsAsync())
                {
                    await blob.FetchAttributesAsync();
                    var sas = blob.GetSharedAccessSignature(this.policy);
                    blobNames.Add(new BlobMetaInfo() { Name = blob.Name, Size = blob.Properties.Length, DownloadUrl = blob.Uri + sas });
                } 
            }

            return blobNames;
        }

        public async Task<string> GetAsync(string name)
        {
            var containerInstance = this.client.GetContainerReference(BlobContainerName);
            if (!await containerInstance.ExistsAsync())
            {
                throw new ArgumentNullException();
            }

            var blob = await containerInstance.GetBlobReferenceFromServerAsync(name);
            var sas = blob.GetSharedAccessSignature(this.policy);

            return blob.Uri + sas;
        }

        public async Task DeleteAsync(string name)
        {
            var containerInstance = this.client.GetContainerReference(BlobContainerName.ToLower());
            if (!await containerInstance.ExistsAsync())
            {
                throw new ArgumentNullException();
            }

            var blob = containerInstance.GetBlobReference(name);

            await blob.DeleteIfExistsAsync();
        }

        public async Task UploadAsync(string name, byte[] data)
        {
            var containerInstance = this.client.GetContainerReference(BlobContainerName);
            if (await containerInstance.CreateIfNotExistsAsync())
            {
                await containerInstance.SetPermissionsAsync(
                    new BlobContainerPermissions()
                    {
                        PublicAccess = BlobContainerPublicAccessType.Off
                    });

            }
            var blob = containerInstance.GetBlockBlobReference(name);
            await blob.UploadFromByteArrayAsync(data, 0, data.Length);
        }
    }
}