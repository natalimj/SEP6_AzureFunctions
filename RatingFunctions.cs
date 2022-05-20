using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using SEP6_AzureFunctions.Models;
using Newtonsoft.Json;

namespace SEP6_AzureFunctions
{
    public static class RatingFunctions
    {
        static CosmosClient cosmosClient = new CosmosClient("https://movieappcosmos.documents.azure.com:443/",
            Environment.GetEnvironmentVariable("CosmosKey"));
        static Container container = cosmosClient.GetContainer("MovieAppDB", "Rating");

        [FunctionName("AddRating")]
        public static async Task<ActionResult<Rating>> AddRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddRating")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ AddRating");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            Rating rating = JsonConvert.DeserializeObject<Rating>(requestBody);

            await documentsOut.AddAsync(new
            {
                id = System.Guid.NewGuid().ToString(),
                userid = rating.UserId,
                productionid = rating.ProductionId,
                type = rating.Type,
                rating = rating.UserRating,
            });

            string responseMessage = "This HTTP triggered function executed successfully. An item has been added: Rating";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GetRatingById")]
        public static IActionResult GetRatingById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetRating/{id}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        Id ="{id}",
        PartitionKey ="{id}")] Rating item,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ GetRatingById");

            if (item == null)
            {
                return new NotFoundResult();
            }

            return new ObjectResult(item);

        }

        [FunctionName("DeleteRatingById")]
        public static async Task<IActionResult> DeleteRatingById(
        [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = "DeleteRatingById/{id}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        Id ="{id}",
        PartitionKey ="{id}")] Rating item,
        ILogger log)
        {

 
            log.LogInformation("C# HTTP trigger function processed a request./DeleteRatingById");

            if (item == null)
            {
                return new NotFoundResult();
            }

            ItemResponse<Rating> response = await container.DeleteItemAsync<Rating>(
                 partitionKey: new Microsoft.Azure.Cosmos.PartitionKey(item.Id),
                 id: item.Id);
            log.LogInformation("C# HTTP trigger function processed a request.An item has been deleted:Rating");
            return new OkObjectResult("An item has been deleted: " + item.Id);

        }
   
        [FunctionName("DeleteRating")]
        public static async Task<IActionResult> DeleteRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteRating/{productionid}/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.productionid={productionid} and  c.userid={userid}")] IEnumerable<Rating> documents,
        ILogger log)
        {

            log.LogInformation("C# HTTP trigger function processed a request./DeleteRating");

            Rating rating = documents.First();

            ItemResponse<Rating> response = await container.DeleteItemAsync<Rating>(
                 partitionKey: new Microsoft.Azure.Cosmos.PartitionKey(rating.Id),
                 id: rating.Id);
            log.LogInformation("C# HTTP trigger function processed a request.An item has been deleted:Rating ");
            return new OkObjectResult("An item has been deleted: " + rating.Id);

        }


        //all ratings by a user
        [FunctionName("GetUserRatings")]
        public static IActionResult GetUserRatings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UserRatings/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.userid={userid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / UserRatings");
            return new OkObjectResult(documents);
        }


        //all ratings for a movie - tvshow
        [FunctionName("GetProductionRatings")]
        public static IActionResult GetProductionRatings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ProductionRatings/{productionid}/{type}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.productionid={productionid} and c.type={type}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ ProductionRatings");
            return new OkObjectResult(documents);
        }


        //user's rating for a movie - tvshow
        [FunctionName("GetUserProductionRating")]
        public static IActionResult GetUserProductionRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UserRating/{productionid}/{userid}/{type}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.productionid={productionid} and c.userid={userid} and c.type = {type}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ UserRating");
            return new OkObjectResult(documents);
        }



        [FunctionName("UpdateRating")]
        public static async Task<ActionResult<Rating>> UpdateRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UpdateRating")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString")]IAsyncCollector<Rating> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ UpdateRating");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            Rating rating = JsonConvert.DeserializeObject<Rating>(requestBody);
            
            ItemResponse<Rating> response = await container.PatchItemAsync<Rating>(
                id: rating.Id,
                partitionKey: new PartitionKey(rating.Id),
                patchOperations: new[] { PatchOperation.Replace("/rating", rating.UserRating) });

            
            string responseMessage = "This HTTP triggered function executed successfully. An item has been updated : Rating";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("AddOrUpdateRating")]
        public static async Task<ActionResult<Rating>> AddOrUpdateRating(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddOrUpdateRating")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString")] IAsyncCollector<object> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ AddRating");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            Rating rating = JsonConvert.DeserializeObject<Rating>(requestBody);

            Console.WriteLine(rating.Id);

            string responseMessage = String.Empty;

            if (rating.Id == null)
            {
                await documentsOut.AddAsync(new
                {
                    id = System.Guid.NewGuid().ToString(),
                    userid = rating.UserId,
                    productionid = rating.ProductionId,
                    type = rating.Type,
                    rating = rating.UserRating
                }); 
                responseMessage = "This HTTP triggered function executed successfully. An item has been added: Rating";

            } else {

                ItemResponse<Rating> response = await container.PatchItemAsync<Rating>(
                id: rating.Id,
                partitionKey:  new Microsoft.Azure.Cosmos.PartitionKey(rating.Id),
                patchOperations: new[] { PatchOperation.Replace("/rating",rating.UserRating ) });

                responseMessage = "This HTTP triggered function executed successfully. An item has been updated: Rating";
            }

            return new OkObjectResult(responseMessage);
        }
    }
}
