using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SEP6_AzureFunctions.Models;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using System.Linq;

namespace SEP6_AzureFunctions
{
    public static class ListFunctions
    {
        static CosmosClient cosmosClient = new CosmosClient("https://movieappcosmos.documents.azure.com:443/", 
            "Kow0snAPedc58qJU7BNSMuAyCIXVTX9QWvKVWwQExrPv35T8N6Q5thHerSo7Ow8YXvrK68oV6PXW8UtKC1Jvpw==");
        static Container container = cosmosClient.GetContainer("MovieAppDB", "UserList");
       
        //create an empty list
        [FunctionName("CreateList")]
        public static async Task<ActionResult<UserList>> CreateList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateList")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ CreateList");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            UserList userList= JsonConvert.DeserializeObject<UserList>(requestBody);

            await documentsOut.AddAsync(new
            {
                id = System.Guid.NewGuid().ToString(),
                userid = userList.UserId,
                listname = userList.ListName,
                movies = new List<string>()
            }) ;
            
            string responseMessage = "This HTTP triggered function executed successfully. An item has been added: UserList";

            return new OkObjectResult(responseMessage);
        }
     
        //add movie to the list
        [FunctionName("UpdateList")]
        public static async Task<ActionResult<UserList>> UpdateList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UpdateList")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ UpdateUserlist");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
           
            UserList userList = JsonConvert.DeserializeObject<UserList>(requestBody);
        
            ItemResponse<UserList> response = await container.PatchItemAsync<UserList>(
                id: userList.Id,
                partitionKey: new PartitionKey(userList.Id),
                patchOperations: new[] { PatchOperation.Replace("/movies", userList.Movies) });
    
            string responseMessage = "This HTTP triggered function executed successfully. An item has been updated : UserList";

            return new OkObjectResult(responseMessage);
        }

        //get User lists - only list names
        [FunctionName("GetUserLists")]
        public static IActionResult GetUserLists(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUserLists/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT c.listname FROM c where c.userid={userid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / GetUserLists");
            return new OkObjectResult(documents);
        }

        // get user lists
        [FunctionName("GetUserListNames")]
        public static IActionResult GetUserListNames(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUserListNames/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.userid={userid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / GetUserListNames");
            return new OkObjectResult(documents);
        }

        [FunctionName("GetMoviesInList")]
        public static IActionResult GetMoviesInList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetMoviesInList/{userid}/{listname}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT c.movies FROM c where c.userid={userid} and c.listname={listname}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / GetMoviesInList");
            return new OkObjectResult(documents);
        }


        [FunctionName("DeleteList")]
        public static async Task<IActionResult> DeleteList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteList/{userid}/{listname}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT c.movies FROM c where c.userid={userid} and c.listname={listname}")] IEnumerable<UserList> documents,
        ILogger log)
        {

            log.LogInformation("C# HTTP trigger function processed a request./DeleteList");

            UserList userList = documents.First();

            ItemResponse<UserList> response = await container.DeleteItemAsync<UserList>(
                 partitionKey: new Microsoft.Azure.Cosmos.PartitionKey(userList.Id),
                 id: userList.Id);
            log.LogInformation("C# HTTP trigger function processed a request.An item has been deleted:List");
            return new OkObjectResult("An item has been deleted: " + userList.Id);

        }


        [FunctionName("AddOrUpdateList")]
        public static async Task<ActionResult<UserList>> AddOrUpdateList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddOrUpdateList")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString")] IAsyncCollector<object> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./ AddOrUpdateList");
            string requestBody = String.Empty;
            using (StreamReader streamReader = new StreamReader(req.Body))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            UserList userList = JsonConvert.DeserializeObject<UserList>(requestBody);


            string responseMessage = String.Empty;

            if (userList.Id == null)
            {
                await documentsOut.AddAsync(new
                {
                    id = System.Guid.NewGuid().ToString(),
                    userid = userList.UserId,
                    listname = userList.ListName,
                    movies = new List<string>()
                });
                responseMessage = "This HTTP triggered function executed successfully. An item has been added: List";

            }
            else
            {

               ItemResponse<UserList> response = await container.PatchItemAsync<UserList>(
               id: userList.Id,
               partitionKey: new PartitionKey(userList.Id),
               patchOperations: new[] { PatchOperation.Replace("/movies", userList.Movies) });

               responseMessage = "This HTTP triggered function executed successfully. An item has been updated: List";
            }

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("DeleteListById")]
        public static async Task<IActionResult> DeleteListById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteListById/{id}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        Id ="{id}",
        PartitionKey ="{id}")] UserList item,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request./DeleteListById");

            if (item == null)
            {
                return new NotFoundResult();
            }

            ItemResponse<UserList> response = await container.DeleteItemAsync<UserList>(
                 partitionKey: new Microsoft.Azure.Cosmos.PartitionKey(item.Id),
                 id: item.Id);
            log.LogInformation("C# HTTP trigger function processed a request.An item has been deleted: UserList");
            return new OkObjectResult("An item has been deleted: " + item.Id);

        }


    }
}
