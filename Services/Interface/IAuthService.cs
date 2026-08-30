using appointmentapi.DTOs.Auth;

namespace appointmentapi.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> RegistrarAsync(RegisterDTO dto);
        Task<AuthResponseDTO?> LoginAsync(LoginDTO dto);
    }
}