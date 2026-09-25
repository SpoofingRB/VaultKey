using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using appointmentapi.DTOs.Auth;
using appointmentapi.Models.AuthEntity;
using appointmentapi.Services.Interface;

namespace appointmentapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("seed-admin")]
        public async Task<IActionResult> SeedAdmin(RegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Senha.Length < 8)
                return BadRequest("E-mail obrigatório e senha com pelo menos 8 caracteres.");

            var usuario = await _authService.SeedAdminAsync(dto);
            if (usuario == null)
                return Conflict("Já existe um administrador cadastrado. Este endpoint está desativado.");

            await FazerLoginAsync(usuario);
            return Ok(new AuthResponseDTO { Email = usuario.Email, Role = usuario.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var (usuario, erro) = await _authService.ValidarLoginAsync(dto);
            if (usuario == null)
                return Unauthorized(erro ?? "E-mail ou senha incorretos.");

            await FazerLoginAsync(usuario);
            return Ok(new AuthResponseDTO { Email = usuario.Email, Role = usuario.Role });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpPost("redefinir-senha")]
        [AllowAnonymous]
        public async Task<IActionResult> RedefinirSenha(RedefinirSenhaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Cpf) || dto.NovaSenha.Length < 8)
                return BadRequest("CPF obrigatório e nova senha com pelo menos 8 caracteres.");

            var sucesso = await _authService.RedefinirSenhaAsync(dto.Cpf, dto.NovaSenha);
            if (!sucesso)
                return BadRequest("Não foi possível redefinir a senha. Confira o CPF informado.");

            return Ok(new { mensagem = "Senha redefinida com sucesso." });
        }

        private async Task FazerLoginAsync(User usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}