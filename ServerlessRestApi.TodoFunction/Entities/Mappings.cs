namespace ServerlessRestApi.TodoFunction.Entities
{
    public static class Mappings
    {
        public static TodoTableEntity ToTodoTableEntity(this Todo todo)
        {
            return new TodoTableEntity
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription
            };
        }

        public static Todo ToTodo(this TodoTableEntity todoTableEntity)
        {
            return new Todo
            {
                Id = todoTableEntity.RowKey,
                CreatedTime = todoTableEntity.CreatedTime,
                IsCompleted = todoTableEntity.IsCompleted,
                TaskDescription = todoTableEntity.TaskDescription
            };
        }
    }
}