using System.Net;

namespace APIuser.Models
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSucceeded { get; set; } = true;
        public List<string>? ErrorsMessage { get; set; }
        public object? Result { get; set; }
    }
}
