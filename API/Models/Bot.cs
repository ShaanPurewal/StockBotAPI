using System.Xml.Serialization;

namespace API.Models
{
    [Serializable]
    public class Bot
    {
        public readonly static int DEFAULT_BALANCE = 100;
        public DateTime Created { get; set; } = DateTime.Now;
        public int Balance { get; set; } = DEFAULT_BALANCE;
        public required string Name { get; set; }
        public required string AuthenticationKey { get; set; }
        public bool isAdmin { get; set; } = false;
    }
}
