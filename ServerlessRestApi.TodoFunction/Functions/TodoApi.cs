using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using ServerlessRestApi.TodoFunction.Entities;

namespace ServerlessRestApi.TodoFunction.Functions
{
    public static class TodoApi
    {
        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")]
            HttpRequest request,
            [Table("todos", Connection = "AzureWebJobsStorage")]
            IAsyncCollector<TodoTableEntity> todoTable,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            TodoCreateModel input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            Todo todo = new Todo
            {
                TaskDescription = input.TaskDescription
            };

            await todoTable.AddAsync(todo.ToTodoTableEntity());
            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")]
            HttpRequest request,
            [Table("todos", Connection = "AzureWebJobsStorage")]
            CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Getting all todo list items");
            TableQuery<TodoTableEntity> query = new TableQuery<TodoTableEntity>();
            TableQuerySegment<TodoTableEntity> segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToTodo));
        }


        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")]
            HttpRequest request,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")]
            TodoTableEntity todoTableEntity,
            ILogger log,
            string id)
        {
            log.LogInformation($"Getting todo by Id. Id = {id}");
            if (todoTableEntity == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }

            return new OkObjectResult(todoTableEntity.ToTodo());
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")]
            HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]
            CloudTable todoTable,
            ILogger log,
            string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TodoUpdateModel updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
            TableOperation findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
            TableResult findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }

            TodoTableEntity existingRow = (TodoTableEntity) findResult.Result;
            existingRow.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                existingRow.TaskDescription = updated.TaskDescription;
            }

            TableOperation replaceOperation = TableOperation.Replace(existingRow);
            await todoTable.ExecuteAsync(replaceOperation);
            return new OkObjectResult(existingRow.ToTodo());
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")]
            HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]
            CloudTable todoTable,
            ILogger log,
            string id)
        {
            log.LogInformation($"Deleting todo by Id. Id = {id}");
            TableOperation deleteOperation = TableOperation.Delete(new TableEntity()
                {PartitionKey = "TODO", RowKey = id, ETag = "*"});
            try
            {
                TableResult deleteResult = await todoTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}