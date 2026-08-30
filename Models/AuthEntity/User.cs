using appointmentapi.Models.VaultEntity;

namespace appointmentapi.Models.AuthEntity
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public ICollection<VaultEntry> VaultEntries { get; set; } = new List<VaultEntry>();
    }
}