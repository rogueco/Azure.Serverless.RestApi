using System;
using Microsoft.Azure.Cosmos.Table;

namespace ServerlessRestApi.TodoFunction.Entities
{
    public class TodoTableEntity : TableEntity 
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}