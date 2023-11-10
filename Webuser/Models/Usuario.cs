using System.ComponentModel.DataAnnotations;

namespace Webuser.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
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
