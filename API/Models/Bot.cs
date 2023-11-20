namespace API.Models
{
    public class Bot
    {
        public DateOnly Created { get; set; }

        public int Balance { get; set; }

        public string Name { get; set; }

        public string AuthenticationKey { get; set; }
    }
}
