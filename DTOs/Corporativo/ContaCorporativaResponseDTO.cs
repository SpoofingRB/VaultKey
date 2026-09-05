namespace appointmentapi.DTOs.Corporativo
{
    public class ContaCorporativaResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Sobrenome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string EmailCorporativo { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
        public string? SenhaVisivel { get; set; }
    }
}