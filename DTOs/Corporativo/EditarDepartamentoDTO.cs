using System.ComponentModel.DataAnnotations;

namespace appointmentapi.DTOs.Corporativo
{
    public class EditarDepartamentoDTO
    {
        [Required]
        public string NovoDepartamento { get; set; } = string.Empty;
    }
}