using Microsoft.AspNetCore.Mvc;

namespace TimePlanner.WebApi.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class StatusController : ControllerBase
  {

    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger)
    {
      _logger = logger;
    }

    [HttpGet]
    public ActionResult<string> Get()
    {
      return Ok("TimePlanner: version 0.0.1");
    }
  }
}