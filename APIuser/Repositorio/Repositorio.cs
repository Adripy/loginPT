using APIuser.Datos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace APIuser.Repositorio
{
    public class Repositorio<T> : IRepositorio<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repositorio(ApplicationDbContext db) 
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public async Task<T> Obtener(Expression<Func<T, bool>>? filtro = null)
        {
            IQueryable<T> query = dbSet;

            if (filtro != null)
                query = query.Where(filtro);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> ObtenerTodos(Expression<Func<T, bool>>? filtro = null)
        {
            IQueryable<T> query = dbSet;

            if (filtro != null)
                query = query.Where(filtro);

            return await query.ToListAsync();
        }
        public async Task Grabar()
        {
            await _db.SaveChangesAsync();
        }

        public async Task Insertar(T entidad)
        {
            await dbSet.AddAsync(entidad);
        }

        public async Task Eliminar(T entidad)
        {
            dbSet.Remove(entidad);
            await Grabar();
        }
    }
}
