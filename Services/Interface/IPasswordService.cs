using appointmentapi.DTOs;
using appointmentapi.Models.PasswordEntity;

namespace appointmentapi.Services.Interface
{
    public interface IPasswordService
    {
        Task<PasswordAnalysisResult> AnalisarAsync(PasswordCheckDTO dto);
    }
}