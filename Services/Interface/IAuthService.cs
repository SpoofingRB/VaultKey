using appointmentapi.DTOs.Auth;
using appointmentapi.Models.AuthEntity;

namespace appointmentapi.Services.Interface
{
    public interface IAuthService
    {
        Task<User?> SeedAdminAsync(RegisterDTO dto);
        Task<User?> ValidarLoginAsync(LoginDTO dto);
        Task<User> CriarUsuarioFuncionarioAsync(string email, string senha);
    }
}