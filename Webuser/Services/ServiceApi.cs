using Webuser.Models;
using Webuser.Models.Dto;
using Newtonsoft.Json;
using System.Text;
using ToJSON;
using Microsoft.AspNetCore.Hosting.Server;

namespace Webuser.Services
{
    public class ServiceApi : IServiceApi
    {
        private readonly IConfiguration configuration;

        public ServiceApi(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<ResCredencial> LoginApi(AccesoDto acceso)
        {
            if (String.IsNullOrEmpty(acceso.Correo) || String.IsNullOrEmpty(acceso.Clave))
                return null;

            acceso.Clave = EncriptarBase64(acceso.Clave);
            HttpClient httpClient = httpClientHeadersAuth();

            var content = new StringContent(JsonConvert.SerializeObject(acceso), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("/Api/login", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

				var result = apiResponse.Result.ToJSON();

				var credencial = JsonConvert.DeserializeObject<ResCredencial>(result);

				return credencial;
            }

			return null;
        }
        public async Task<List<Usuario>> GetListEmployees(string email)
        {
            HttpClient httpClient = httpClientHeadersAuth();

            HttpResponseMessage response = await httpClient.GetAsync("/Api/employees?email=" + email);

            var usuarios = new List<Usuario>();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                var result = apiResponse.Result.ToJSON();

                usuarios = JsonConvert.DeserializeObject<List<Usuario>>(result);
            }
            return usuarios;
        }

        public async Task<Usuario?> GetUserForEmail(string email)
        {
            HttpClient httpClient = httpClientHeadersAuth();

            HttpResponseMessage response = await httpClient.GetAsync("/Api/user?email=" + email);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

                var result = apiResponse.Result.ToJSON();

                var user = JsonConvert.DeserializeObject<Usuario>(result);

                return user;
            }
            return null;
        }

        HttpClient httpClientHeadersAuth()
        {
            var httpClient = new HttpClient();
            string uri = configuration["UrlApi"];
            httpClient.BaseAddress = new Uri(uri);

            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");

            var credentialApi = configuration["credencialApi"];

            string valBasicAuth = credentialApi + ":" + EncriptarBase64(credentialApi);

            string val = EncriptarBase64(valBasicAuth);

            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + val);
            return httpClient;
        }
        public static string EncriptarBase64(string texto)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(texto));
        }

        public static string DesencriptarBase64(string texto)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(texto));
		}
    }
}
