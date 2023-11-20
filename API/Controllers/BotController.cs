using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("/")]
    public class BotController(ILogger<BotController> logger, AuthentificationService authService) : ControllerBase
    {

        private readonly ILogger<BotController> _logger = logger;
        private AuthentificationService _authService = authService;

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot()
        {
            _logger.LogInformation("Request to root");
            return Ok(new Response { result = "Welcome to the Stock Bot API. Please Navigate to the /swagger/index.html endpoint for supported calls.", status = Status.Successful });
        }

        [HttpGet("validateKey", Name = "GetAuthCheck")]
        public IActionResult GetAuthCheck(string key)
        {
            _logger.LogInformation($"The key '{key}' was authenicated");

            if(_authService.AuthenticateKey(key)) return Ok(new Response { result = "Valid Key", status = Status.Successful });

            return Unauthorized(new Response { result = "Invalid Key", status = Status.Failed });
        }
    }
}
