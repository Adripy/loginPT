using Webuser.Models;
using Webuser.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Webuser.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Webuser.Controllers
{
    public class WebController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IServiceApi serviceApi;

		public WebController(IConfiguration configuration, IServiceApi serviceApi) 
        { 
            this.configuration = configuration;
            this.serviceApi = serviceApi;
        }
        
        public IActionResult Login()
        {
			return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AccesoDto acceso)
        {
            ResCredencial resCredencial = new ResCredencial();

            resCredencial = await serviceApi.LoginApi(acceso);

            if (resCredencial == null)
            {
                ViewData["Mensaje"] = "usuario no encontrado";
                return View();
            }

            var usuario = await Authorize(resCredencial);

            if (usuario != null)
            {
                return View("Usuario", usuario);
            }
            else
            {
                ViewData["Mensaje"] = "usuario no encontrado";
                return View();
            }
        }


		[Authorize]
		public IActionResult Usuario(Usuario usuario)
		{
			return View("Usuario", usuario);
		}

		[Authorize]
		public async Task<IActionResult> Jerarquia(Usuario usuario)
		{
			var usuarioTree = await UsuarioTree(usuario);

			return View("Tree", usuarioTree);
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return View("Login");
		}

		private async Task<Usuario?> Authorize(ResCredencial credencial)
        {
            string token = credencial.Token;

            if (String.IsNullOrEmpty(token))
                return null;

            var usuario = await ValidateToken(token);

            if (usuario == null) 
                return null;

            return usuario;
        }

        private async Task<Usuario?> ValidateToken(string token) 
        {
            var keyToken = configuration["keyToken"];
            var keyBytes = Encoding.UTF8.GetBytes(keyToken);
            var key = new SymmetricSecurityKey(keyBytes);

			var validations = new TokenValidationParameters
			{
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = key
			};
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var claimsPrincipal = tokenHandler.ValidateToken(token, validations, out SecurityToken validatedToken);

                if(claimsPrincipal == null)
                    return null;

                string emailReturn = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ?? "";
				string rolReturn = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? "";

				var usuario = await serviceApi.GetUserForEmail(emailReturn);
                if (usuario != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.Nombre),
                        new Claim("Correo", usuario.Correo),
						new Claim(ClaimTypes.Role, rolReturn)
					};

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                }
				return usuario;
			}
            catch 
            { 
                return null;
            }
        }

        public async Task<UsuarioTreeDto> UsuarioTree(Usuario usuario)
        {
            DateTime fechaNacimiento = usuario.FechaNacimiento;

            bool esCumple = CumpleEstaSemana(fechaNacimiento);

            var subordinados = await ObtenerSubordinados(usuario.Correo);

            // Crea el modelo UsuarioTreeDto
            UsuarioTreeDto usuarioTree = new UsuarioTreeDto
            {
                Correo = usuario.Correo,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                FechaNacimiento = usuario.FechaNacimiento,
                esCumple = esCumple,
                Subordinados = subordinados
            };

            return usuarioTree;
        }

        private bool CumpleEstaSemana(DateTime fechaNacimiento)
        {
            DateTime fechaActual = DateTime.Now;

            CultureInfo cultura = CultureInfo.CurrentCulture;

            int semanaActual = cultura.Calendar.GetWeekOfYear(fechaActual, cultura.DateTimeFormat.CalendarWeekRule, cultura.DateTimeFormat.FirstDayOfWeek);
            int semanaCumple = cultura.Calendar.GetWeekOfYear(fechaNacimiento, cultura.DateTimeFormat.CalendarWeekRule, cultura.DateTimeFormat.FirstDayOfWeek);

            return semanaActual == semanaCumple;
        }

        private async Task<List<UsuarioTreeDto>> ObtenerSubordinados(string correo)
        {
            var usuarios = await serviceApi.GetListEmployees(correo);
            var usariosTree = new List<UsuarioTreeDto>();

            foreach (var usuario in usuarios) 
            {
                usariosTree.Add(await UsuarioTree(usuario));
            }
            return usariosTree;
        }
    }
}
