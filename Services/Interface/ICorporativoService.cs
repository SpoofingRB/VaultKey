using appointmentapi.DTOs.Corporativo;

namespace appointmentapi.Services.Interface
{
    public interface ICorporativoService
    {
        Task<List<ContaCorporativaResponseDTO>> ListarAsync();
        Task<ContaCorporativaResponseDTO> CriarFuncionarioAsync(CriarFuncionarioDTO dto);
        Task<ContaCorporativaResponseDTO?> CriarViaOnboardingAsync(OnboardingDTO dto);
        Task<string?> RevelarSenhaAsync(int contaId);
        Task<string?> ResetarSenhaAsync(int contaId);
        Task<bool> AlterarStatusAsync(int contaId, bool ativo);
        Task<bool> ExcluirAsync(int contaId);
    }
}