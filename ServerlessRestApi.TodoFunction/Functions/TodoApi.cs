using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessRestApi.TodoFunction.Entities;

namespace ServerlessRestApi.TodoFunction.Functions
{
    public static class TodoApi
    {
        private static List<Todo> items = new List<Todo>();

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")]
            HttpRequest request,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            TodoCreateModel input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);

            Todo todo = new Todo
            {
                TaskDescription = input.TaskDescription
            };
            items.Add(todo);
            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static IActionResult GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")]
            HttpRequest request,
            ILogger log)
        {
            log.LogInformation("Getting all todo list items");
            return new OkObjectResult(items);
        }


        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")]
            HttpRequest request,
            ILogger log,
            string id)
        {
            log.LogInformation($"Getting todo by Id. Id = {id}");
            Todo todo = items.FirstOrDefault(x => x.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<ActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")]
            HttpRequest request,
            ILogger log,
            string id)
        {
            log.LogInformation($"Updating todo by Id. Id = {id}");
            Todo todo = items.FirstOrDefault(x => x.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            TodoUpdateModel updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);

            todo.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                todo.TaskDescription = updated.TaskDescription;
            }

            return new OkObjectResult(todo);
        }

        [FunctionName("DeleteTodo")]
        public static IActionResult DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")]
            HttpRequest request,
            ILogger log,
            string id)
        {
            log.LogInformation($"Deleting todo by Id. Id = {id}");
            Todo todo = items.FirstOrDefault(x => x.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            items.Remove(todo);
            return new OkObjectResult(todo);
        }
    }
}