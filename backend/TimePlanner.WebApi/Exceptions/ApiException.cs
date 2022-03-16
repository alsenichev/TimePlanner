using System.Net;

namespace TimePlanner.WebApi.Exceptions
{
  public class ApiException : ApplicationException
  {
    public ApiException(string message, HttpStatusCode statusCode) : base(message)
    {
      StatusCode = (int)statusCode;
    }

    public int StatusCode { get; }
  }
}
