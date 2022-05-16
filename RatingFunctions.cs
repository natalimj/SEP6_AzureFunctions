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
using System.Collections.Generic;
using SEP6_AzureFunctions.Models;
using System.Linq;

namespace SEP6_AzureFunctions
{
    public static class RatingFunctions
    {
        static CosmosClient cosmosClient = new CosmosClient("https://movieappcosmos.documents.azure.com:443/", 
            "Kow0snAPedc58qJU7BNSMuAyCIXVTX9QWvKVWwQExrPv35T8N6Q5thHerSo7Ow8YXvrK68oV6PXW8UtKC1Jvpw==");
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
                movieid = rating.MovieId,
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteRating/{movieid}/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.movieid={movieid} and  c.userid={userid}")] IEnumerable<Rating> documents,
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUserRatings/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.userid={userid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / GetUserRatings");
            return new OkObjectResult(documents);
        }


        //all ratings for a movie
        [FunctionName("GetMovieRatings")]
        public static IActionResult GetMovieRatings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetMovieRatings/{movieid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.movieid={movieid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ GetMovieRatings");
            return new OkObjectResult(documents);
        }


        //user's rating for a movie
        [FunctionName("getusermovieratings")]
        public static IActionResult GetUserMovieRatings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UserMovieRating/{movieid}/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "Rating",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.movieid={movieid} and  c.userid={userid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ GetUserMovieRatings");
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
                    movieid = rating.MovieId,
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