using System.ComponentModel.DataAnnotations;

namespace Webuser.Models.Dto
{
    public class AccesoDto
    {
        [Required]
        public string Correo { get; set; }
        [Required]
        public string Clave { get; set; }
    }
}
