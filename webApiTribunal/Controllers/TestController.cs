using Microsoft.AspNetCore.Mvc;

namespace webApiTribunal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("Index")]
        public IActionResult Get()
        {
            return Ok("web api tribunal - minsa");
        }
    }
}