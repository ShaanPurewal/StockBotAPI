using Discord.WebSocket;
using Discord;
using API.Models;

namespace API.Services
{
    public class DiscordService
    {
        public async void TrySendTrade(string message)
        {
            try
            {
                const string botToken = @"MTE3Nzc5NTk4NjYyMjUyOTU5Ng.GEbGYT.nPXp5IbeVBfTvqXW8oHHa-OSe8uG9nHj58i9vk";
                ulong channelId = 1177795306444832819;

                var client = new DiscordSocketClient();
                await client.LoginAsync(TokenType.Bot, botToken);
                await client.StartAsync();
                var channel = await client.GetChannelAsync(channelId) as IMessageChannel;
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
                    const string botToken = @"MTE3Nzc5NTk4NjYyMjUyOTU5Ng.GEbGYT.nPXp5IbeVBfTvqXW8oHHa-OSe8uG9nHj58i9vk";
                    ulong channelId = 1177809579329990716;

                    var client = new DiscordSocketClient();
                    await client.LoginAsync(TokenType.Bot, botToken);
                    await client.StartAsync();
                    var channel = await client.GetChannelAsync(channelId) as IMessageChannel;

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
