using APIuser.Models;

namespace APIuser.Repositorio
{
    public interface IAccesoRepositorio : IRepositorio<Acceso>
    {
        Task<Acceso> Actualizar(Acceso acceso);
    }
}
