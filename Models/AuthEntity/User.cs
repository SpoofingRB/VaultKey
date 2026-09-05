namespace appointmentapi.Models.AuthEntity
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Funcionario"; // "Admin" ou "Funcionario"
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}