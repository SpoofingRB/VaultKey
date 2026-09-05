using appointmentapi.Models.AuthEntity;

namespace appointmentapi.Models.CorporativoEntity
{
    public class ContaCorporativa
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Sobrenome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string EmailCorporativo { get; set; } = string.Empty;

        public string SenhaCriptografada { get; set; } = string.Empty;
        public string SenhaIv { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
    }
}