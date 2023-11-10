using Webuser.Models;
using Webuser.Models.Dto;

namespace Webuser.Services
{
    public interface IServiceApi
    {
        Task<ResCredencial> LoginApi(AccesoDto acceso);
        Task<Usuario> GetUserForEmail(string email);
        Task<List<Usuario>> GetListEmployees(string email);
    }
}
