using appointmentapi.Models.AuthEntity;

namespace appointmentapi.Models.VaultEntity
{
    public class VaultEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public string NomeServico { get; set; } = string.Empty;
        public string UsuarioServico { get; set; } = string.Empty;

        public string SenhaCriptografada { get; set; } = string.Empty;
        public string Iv { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
    }
}