namespace ServerlessRestApi.TodoFunction.Entities
{
    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }

        public bool IsCompleted { get; set; }
    }
}