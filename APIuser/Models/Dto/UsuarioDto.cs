using System.ComponentModel.DataAnnotations;

namespace APIuser.Models.Dto
{
    public class UsuarioDto
    {
        [Required]
        public string Correo { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Apellidos { get; set; }
        [Required]
        public DateTime FechaNacimiento { get; set; }
        public string? Responsable { get; set; }
    }
}
