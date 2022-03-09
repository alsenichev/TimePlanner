using System.Net;

namespace TimePlanner.WebApi.Services
{
  public class StatusException : HttpRequestException
  {
    public StatusException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, null, statusCode)
    {
    }
  }
}
