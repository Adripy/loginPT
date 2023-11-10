using APIuser.Models;

namespace APIuser.Repositorio
{
    public interface IUsuarioRepositorio: IRepositorio<Usuario>
    {
        Task<Usuario> Actualizar(Usuario usuario);
    }
}
