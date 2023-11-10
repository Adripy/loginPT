using APIuser.Datos;
using APIuser.Models;

namespace APIuser.Repositorio
{
    public class AccesoRepositorio : Repositorio<Acceso>, IAccesoRepositorio
    {
        private readonly ApplicationDbContext _db;
        public AccesoRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Acceso> Actualizar(Acceso acceso)
        {
            //meter parametros a actualizar
            _db.Accesos.Update(acceso);
            await _db.SaveChangesAsync();
            return acceso;
        }
    }
}
