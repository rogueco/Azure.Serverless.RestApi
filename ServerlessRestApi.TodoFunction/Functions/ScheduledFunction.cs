using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ServerlessRestApi.TodoFunction.Entities;
using  Microsoft.Azure.Cosmos.Table;

namespace ServerlessRestApi.TodoFunction.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task RunAsync(
            [TimerTrigger("0 */5 * * * *")] TimerInfo myTimer,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            TableQuery<TodoTableEntity> query = new TableQuery<TodoTableEntity>();
            TableQuerySegment<TodoTableEntity> segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            int deleted = 0;
            foreach (TodoTableEntity todoTableEntity in segment)
            {
                if (todoTableEntity.IsCompleted)
                {
                    await todoTable.ExecuteAsync(TableOperation.Delete(todoTableEntity));
                    deleted++;
                }
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
            log.LogInformation($"Deleted {deleted} item at {DateTime.UtcNow}");
        }
    }
}