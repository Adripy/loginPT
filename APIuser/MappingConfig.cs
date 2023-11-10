using APIuser.Models;
using APIuser.Models.Dto;
using AutoMapper;

namespace APIuser
{
    public class MappingConfig: Profile
    {
        public MappingConfig() 
        {
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Acceso, AccesoDto>().ReverseMap();
        }
    }
}
