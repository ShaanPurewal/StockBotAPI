using API.Models;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public class AuthentificationService
    {
        public Response AuthenticateKey(string key, ref BotService botService)
        { 
            if (!botService.KeyExists(HashKey(key))) return new Response { Result = "Key is not valid", Status = Status.Failed };
            return new Response { Result = "Key is valid", Status = Status.Successful };
        }

        public Response AdminAuthenticateKey(string key, ref BotService botService)
        {
            Response authResponse = AuthenticateKey(key, ref botService);
            if (authResponse.Status == Status.Failed) return authResponse;

            Bot foundBot = botService.FindBot(HashKey(key));
            if (!foundBot.isAdmin) return new Response { Result = "Bot isn't an admin", Status = Status.Failed };

            return new Response { Result = "Admin key is valid", Status = Status.Successful };
        }

        public string GenerateKey() { return Guid.NewGuid().ToString(); }

        public string HashKey(string key)
        {
            byte[] data = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
