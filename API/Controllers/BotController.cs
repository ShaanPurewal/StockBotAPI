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
        private StockService _stockService = stockService;
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

        [HttpPost("buy", Name = "ProcessBuy")]
        public IActionResult ProcessBuy(string key, string TKR, int quantity)
        {
            if (quantity <= 0) { return BadRequest(new Response { Result = "Invalid quantity", Status = Status.Failed }); }

            string hashed_key = _authService.HashKey(key);
            _logger.LogInformation($"Authenticating key...");
            if (!_botService.KeyExists(hashed_key)) { _logger.LogInformation($"Key is not valid"); return Unauthorized(new Response { Result = "Could not find bot/Invalid key", Status = Status.Failed }); }
            _logger.LogInformation($"Key is valid");

            _logger.LogInformation($"Fetching price...");
            Response priceResponse = _stockService.GetStockPrice(hashed_key, TKR).Result;
            if (priceResponse.Status == Status.Failed) { _logger.LogInformation($"Failed to fetch price"); return BadRequest(priceResponse); }
            _logger.LogInformation($"Successfuly fetched price");

            _logger.LogInformation($"Updating portfolio...");
            _botService.UpdatePortfolio(hashed_key, TKR, quantity);
            _logger.LogInformation($"Successfuly updated portfolio");

            _logger.LogInformation($"Logging trade...");
            Response tradeLogResponse = _stockService.UpdateTradeLog(hashed_key, TKR, quantity);
            if (tradeLogResponse.Status != Status.Successful) { _logger.LogInformation($"Failed to log trade"); return BadRequest(tradeLogResponse); }
            _logger.LogInformation($"Successfuly logged trade");

            _logger.LogInformation($"Updating balance...");
            Response botResponse = _botService.UpdatePrice(hashed_key, -(double)priceResponse.Result * quantity);

            if (botResponse.Status == Status.Failed) { _logger.LogInformation($"Failed to update balance"); return BadRequest(botResponse); }
            _logger.LogInformation($"Successfuly updated balance");
            return Ok(botResponse);
        }

        [HttpPost("sell", Name = "ProcessSell")]
        public IActionResult ProcessSell(string key, string TKR, int quantity)
        {
            if (quantity <= 0) { return BadRequest(new Response { Result = "Invalid quantity", Status = Status.Failed }); }

            string hashed_key = _authService.HashKey(key);
            _logger.LogInformation($"Authenticating key...");
            if (!_botService.KeyExists(hashed_key)) { _logger.LogInformation($"Key is not valid"); return Unauthorized(new Response { Result = "Could not find bot/Invalid key", Status = Status.Failed }); }
            _logger.LogInformation($"Key is valid");

            _logger.LogInformation($"Fetching price...");
            Response priceResponse = _stockService.GetStockPrice(hashed_key, TKR).Result;
            if (priceResponse.Status == Status.Failed) { _logger.LogInformation($"Failed to fetch price"); return BadRequest(priceResponse); }
            _logger.LogInformation($"Successfuly fetched price");

            _logger.LogInformation($"Updating portfolio...");
            Response portfolioResponse = _botService.UpdatePortfolio(hashed_key, TKR, -quantity);
            if (portfolioResponse.Status == Status.Failed) { _logger.LogInformation($"Failed to update portfolio"); return BadRequest(portfolioResponse); }
            _logger.LogInformation($"Successfuly updated portfolio");

            _logger.LogInformation($"Logging trade...");
            Response tradeLogResponse = _stockService.UpdateTradeLog(hashed_key, TKR, -quantity);
            if (tradeLogResponse.Status != Status.Successful) { _logger.LogInformation($"Failed to log trade"); return BadRequest(tradeLogResponse); }
            _logger.LogInformation($"Successfuly logged trade");

            _logger.LogInformation($"Updating balance...");
            Response botResponse = _botService.UpdatePrice(hashed_key, (double)priceResponse.Result * quantity);

            if (botResponse.Status == Status.Failed) { _logger.LogInformation($"Failed to update balance"); return BadRequest(botResponse); }
            _logger.LogInformation($"Successfuly updated balance");
            return Ok(botResponse);
        }

        [HttpGet("price", Name = "GetPrice")]
        public IActionResult GetPrice(string TKR)
        {
            Response priceResponse = _stockService.GetStockPrice("-1", TKR).Result;
            if (priceResponse.Status == Status.Failed) { return BadRequest(priceResponse); }

            return Ok(priceResponse);
        }

        [HttpGet("trades", Name = "GetTrades")]
        public IActionResult GetTrades(string key)
        {
            string hashed_key = _authService.HashKey(key);
            if (!_botService.KeyExists(hashed_key)) return Unauthorized(new Response { Result = "Could not find bot/Invalid key", Status = Status.Failed });

            return Ok(_stockService.GetTrades(hashed_key));
        }
    }
}
