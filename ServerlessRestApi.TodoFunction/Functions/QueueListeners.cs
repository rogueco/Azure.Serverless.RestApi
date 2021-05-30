using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServerlessRestApi.TodoFunction.Entities;

namespace ServerlessRestApi.TodoFunction.Functions
{
    public static class QueueListeners
    {
        [FunctionName("QueueListeners")]
        public static async Task RunAsync([QueueTrigger("todos", Connection = "AzureWebJobsStorage")]
            Todo todo,
            [Blob("todos", Connection = "AzureWebJobsStorage")]
            CloudBlobContainer container,
            ILogger log)
        {
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blob = container.GetBlockBlobReference($"{todo.Id}.txt");
            await blob.UploadTextAsync($"Create a new task: {todo.TaskDescription}");
            log.LogInformation($"C# Queue trigger function processed: {todo.TaskDescription}");
        }
    }
}