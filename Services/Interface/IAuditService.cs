using appointmentapi.Models.AuthEntity;

namespace appointmentapi.Services.Interface
{
    public interface IAuditService
    {
        Task RegistrarAsync(int adminId, string adminEmail, string acao, string alvoEmail, int? alvoId = null, string? detalhes = null);
        Task<List<AuditLog>> ListarAsync();
    }
}