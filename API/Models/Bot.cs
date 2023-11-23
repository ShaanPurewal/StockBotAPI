using Microsoft.VisualStudio.Services.Common;

namespace API.Models
{
    [Serializable]
    public class Bot
    {
        public readonly static double DEFAULT_BALANCE = 100000;
        public DateTime Created { get; set; } = DateTime.Now;
        public double Balance { get; set; } = DEFAULT_BALANCE;
        public required string Name { get; set; }
        public required string AuthenticationKey { get; set; }
        public SerializableDictionary<string, double> Portfolio { get; set; } = [];
        public bool isAdmin { get; set; } = false;
    }
}
