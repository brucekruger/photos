using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Photos.Models;

namespace Photos
{
    public static class PhotosSearch
    {
        [FunctionName(nameof(PhotosSearch))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB("photos", "metadata", ConnectionStringSetting = Literals.CosmosDBConnection)] DocumentClient client,
            ILogger logger)
        {
            logger?.LogInformation("Searching...");

            var searchTerm = req.Query["searchTerm"];
            if(string.IsNullOrWhiteSpace(searchTerm))
            {
                return new NotFoundResult();
            }

            var collectionUri = UriFactory.CreateDocumentCollectionUri("photos", "metadata");

            var query = client.CreateDocumentQuery<PhotoUploadModel>(collectionUri,
                        new FeedOptions() { EnableCrossPartitionQuery = true })
                        .Where(p => p.Description.Contains(searchTerm))
                        .AsDocumentQuery();

            var results = new List<dynamic>();

            while (query.HasMoreResults)
            {
                foreach(var result in await query.ExecuteNextAsync())
                {
                    results.Add(result);
                }
            }

            return new OkObjectResult(results);
        }
    }
}

