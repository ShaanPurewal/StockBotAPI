using Discord.WebSocket;
using Discord;
using API.Models;

namespace API.Services
{
    public class DiscordService
    {
        private readonly string botToken = "";
        ulong tradeChannelId = 0;
        ulong portfolioChannelId = 0;
        public DiscordService()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

            botToken = configuration["Secret:BotToken"];
            tradeChannelId = ulong.Parse(configuration["Secret:tradeID"]);
            portfolioChannelId = ulong.Parse(configuration["Secret:portfolioID"]);
        }

        public async void TrySendTrade(string message)
        {
            try
            {
                var client = new DiscordSocketClient();
                await client.LoginAsync(TokenType.Bot, botToken);
                await client.StartAsync();
                var channel = await client.GetChannelAsync(tradeChannelId) as IMessageChannel;
                await channel!.SendMessageAsync(message);
            } catch (Exception e)
            {
                Console.WriteLine($"Message '{message}' failed to send");
                Console.WriteLine(e);
            }
            
        }

        public async void TrySendPortfolios(List<Bot> bots)
        {
            foreach (Bot bot in bots)
            {
                try
                {
                    var client = new DiscordSocketClient();
                    await client.LoginAsync(TokenType.Bot, botToken);
                    await client.StartAsync();
                    var channel = await client.GetChannelAsync(portfolioChannelId) as IMessageChannel;

                    string message = $"**{bot.Name}**" +
                                    $"  \nbalance: ${bot.Balance}\n";
                    foreach (string key in bot.Portfolio.Keys)
                    {
                        message += $"   {key}: {bot.Portfolio[key]} shares,\n";
                    }

                    await channel!.SendMessageAsync(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to send '{bot.Name}' portfolio");
                    Console.WriteLine(e);
                }
            }

        }
    }
}
