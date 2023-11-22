using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("/")]
    public class BotController(ILogger<BotController> logger, AuthentificationService authService, BotService botService, StockService stockService) : ControllerBase
    {

        private readonly ILogger<BotController> _logger = logger;
        private readonly AuthentificationService _authService = authService;
        private readonly StockService _stockService = stockService;
        private BotService _botService = botService;

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot()
        {
            _logger.LogInformation("Request to root");
            return Ok(new Response { Result = "Welcome to the Stock Bot API. Please Navigate to the /swagger/index.html endpoint for supported calls.", Status = Status.Successful });
        }

        [HttpGet("validateKey", Name = "GetAuthCheck")]
        public IActionResult GetAuthCheck(string key)
        {
            _logger.LogInformation($"Validating key...");
            Response authResponse = _authService.AuthenticateKey(key, ref _botService);

            if (authResponse.Status == Status.Failed) { _logger.LogInformation($"Key is invalid"); return Unauthorized(authResponse); }

            _logger.LogInformation($"Key is valid");
            return Ok(authResponse);
        }

        [HttpPost("register", Name = "RegisterBot")]
        public IActionResult RegisterBot(string adminKey, string name)
        {
            _logger.LogInformation($"Authenticating admin key...");
            Response authResponse = _authService.AdminAuthenticateKey(adminKey, ref _botService);

            if (authResponse.Status == Status.Failed) { _logger.LogInformation($"Admin key is invalid"); return Unauthorized(authResponse); }
            _logger.LogInformation($"Admin key is valid");

            _logger.LogInformation($"Registering new bot '{name}'...");

            _logger.LogInformation($"Generating a new auth key...");
            string newKey = _authService.GenerateKey();

            _logger.LogInformation($"Hashing key...");
            string hashedKey = _authService.HashKey(newKey);

            Bot newBot = new() { Name = name, AuthenticationKey = hashedKey };

            _logger.LogInformation($"Storing bot information...");
            Response createBotResponse = _botService.CreateBot(newBot);

            if (createBotResponse.Status == Status.Error) { _logger.LogInformation($"Error thrown when creating Bot"); return StatusCode(500, createBotResponse); }
            if (createBotResponse.Status == Status.Failed) { _logger.LogInformation($"Failed to create bot"); return BadRequest(createBotResponse); }

            _logger.LogInformation($"Successfully registered bot '{name}'");
            createBotResponse.Result = newKey;
            return Ok(createBotResponse);
        }

        [HttpDelete("delete", Name = "DeleteBot")]
        public IActionResult DeleteBot(string adminKey, string targetKey)
        {
            _logger.LogInformation($"Authenticating admin key...");
            Response authResponse = _authService.AdminAuthenticateKey(adminKey, ref _botService);

            if (authResponse.Status == Status.Failed) { _logger.LogInformation($"Admin key is invalid"); return Unauthorized(authResponse); }
            _logger.LogInformation($"Admin key is valid");

            _logger.LogInformation($"Authenticating target key...");
            Response authTargetResponse = _authService.AuthenticateKey(targetKey, ref _botService);

            if (authTargetResponse.Status == Status.Failed) { _logger.LogInformation($"Target key is invalid"); return Unauthorized(authTargetResponse); }
            _logger.LogInformation($"Target key is valid");

            _logger.LogInformation($"Searching for bot...");
            Bot foundBot = _botService.FindBot(_authService.HashKey(targetKey));

            _logger.LogInformation($"Deleting bot '{foundBot.Name}'...");
            Response deleteBotResponse = _botService.DeleteBot(foundBot);

            if (deleteBotResponse.Status == Status.Failed) { _logger.LogInformation($"Failed to deleted bot '{foundBot.Name}'"); return BadRequest(deleteBotResponse); }

            _logger.LogInformation($"Successfully deleted bot '{foundBot.Name}'");
            return Ok(deleteBotResponse);
        }

        [HttpGet("bot", Name = "GetBot")]
        public IActionResult GetBot(string key)
        {
            string hashed_key = _authService.HashKey(key);
            if (!_botService.KeyExists(hashed_key)) return Unauthorized(new Response { Result = "Could not find bot/Invalid key", Status = Status.Failed });

            return Ok(new Response { Result = _botService.FindBot(hashed_key), Status = Status.Successful });
        }

        [HttpPost("trade", Name = "ProcessTrade")]
        public IActionResult ProcessTrade(string key, string TKR)
        {
            string hashed_key = _authService.HashKey(key);
            if (!_botService.KeyExists(hashed_key)) return Unauthorized(new Response { Result = "Could not find bot/Invalid key", Status = Status.Failed });

            Response priceResponse = _stockService.GetPrice(TKR);
            if (priceResponse.Status == Status.Failed) { return BadRequest(priceResponse); }

            Response botResponse = _botService.UpdatePrice(hashed_key, (double)priceResponse.Result);

            if (botResponse.Status == Status.Failed) return BadRequest(botResponse);
            return Ok(botResponse);
        }

        [HttpGet("price", Name = "GetPrice")]
        public IActionResult GetPrice(string TKR)
        {
            Response priceResponse = _stockService.GetPrice(TKR);
            if (priceResponse.Status == Status.Failed) { return BadRequest(priceResponse); }

            return Ok(priceResponse);
        }
    }
}
