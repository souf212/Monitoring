using Microsoft.AspNetCore.Mvc;
using KtcWeb.Services;
using KtcWeb.Models;

namespace KtcWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ActiveDirectoryService _adService;

        public AuthController(ActiveDirectoryService adService)
        {
            _adService = adService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"Tentative login pour : {request.Username}");

            bool isAuth = _adService.Authenticate(request.Username, request.Password);

            if (!isAuth)
                return Unauthorized("Identifiants invalides");

            var roles = _adService.GetRoles(request.Username, request.Password);

            // Génération du JWT Token
            string token = _adService.GenerateJwtToken(request.Username, roles);

            return Ok(new AuthResponse
            {
                Username = request.Username,
                Roles = roles,
                Token = token,
                Expiration = DateTime.Now.AddHours(2)
            });
        }
    }
}