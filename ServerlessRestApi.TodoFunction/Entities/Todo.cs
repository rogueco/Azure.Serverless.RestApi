using System;

namespace ServerlessRestApi.TodoFunction.Entities
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");

        public DateTime CreatedTime { get; set; } = DateTime.Now;

        public string TaskDescription { get; set; }

        public bool IsCompleted { get; set; }
    }
}