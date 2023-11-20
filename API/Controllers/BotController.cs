using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("/")]
    public class BotController(ILogger<BotController> logger) : ControllerBase
    {

        private readonly ILogger<BotController> _logger = logger;

        [HttpGet(Name = "GetRoot")]
        public String Get()
        {
            this._logger.LogInformation("Request to root");
            return "Welcome to the Stock Bot API. Please Navigate to the /swagger/index.html endpoint for supported calls.";
        }
    }
}
