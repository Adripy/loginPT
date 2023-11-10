namespace Webuser.Models.Dto
{
    public class UsuarioTreeDto
    {
        public string Correo { get; set; }

        public string Nombre { get; set; }

        public string Apellidos { get; set; }

        public DateTime FechaNacimiento { get; set; }
        public List<UsuarioTreeDto>? Subordinados { get; set; }

        public bool esCumple { get; set; }
    }
}
