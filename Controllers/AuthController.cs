using Microsoft.AspNetCore.Mvc;
using appointmentapi.DTOs.Auth;
using appointmentapi.Services.Interface;

namespace appointmentapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
                return BadRequest("E-mail e senha são obrigatórios.");

            if (dto.Senha.Length < 8)
                return BadRequest("A senha mestra deve ter pelo menos 8 caracteres.");

            var resultado = await _authService.RegistrarAsync(dto);
            if (resultado == null)
                return Conflict("Este e-mail já está cadastrado.");

            return Ok(resultado);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var resultado = await _authService.LoginAsync(dto);
            if (resultado == null)
                return Unauthorized("E-mail ou senha incorretos.");

            return Ok(resultado);
        }
    }
}