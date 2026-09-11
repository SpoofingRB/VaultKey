using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using appointmentapi.DTOs.Corporativo;
using appointmentapi.Services.Interface;

namespace appointmentapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorporativoController : ControllerBase
    {
        private readonly ICorporativoService _service;
        public CorporativoController(ICorporativoService service) => _service = service;

        [HttpPost("onboarding")]
        [AllowAnonymous]
        public async Task<IActionResult> Onboarding(OnboardingDTO dto)
        {
            var resultado = await _service.CriarViaOnboardingAsync(dto);
            if (resultado == null)
                return BadRequest("Não foi possível gerar as credenciais. Verifique os dados ou se o CPF já está cadastrado.");

            return Ok(resultado);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Listar()
        {
            var contas = await _service.ListarAsync();
            return Ok(contas);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar(CriarFuncionarioDTO dto)
        {
            var conta = await _service.CriarFuncionarioAsync(dto);
            if (conta == null)
                return BadRequest("Não foi possível criar o funcionário. Verifique o CPF ou se ele já está cadastrado.");

            return Ok(conta);
        }

        [HttpGet("{id}/senha")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevelarSenha(int id)
        {
            var senha = await _service.RevelarSenhaAsync(id);
            if (senha == null) return NotFound();

            return Ok(new { senha });
        }

        [HttpPost("{id}/resetar-senha")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetarSenha(int id)
        {
            var novaSenha = await _service.ResetarSenhaAsync(id);
            if (novaSenha == null) return NotFound();

            return Ok(new { novaSenha });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AlterarStatus(int id, [FromQuery] bool ativo)
        {
            var sucesso = await _service.AlterarStatusAsync(id, ativo);
            if (!sucesso) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Excluir(int id)
        {
            var sucesso = await _service.ExcluirAsync(id);
            if (!sucesso) return NotFound();

            return NoContent();
        }
    }
}