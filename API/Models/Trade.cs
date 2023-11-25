namespace API.Models
{
    [Serializable]
    public class Trade
    {
        public required string TKR { get; set; }
        public required int Quantity { get; set; }
        public string Action { get { return Quantity > 0 ? "Buy" : "Sell"; } }
        public DateTime TradeTimeStamp { get; set; } = DateTime.Now;
    }
}
