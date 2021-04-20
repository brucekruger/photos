using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Blob;
using System.IO;
using Photos.Models;
using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Photos
{
    public class PhotosStorage
    {
        [FunctionName(nameof(PhotosStorage))]
        public async Task<byte[]> Run(
            [ActivityTrigger] PhotoUploadModel request,
            [Blob("photos", FileAccess.ReadWrite, Connection = Literals.StorageConnectionString)] CloudBlobContainer blobContainer,
            [CosmosDB("photos",
                      "metadata",
                      ConnectionStringSetting = Literals.CosmosDBConnection,
                      CreateIfNotExists = true)] IAsyncCollector<dynamic> items,
            ILogger logger)
        {
            var newId = Guid.NewGuid();
            var blobName = $"{newId}.jpg";

            await blobContainer.CreateIfNotExistsAsync();

            var cloudBlockBlob = blobContainer.GetBlockBlobReference(blobName);
            var photoBytes = Convert.FromBase64String(request.Photo);
            await cloudBlockBlob.UploadFromByteArrayAsync(photoBytes, 0, photoBytes.Length);

            var item = new
            {
                id = newId,
                name = request.Name,
                description = request.Description,
                tags = request.Tags
            };
            await items.AddAsync(item);

            logger?.LogInformation($"Successfully uploaded {newId}.jpg file and its metadata");

            return photoBytes;
        }
    }
}
