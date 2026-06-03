using System.Collections.Generic;
using System.Net;

namespace YourAppName.Core.Bases
{
    public class Response<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public object? Meta { get; set; }

        public bool Succeeded { get; set; }
        public string? Message { get; set; }

        public List<string> Errors { get; set; } = new List<string>();
        public T? Data { get; set; }

        public Response() { }

        // Made 'message' explicitly nullable (string?)
        public Response(T data, string? message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
            StatusCode = HttpStatusCode.OK; // Default to 200 OK
        }

        public Response(string message)
        {
            Succeeded = false;
            Message = message;
            StatusCode = HttpStatusCode.BadRequest; // Default to 400 Bad Request
        }

        public Response(string message, bool succeeded)
        {
            Succeeded = succeeded;
            Message = message;
            // Smartly assign status based on the boolean
            StatusCode = succeeded ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        }
    }
}