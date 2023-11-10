using System.ComponentModel.DataAnnotations;

namespace APIuser.Models
{
    public class Acceso
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Correo { get; set; }
        [Required]
        public string Clave { get; set; }
    }
}
