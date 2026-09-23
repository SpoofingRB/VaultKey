using appointmentapi.Data;
using appointmentapi.Models.AuthEntity;
using appointmentapi.Services.Interface;

namespace appointmentapi.Services
{
    public class AuditService : IAuditService
    {
        private readonly JsonDataStore _store;
        private const string ARQUIVO_AUDITORIA = "auditoria.json";

        public AuditService(JsonDataStore store)
        {
            _store = store;
        }

        public async Task RegistrarAsync(int adminId, string adminEmail, string acao, string alvoEmail, int? alvoId = null, string? detalhes = null)
        {
            var logs = await _store.LerAsync<AuditLog>(ARQUIVO_AUDITORIA);
            var novoId = logs.Count > 0 ? logs.Max(l => l.Id) + 1 : 1;

            logs.Add(new AuditLog
            {
                Id = novoId,
                AdminId = adminId,
                AdminEmail = adminEmail,
                Acao = acao,
                AlvoEmail = alvoEmail,
                AlvoId = alvoId,
                Detalhes = detalhes,
                DataHora = DateTime.UtcNow
            });

            await _store.SalvarAsync(ARQUIVO_AUDITORIA, logs);
        }

        public async Task<List<AuditLog>> ListarAsync()
        {
            var logs = await _store.LerAsync<AuditLog>(ARQUIVO_AUDITORIA);
            return logs.OrderByDescending(l => l.DataHora).ToList();
        }
    }
}