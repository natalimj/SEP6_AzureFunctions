using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SEP6_AzureFunctions
{
    public static class DatabaseFunctions
    {
        [FunctionName("adduserrating")]

        public static async Task<IActionResult> AddUserRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserRating",
        ConnectionStringSetting = "DatabaseConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // string name = req.Query["name"];
            string name = "Natali";

            //  string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //  dynamic data = JsonConvert.DeserializeObject(requestBody);
            //  name = name ?? data?.name;

            if (!string.IsNullOrEmpty(name))
            {
                // Add a JSON document to the output container.
                await documentsOut.AddAsync(new
                {
                    id = System.Guid.NewGuid().ToString(),
                    timestamp = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK"),
                    username = "stephen",
                    movieid = "143936",
                    rating = 6
                }); 
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully. An item has been added to cosmos DB";

            return new OkObjectResult(responseMessage);
        }
    }
}
