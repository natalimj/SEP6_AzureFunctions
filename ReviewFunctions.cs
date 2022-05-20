using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using SEP6_AzureFunctions.Models;


namespace SEP6_AzureFunctions
{
    public static class ReviewFunctions
    {
        static CosmosClient cosmosClient = new CosmosClient("https://movieappcosmos.documents.azure.com:443/", 
            Environment.GetEnvironmentVariable("CosmosKey"));
        static Container container = cosmosClient.GetContainer("MovieAppDB", "Review");


        [FunctionName("AddReview")]
        public static async Task<ActionResult<Review>> AddReview(
         [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddReview")] HttpRequest req,
         [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Review",
        ConnectionStringSetting = "DatabaseConnectionString")]IAsyncCollector<dynamic> documentsOut,
         ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ AddReview");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            Review review = JsonConvert.DeserializeObject<Review>(requestBody);

            await documentsOut.AddAsync(new
            {
                id = System.Guid.NewGuid().ToString(),
                userid = review.UserId,
                productionid = review.ProductionId,
                type= review.Type,
                review = review.UserReview,
            });

            string responseMessage = "This HTTP triggered function executed successfully. An item has been added: Review";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GetUserReviews")]
        public static IActionResult GetUserReviews(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UserReviews/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Review",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.userid={userid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / GetUserReviews");
            return new OkObjectResult(documents);
        }

        [FunctionName("GetProductionReview")]
        public static IActionResult GetProductionReview(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ProductionReview/{productionid}/{type}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Review",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.productionid={productionid} and c.type={type}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ GetProductionReviews");
            return new OkObjectResult(documents);
        }
    
        [FunctionName("UpdateReview")]
        public static async Task<ActionResult<Review>> UpdateReview(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UpdateReview")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Review",
        ConnectionStringSetting = "DatabaseConnectionString")]IAsyncCollector<dynamic> documentsOut,
       ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ UpdateReview");
            string requestBody = String.Empty;

            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            Review review= JsonConvert.DeserializeObject<Review>(requestBody);
            
            ItemResponse<Review> response = await container.PatchItemAsync<Review>(
                id: review.Id,
                partitionKey: new PartitionKey(review.Id),
                patchOperations: new[] {PatchOperation.Replace("/review", review.UserReview)});
           
            string responseMessage = "This HTTP triggered function executed successfully. An item has been updated :Review";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("DeleteReviewById")]
        public static async Task<IActionResult> DeleteReviewById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteReviewById/{id}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Review",
        ConnectionStringSetting = "DatabaseConnectionString",
        Id ="{id}",
        PartitionKey ="{id}")] Review item,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./DeleteRatingById");

            if (item == null)
            {
                return new NotFoundResult();
            }

            ItemResponse<Review> response = await container.DeleteItemAsync<Review>(
                 partitionKey: new Microsoft.Azure.Cosmos.PartitionKey(item.Id),
                 id: item.Id);
            log.LogInformation("C# HTTP trigger function processed a request.An item has been deleted: Review");
            return new OkObjectResult("An item has been deleted: " + item.Id);

        }

        [FunctionName("GetUserProductionReview")]
        public static IActionResult GetUserProductionReview(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UserReview/{productionid}/{userid}/{type}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Review",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.productionid={productionid} and c.userid={userid} and c.type = {type}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ UserReview");
            return new OkObjectResult(documents);
        }



    }
}
