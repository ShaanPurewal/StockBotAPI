namespace API.Models
{
    public enum Status
    {
        Successful,
        Failed
    }

    public class Response
    {
        public DateOnly timeStamp = System.DateOnly.FromDateTime(DateTime.Now);
        public required Status status { get; set; } = Status.Failed;
        public required Object result { get; set; } = "Object not set";
    }
}
