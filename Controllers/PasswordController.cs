using Microsoft.AspNetCore.Mvc;
using appointmentapi.DTOs;
using appointmentapi.Services.Interface;

namespace appointmentapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _service;
        public PasswordController(IPasswordService service) => _service = service;

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(PasswordCheckDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Password))
                return BadRequest("Senha não pode ser vazia.");

            var resultado = await _service.AnalisarAsync(dto);
            return Ok(resultado);
        }
    }
}