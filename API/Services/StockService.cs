
using API.Models;
using Newtonsoft.Json.Linq;

namespace API.Services
{
    public class StockService
    {
        private readonly string apiKey = "YJRKRWQ56XY87CF1";

        public async Task<Response> GetPriceAsync(string TKR)
        {
            double currentPrice;
            try
            {
                string apiUrl = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={TKR}&apikey={apiKey}";

                HttpClient client = new HttpClient();
                string response = await client.GetStringAsync(apiUrl);

                JObject data = JObject.Parse(response);
                JObject? globalQuote = data["Global Quote"]?.ToObject<JObject>();

                var priceValue = globalQuote != null ? globalQuote["05. price"] : null;
                if (priceValue == null) { return new Response { Result = "Failed to retrieve price (check TKR)", Status = Status.Failed }; }

                currentPrice = Double.Parse(priceValue.ToString());
            }
            catch (Exception e)
            {
                return new Response { Result = e, Status = Status.Error, Message = $"Failed to retrieve price for '{TKR}'" };
            }

            return new Response { Result = currentPrice, Status = Status.Successful, Message = $"The current price of {TKR} is: ${currentPrice}" };
        }
    }
}
