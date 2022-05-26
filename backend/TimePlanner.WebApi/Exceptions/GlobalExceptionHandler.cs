using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TimePlanner.WebApi.Exceptions
{
  public class GlobalExceptionHandler
  {
    public static async Task HandleException(HttpContext httpContext)
    {
      string exceptionType = "InternalServerError";
      int status = (int)HttpStatusCode.InternalServerError;
      string message = string.Empty;

      var contextFeature = httpContext.Features.Get<IExceptionHandlerPathFeature>();
      if (contextFeature != null)
      {
        Exception ex = contextFeature.Error;
        exceptionType = ex.GetType().Name;
        message = ex.Message;
        if (ex is ApiException exception)
        {
          status = exception.StatusCode;
        }
      }

      httpContext.Response.ContentType = "application/json";
      httpContext.Response.StatusCode = status;
      await httpContext.Response.WriteAsync(JsonSerializer.Serialize(
        new 
        {
          Exception = exceptionType,
          ErrorMessage = message
        }));
    }
  }
}
