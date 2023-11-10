using APIuser.Datos;
using APIuser.Models;

namespace APIuser.Repositorio
{
    public class UsuarioRepositorio : Repositorio<Usuario>, IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _db;
        public UsuarioRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Usuario> Actualizar(Usuario usuario)
        {
            //meter parametros a actualizar
            _db.Usuarios.Update(usuario);
            await _db.SaveChangesAsync();
            return usuario;
        }
    }
}
