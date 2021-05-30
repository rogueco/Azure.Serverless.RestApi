using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServerlessRestApi.TodoFunction.Entities
{
    public class TodoTableEntity : TableEntity 
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}