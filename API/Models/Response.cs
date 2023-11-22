namespace API.Models
{
    public enum Status
    {
        Successful,
        Failed,
        Error
    }

    public class Response
    {
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public required Status Status { get; set; } = Status.Failed;
        public required Object Result { get; set; } = "Result not set";
        public string Message { get; set; } = "Message not set";
    }
}
