using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }

    [Route("auth")]
    public class AuthController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            var token = HttpContext.GetTokenAsync("access_token").Result; // Get the token (optional here)
            Console.WriteLine("Access Token: " + token);
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "http://localhost:3000/newdash" // Redirect to React app after login
            }, OpenIdConnectDefaults.AuthenticationScheme); // Use the correct scheme
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/"
            }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("user")]
        public IActionResult GetUser()
        {
            return Ok(User.Identity.Name);
        }
        [HttpGet("tokens")]
      //  [Authorize] // Ensure the user is authenticated
        public async Task<IActionResult> GetTokens()
        {
            // Retrieve tokens from the HttpContext
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var idToken = await HttpContext.GetTokenAsync("id_token");

            if (accessToken == null && idToken == null)
            {
                return Unauthorized("No tokens found. Are you authenticated?");
            }

            // Return tokens in the response (for testing/debugging purposes only)
            return Ok(new
            {
                AccessToken = accessToken,
                IdToken = idToken,
                UserName = User.Identity?.Name ?? "Unknown User"
            });
        }
    }


}
