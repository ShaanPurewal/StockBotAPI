using API.Models;
using System.Xml.Serialization;

namespace API.Services
{
    public class BotService
    {
        private Dictionary<string, Bot> _bots = [];
        private readonly string botDirectory = Path.Combine(Directory.GetCurrentDirectory(), "bots");

        public BotService()
        {
            if (!Directory.Exists(botDirectory)) { Directory.CreateDirectory(botDirectory); }
            LoadBots();
        }

        private Response LoadBots()
        {
            try
            {
                foreach (string fileName in Directory.GetFiles(botDirectory))
                {
                    XmlSerializer serializer = new(typeof(Bot));
                    FileStream fileStream = new(Path.Combine(botDirectory, fileName), FileMode.Open);
                    Bot readBot;

                    readBot = (Bot)serializer.Deserialize(fileStream);
                    if (readBot != null) _bots.Add(readBot.AuthenticationKey, readBot);
                    fileStream.Close();
                }
            } catch (Exception e)
            {
                return new Response { Result = e, Status = Status.Error, Message = "Reading from bot files through and error" };
            }
            return new Response { Result = "Successfully loaded bots", Status = Status.Successful };
        }

        public Response CreateBot(Bot newBot)
        {
            if (NameExists(newBot.Name)) return new Response { Result = $"Bot '{newBot.Name}' already exists", Status=Status.Failed };

            XmlSerializer serializer = new(typeof(Bot));
            string botFilePath = Path.Combine(botDirectory, newBot.AuthenticationKey);
            TextWriter writer = new StreamWriter(botFilePath);

            try { serializer.Serialize(writer, newBot); } 
            catch (Exception e) { return new Response { Result = e, Status = Status.Error, Message = "Error thrown trying to serialize new bot"}; }
            writer.Close();

            if (!File.Exists(botFilePath)) return new Response { Result = "Failed to create bot file", Status = Status.Failed };

            _bots.Add(newBot.AuthenticationKey, newBot);
            return new Response { Result = "key not set", Status = Status.Successful, Message = $"Successfully created bot '{newBot.Name}'"};
        }

        public Response UpdateBot(Bot newBot)
        {
            if (!KeyExists(newBot.AuthenticationKey)) return new Response { Result = $"Bot '{newBot.Name}' doesn't exists", Status = Status.Failed };

            XmlSerializer serializer = new(typeof(Bot));
            string botFilePath = Path.Combine(botDirectory, newBot.AuthenticationKey);
            TextWriter writer = new StreamWriter(botFilePath);

            try { serializer.Serialize(writer, newBot); }
            catch (Exception e) { return new Response { Result = e, Status = Status.Error, Message = "Error thrown trying to serialize new bot" }; }
            writer.Close();

            if (!File.Exists(botFilePath)) return new Response { Result = "Failed to create bot file", Status = Status.Failed };

            return new Response { Result = $"Successfully updated bot '{newBot.Name}'", Status = Status.Successful };
        }

        public Response DeleteBot(Bot bot)
        {
            if (!_bots.ContainsKey(bot.AuthenticationKey)) return new Response { Result = "Failed to find bot (for deletion)", Status = Status.Failed };
            _bots.Remove(bot.AuthenticationKey);

            string botFilePath = Path.Combine(botDirectory, bot.AuthenticationKey);
            if (!File.Exists(botFilePath)) return new Response { Result = "Failed to find bot file", Status = Status.Failed };
            File.Delete(botFilePath);

            return new Response { Result = $"Successfully deleted bot '{bot.Name}'", Status = Status.Successful };
        }

        public Response UpdatePrice(string hashed_key, double price)
        {
            if (!KeyExists(hashed_key)) return new Response { Result = "Couldn't find bot/key", Status = Status.Failed };

            Bot foundBot = FindBot(hashed_key);
            if (price < 0 && foundBot.Balance < -price) return new Response { Result = $"Insuffecient Balance, Price:{price} Balance:{foundBot.Balance}", Status = Status.Failed };

            foundBot.Balance += price;
            UpdateBot(foundBot);
            return new Response { Result = foundBot.Balance, Status = Status.Successful, Message = $"Successfully updated balance, New Balance:{foundBot.Balance}" };
        }

        public Response UpdatePortfolio(string key, string TKR, int quantity)
        {
            Bot foundBot = FindBot(key);

            foundBot.Portfolio.TryGetValue(TKR, out double inventory);

            if (inventory < -quantity) return new Response { Result = $"Insuffecient stock in current holdings, inventory:{inventory} request:{quantity}", Status = Status.Failed };

            foundBot.Portfolio[TKR] = inventory + quantity;
            if (foundBot.Portfolio[TKR] == 0) foundBot.Portfolio.Remove(TKR);
            UpdateBot(foundBot);

            return new Response { Result = inventory + quantity, Status = Status.Successful, Message = $"Successfully updated portfolio, '{TKR}': {inventory + quantity}" };
        }

        public Bot FindBot(string key) { return _bots[key]; }

        public bool KeyExists(string key) { return _bots.ContainsKey(key); }

        public bool NameExists(string botName) { return _bots.Values.Any(bot => bot.Name.Equals(botName)); }
    }
}
