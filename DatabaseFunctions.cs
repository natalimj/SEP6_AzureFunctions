using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SEP6_AzureFunctions.Models;
using Microsoft.Azure.Cosmos;

namespace SEP6_AzureFunctions
{
    public static class DatabaseFunctions
    {
        // static CosmosClient cosmosClient = new CosmosClient("DatabaseConnectionString", new CosmosClientOptions() { AllowBulkExecution = true });
        //static Microsoft.Azure.Cosmos.Database database = cosmosClient.GetDatabase("MovieAppDB");
        //static Container container = database.GetContainer("UserRating");

        static CosmosClient cosmosClient = new CosmosClient("DatabaseConnectionString");

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
                    username = "leila",
                    movieid = "143936",
                    rating = 6
                });
            }

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully. An item has been added to cosmos DB";

            return new OkObjectResult(responseMessage);
        }




        //http://localhost:7071/api/UserRating/timestamp/1
        [FunctionName("getuserrating")]
        public static IActionResult GetUserRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{CollectionName}/{timestamp}/{id}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserRating",
        ConnectionStringSetting = "DatabaseConnectionString",
        Id ="{id}",
        PartitionKey ="{timestamp}")] UserRating item,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (item == null)
            {
                return new NotFoundResult();
            }

            return new ObjectResult(item);

        }


        [FunctionName("getallratings")]
        public static IActionResult GetAllRatings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserRating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult(documents);
        }

    


        [FunctionName("deleterating")]
        public async static Task<IActionResult> DeleteRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{timestamp}/{id}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserRating",
        ConnectionStringSetting = "DatabaseConnectionString",
        Id ="{id}",
        PartitionKey ="{timestamp}")] UserRating item,
        ILogger log)

        {     
            Container container = cosmosClient.GetContainer("MovieAppDB","UserRating");
 
            await container.DeleteItemAsync<UserRating>("1",new PartitionKey("timestamp"));
            log.LogInformation("C# HTTP trigger function processed a request.An item has been deleted: ");
            return new OkObjectResult("An item has been deleted: ");


        }
      
   
        }
    }
