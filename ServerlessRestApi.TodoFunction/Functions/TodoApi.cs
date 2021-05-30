using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using ServerlessRestApi.TodoFunction.Entities;
using StorageException = Microsoft.Azure.Cosmos.Table.StorageException;

namespace ServerlessRestApi.TodoFunction.Functions
{
    public static class TodoApi
    {
        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")]
            HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]
            IAsyncCollector<TodoTableEntity> todoTable,
            [Queue("todos", Connection = "AzureWebJobsStorage")]
            IAsyncCollector<Todo> todoQueue,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TodoCreateModel input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            Todo todo = new Todo() {TaskDescription = input.TaskDescription};
            await todoTable.AddAsync(todo.ToTodoTableEntity());
            await todoQueue.AddAsync(todo);
            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")]
            HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]
            CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Getting todo list items");
            TableQuery<TodoTableEntity> query = new TableQuery<TodoTableEntity>();
            TableQuerySegment<TodoTableEntity> segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToTodo));
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")]
            HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")]
            TodoTableEntity todo,
            ILogger log, string id)
        {
            log.LogInformation("Getting todo item by id");
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }

            return new OkObjectResult(todo.ToTodo());
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")]
            HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")]
            CloudTable todoTable,
            ILogger log, string id)
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
            ILogger log, string id)
        {
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