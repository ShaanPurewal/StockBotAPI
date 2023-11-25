
using API.Models;
using System.Xml.Serialization;

namespace API.Services
{
    public class StockService
    {
        private readonly string apiKey = "";

        private readonly double requestThreshold = 60 * 5;
        private static Dictionary<string, DateTime> lastRequest = [];

        private readonly double cacheThreshold = 60 * 10;
        private static Dictionary<string, PriceCache> priceCache = [];

        private readonly string tradeDirectory = Path.Combine(Directory.GetCurrentDirectory(), "trades");
        private static Dictionary<string, List<Trade>> _trades = [];

        private static bool loaded = false;

        public StockService()
        {

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

            apiKey = configuration["Secret:StockApiKey"];

            if (!Directory.Exists(tradeDirectory)) { Directory.CreateDirectory(tradeDirectory); }
            LoadTrades();
        }

        public async Task<Response> GetStockPrice(string hashedKey, string symbol)
        {
            DateTime currentDT = DateTime.Now;

            if (priceCache.TryGetValue(symbol, out PriceCache cachedPrice) && currentDT.Subtract(cachedPrice.CacheDT).Seconds < cacheThreshold)
            {
                return new Response { Result = cachedPrice.Price, Status = Status.Successful, Message = "Cached Price Value" };
            }

            bool exists = lastRequest.TryGetValue(hashedKey, out DateTime lastDT);
            TimeSpan timePassed = currentDT - lastDT;
            if (exists && timePassed.TotalSeconds < requestThreshold)
            {
                return new Response { Result = $"{requestThreshold - timePassed.TotalSeconds} seconds until next request", Status = Status.Failed };
            }
            lastRequest[hashedKey] = currentDT;

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://twelve-data1.p.rapidapi.com/price?symbol={symbol}&format=json&outputsize=30"),
                    Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", "twelve-data1.p.rapidapi.com" },
                },
                };

                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                // Parse the JSON response to extract the price value
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<PriceResponse>(body);
                symbol = symbol.ToUpper();
                if (json != null && json.Price != null)
                {
                    priceCache[symbol] =  new PriceCache { Price = (double)json.Price };
                    return new Response { Result = (double)json.Price, Status = Status.Successful };
                }
                else
                {
                    return new Response { Result = $"Unable to find price for {symbol}", Status = Status.Failed };
                }
            }
        }

        private Response LoadTrades()
        {
            if (loaded) { return new Response { Result = "Successfully loaded trade logs", Status = Status.Successful }; }
            loaded = true;
            try
            {
                foreach (string fileName in Directory.GetFiles(tradeDirectory))
                {
                    XmlSerializer serializer = new(typeof(List<Trade>));
                    FileStream fileStream = new(Path.Combine(tradeDirectory, fileName), FileMode.Open);
                    List<Trade> readTrades;

                    readTrades = (List<Trade>)serializer.Deserialize(fileStream);
                    if (readTrades != null) _trades.Add(fileName.Substring(fileName.LastIndexOfAny("\\".ToCharArray()) + 1), readTrades);
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                return new Response { Result = e, Status = Status.Error, Message = "Error read trade logs" };
            }
            return new Response { Result = "Successfully loaded trade logs", Status = Status.Successful };
        }

        public Response UpdateTradeLog(string key, string symbol, int quantity)
        {
            symbol = symbol.ToLower();

            if (_trades.ContainsKey(key)) { _trades[key].Add(new Trade { TKR = symbol, Quantity = quantity }); }
            else
            {
                List<Trade> newTrade = [new Trade { TKR = symbol, Quantity = quantity }];

                _trades[key] = newTrade;
            }

            return StoreTrades(key);
        }

        private Response StoreTrades(string key)
        {
            if (!_trades.TryGetValue(key, out List<Trade> newTradeLog)) { return new Response { Result = "Cannot find any logs associated to user", Status = Status.Failed }; }

            XmlSerializer serializer = new(typeof(List<Trade>));
            string tradeFilePath = Path.Combine(tradeDirectory, key);
            TextWriter writer = new StreamWriter(tradeFilePath);

            try { serializer.Serialize(writer, newTradeLog); }
            catch (Exception e) { return new Response { Result = e, Status = Status.Error, Message = "Error thrown trying to serialize new trade log" }; }
            writer.Close();

            if (!File.Exists(tradeFilePath)) return new Response { Result = "Failed to create trade log file", Status = Status.Failed };

            return new Response { Result = $"Successfully updated trade log for '{key}'", Status = Status.Successful };
        }

        public Response GetTrades(string key)
        {
            if(!_trades.TryGetValue(key, out List<Trade> tradeLogs)) { return new Response { Result = "Couldn't find trade logs", Status= Status.Failed }; }
            return new Response { Result = tradeLogs, Status = Status.Successful };
        }
    }
    public class PriceResponse
    {
        public required double? Price { get; set; }
    }

    public class PriceCache
    {
        public required double Price { get; set; }
        public DateTime CacheDT { get; set; } = DateTime.Now;
    }
}
