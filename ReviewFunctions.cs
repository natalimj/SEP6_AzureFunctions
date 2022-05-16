using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using SEP6_AzureFunctions.Models;
using System.Collections.Generic;

namespace SEP6_AzureFunctions
{
    public static class ReviewFunctions
    {

        //static CosmosClient cosmosClient = new CosmosClient("accountEndPoint", "authKeyCosmos");
        static CosmosClient cosmosClient = new CosmosClient("https://movieappcosmos.documents.azure.com:443/", 
            "Kow0snAPedc58qJU7BNSMuAyCIXVTX9QWvKVWwQExrPv35T8N6Q5thHerSo7Ow8YXvrK68oV6PXW8UtKC1Jvpw==");
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
                movieid = review.MovieId,
                review = review.UserReview,
            });

            string responseMessage = "This HTTP triggered function executed successfully. An item has been added: Review";

            return new OkObjectResult(responseMessage);
        }

        //all reviews by a user
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


        //all reviews for a movie
        [FunctionName("GetMovieReview")]
        public static IActionResult GetMovieReview(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "MovieReview/{movieid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Review",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.movieid={movieid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ GetMovieReviews");
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

        [FunctionName("AddOrUpdateReview")]
        public static async Task<ActionResult<Review>> AddOrUpdateReview(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddOrUpdateReview")] HttpRequest req,
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

            string responseMessage = String.Empty;

            if (review.Id == null)
            {
                await documentsOut.AddAsync(new
                {
                    id = System.Guid.NewGuid().ToString(),
                    userid = review.UserId,
                    movieid = review.MovieId,
                    review = review.UserReview,
                });
                responseMessage = "This HTTP triggered function executed successfully. An item has been added: Review";

            } else {


              ItemResponse<Review> response = await container.PatchItemAsync<Review>(
              id: review.Id,
              partitionKey: new PartitionKey(review.Id),
              patchOperations: new[] { PatchOperation.Replace("/review", review.UserReview) });

              responseMessage = "This HTTP triggered function executed successfully. An item has been updated: Review";

            }
          
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("DeleteReviewById")]
        public static async Task<IActionResult> DDeleteReviewById(
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
    }
}