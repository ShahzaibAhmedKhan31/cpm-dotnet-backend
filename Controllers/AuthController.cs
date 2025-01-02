using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [ApiController]
    // [Route("[controller]")]
    // public class WeatherForecastController : ControllerBase
    // {
    //     private static readonly string[] Summaries = new[]
    //     {
    //         "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    //     };

    //     private readonly ILogger<WeatherForecastController> _logger;

    //     public WeatherForecastController(ILogger<WeatherForecastController> logger)
    //     {
    //         _logger = logger;
    //     }

    //     [HttpGet(Name = "GetWeatherForecast")]
    //     [Authorize]
    //     public IEnumerable<WeatherForecast> Get()
    //     {
    //         return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //         {
    //             Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    //             TemperatureC = Random.Shared.Next(-20, 55),
    //             Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //         })
    //         .ToArray();
    //     }
    // }

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
                RedirectUri = "http://localhost:3000/dashboard" // Redirect to React app after login
            }, OpenIdConnectDefaults.AuthenticationScheme); // Use the correct scheme
        } 

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Clear all cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // // Sign out from authentication schemes
            // return SignOut(new AuthenticationProperties
            // {
            //     RedirectUri = "http://localhost:3000/auth/login" // Redirect to home page or another specified URL after logout
            // }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

            SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
            return Ok(new { statusCode=200,Message = "Logged out successfully" });

        }


        [HttpGet("user")]
        public IActionResult GetUser()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var email = User.Identity?.Name;

            // If the email claim is not found, try another approach (Azure AD often uses "preferred_username")
            if (string.IsNullOrEmpty(email))
            {
                email = User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            }

            // Return the username and email
            return Ok(new
            {
                Username = name ?? "Unknown Username",
                Email = email ?? "Unknown Email"
            });
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
        [HttpGet("user-details")]
        public IActionResult GetUserDetails()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Extract all claims
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            // Return the claims along with the standard identity info
            return Ok(new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                AuthenticationType = User.Identity.AuthenticationType,
                UserName = User.Identity.Name ?? "Unknown User",
                Claims = claims
            });
        }

    }


}
