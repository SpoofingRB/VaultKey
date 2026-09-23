namespace appointmentapi.Models.AuthEntity
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty;      // "ResetSenha", "ExcluirConta", "Ativar", "Desativar"
        public string AlvoEmail { get; set; } = string.Empty;
        public int? AlvoId { get; set; }
        public string? Detalhes { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
    }
}