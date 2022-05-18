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
            Environment.GetEnvironmentVariable("CosmosKey"));
        static Container container = cosmosClient.GetContainer("MovieAppDB", "UserList");
       
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
                listItems = new List<ListItem>()
            }) ;
            
            string responseMessage = "This HTTP triggered function executed successfully. An item has been added: UserList";

            return new OkObjectResult(responseMessage);
        }
     
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
                patchOperations: new[] { PatchOperation.Replace("/listItems", userList.ListItems) });
    
            string responseMessage = "This HTTP triggered function executed successfully. An item has been updated : UserList";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("GetUserLists")]
        public static IActionResult GetUserLists(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "UserLists/{userid}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT * FROM c where c.userid={userid}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / UserLists");
            return new OkObjectResult(documents);
        }


        [FunctionName("GetProductionsInList")]
        public static IActionResult GetProductionsInList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetProductionsInList/{userid}/{listname}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT c.listItems FROM c where c.userid={userid} and c.listname={listname}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / GetProductionsInList");
            return new OkObjectResult(documents);
        }

        [FunctionName("GetProductionsInListById")]
        public static IActionResult GetMoviesInListById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetProductionsInListById/{id}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT c FROM c where c.id={id}")] IEnumerable<object> documents,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request. / GetProductionsInListById");
            return new OkObjectResult(documents);
        }

        [FunctionName("DeleteList")]
        public static async Task<IActionResult> DeleteList(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeleteList/{userid}/{listname}")] HttpRequest req,
        [CosmosDB(
        databaseName: "MovieAppDB",
        collectionName: "UserList",
        ConnectionStringSetting = "DatabaseConnectionString",
        SqlQuery = "SELECT c.listItems FROM c where c.userid={userid} and c.listname={listname}")] IEnumerable<UserList> documents,
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
