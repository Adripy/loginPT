using Microsoft.AspNetCore.Mvc;
using APIuser.Models;
using APIuser.Repositorio;
using System.Net;
using APIuser.Models.Dto;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Security.Principal;

namespace APIuser.Controllers
{
    [Route("/")]
    [ApiController]
    public class ApiController : Controller
    {
        private readonly ILogger<ApiController> logger;
        private readonly IUsuarioRepositorio userRepo;
        private readonly IAccesoRepositorio accesRepo;
        private readonly IConfiguration configuracion;
        private readonly IMapper mapper;
        protected ApiResponse response;

        public ApiController(ILogger<ApiController> logger,
                                IUsuarioRepositorio uRepo,
                                IAccesoRepositorio aRepo,
                                IMapper mapper,
                                IConfiguration configuracion)
        {
            this.logger = logger;
            this.userRepo = uRepo;
            this.accesRepo = aRepo;
            this.mapper = mapper;
            this.response = new();
            this.configuracion = configuracion;
        }

        [HttpGet("api/user", Name = "getUserEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ObtenerUsuarioPorEmail([FromQuery] string email) 
        {
            try
            {
                if(!AutorizacionApi(HttpContext))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.ErrorsMessage = new List<string>() { "Cliente no autorizado" };
                    return BadRequest(response);
                }

                if (String.IsNullOrEmpty(email))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.ErrorsMessage = new List<string>() { "Valor id inválido" };
                    return BadRequest(response);
                }

                var usuario = await userRepo.Obtener(x => x.Correo.ToLower() == email.ToLower());

                if (usuario == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSucceeded = false;
                    response.ErrorsMessage = new List<string>() { "Usuario no encontrado" };
                    return NotFound(response);
                }
                response.StatusCode = HttpStatusCode.OK;
                response.Result = mapper.Map<UsuarioDto>(usuario);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError("getUser", ex.Message);
                response.IsSucceeded = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorsMessage = new List<string>() { ex.ToString() };
                return response;
            }
        }

        [HttpGet("api/employees", Name = "getEmployeesEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ObtenerEmpleadosACargo([FromQuery] string email)
        {
            try
            {
                if (!AutorizacionApi(HttpContext))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.ErrorsMessage = new List<string>() { "Cliente no autorizado" };
                    return BadRequest(response);
                }

                if (String.IsNullOrEmpty(email))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.ErrorsMessage = new List<string>() { "Valor correo inválido" };
                    return BadRequest(response);
                }

                var usuarios = await userRepo.ObtenerTodos(x => x.Responsable != null && x.Responsable.ToLower() == email.ToLower());

                response.StatusCode = HttpStatusCode.OK;
                response.Result = mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError("getEmployeesEmail", ex.Message);
                response.IsSucceeded = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorsMessage = new List<string>() { ex.ToString() };
                return response;
            }
        }

        [HttpPost("api/login", Name = "login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] AccesoDto accesoDto)
        {
            try
            {
                if(!AutorizacionApi(HttpContext))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.ErrorsMessage = new List<string>() { "Cliente no autorizado" };
                    return BadRequest(response);
                }

                if (!ModelState.IsValid) 
                    return BadRequest(ModelState);

                if (String.IsNullOrEmpty(accesoDto.Correo) || String.IsNullOrEmpty(accesoDto.Clave))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.ErrorsMessage = new List<string>() { "Parámetros inválidos" };
                    return BadRequest(response);
                }

                var accesoRecuperado = await accesRepo.Obtener(x => x.Correo.ToLower() == accesoDto.Correo.ToLower());

                if (accesoRecuperado == null || accesoRecuperado.Clave != accesoDto.Clave)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.IsSucceeded = false;
                    response.ErrorsMessage = new List<string>() { "Usuario/Password incorrecto" };
                    return NotFound(response);
                }

                ResAcceso resAcceso = new ResAcceso();
                resAcceso.Token = CreateToken(accesoRecuperado);

                response.StatusCode = HttpStatusCode.OK;
                response.Result = resAcceso;
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError("login", ex.Message);
                response.IsSucceeded = false;
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.ErrorsMessage = new List<string>() { ex.ToString() };
                return response;
            }
        }

        private string CreateToken(Acceso acceso)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Email, acceso.Correo),
                new Claim(ClaimTypes.Role, "User"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("keyToken")));

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private bool AutorizacionApi(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var authHeader = request.Headers["Authorization"];

            if (String.IsNullOrEmpty(authHeader))
                return false;

            var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

            if (authHeaderVal.Scheme.Equals("basic",
                        StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
            {
                return AutentificarCliente(authHeaderVal.Parameter);
            }

            return false;
        }

        private bool AutentificarCliente(string credentials)
        {
            credentials = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(credentials));

            int separator = credentials.IndexOf(':');
            string nombre = credentials.Substring(0, separator);
            string password = credentials.Substring(separator + 1);

            return CheckPassword(nombre, password);
        }

        private bool CheckPassword(string nombre, string password)
        {
            var nombreApi = Environment.GetEnvironmentVariable("credencialApi");
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var passwordApi = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(nombreApi));

            return nombre == nombreApi && password == passwordApi;
        }
    }
}
