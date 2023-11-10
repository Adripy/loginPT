using System.Linq.Expressions;

namespace APIuser.Repositorio
{
    public interface IRepositorio<T> where T : class
    {
        Task Insertar(T entidad);
        Task<T> Obtener(Expression<Func<T, bool>>? filtro = null);
        Task<List<T>> ObtenerTodos(Expression<Func<T,bool>>? filtro = null);
        Task Eliminar(T entidad);
        Task Grabar();
    }
}
