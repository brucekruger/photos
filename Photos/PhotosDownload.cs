using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Photos
{
    public static class PhotosDownload
    {
        [FunctionName(nameof(PhotosDownload))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "photos/{id}")] HttpRequest req,
            [Blob("photos-small/{id}.jpg", FileAccess.Read, Connection = Literals.StorageConnectionString)] Stream imageSmall,
            [Blob("photos-medium/{id}.jpg", FileAccess.Read, Connection = Literals.StorageConnectionString)] Stream imageMedium,
            [Blob("photos/{id}.jpg", FileAccess.Read, Connection = Literals.StorageConnectionString)] Stream imageOriginal,
            Guid id,
            ILogger logger)
        {
            logger?.LogInformation($"Downloading {id}...");

            byte[] data;

            var size = req.Query["size"];

            switch (size)
            {
                case "sm":
                    logger?.LogInformation("Retrieving the small size");
                    data = await GetBytesFromStreamAsync(imageSmall);
                    break;

                case "md":
                    logger?.LogInformation("Retrieving the medium size");
                    data = await GetBytesFromStreamAsync(imageMedium);
                    break;

                default:
                    logger?.LogInformation("Retrieving the original size");
                    data = await GetBytesFromStreamAsync(imageOriginal);
                    break;
            }

            return new FileContentResult(data, "image/jpeg")
            {
                FileDownloadName = $"{id}.jpg"
            };
        }

        private static async Task<byte[]> GetBytesFromStreamAsync(Stream stream)
        {
            byte[] data = new byte[stream.Length];
            await stream.ReadAsync(data, 0, data.Length);
            return data;
        }
    }
}

