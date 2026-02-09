using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using webApiTribunal.Repositories.Interfaces;

namespace webApiTribunal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TribunalController : ControllerBase
    {
        private readonly ITribunalService _tribunalService;

        public TribunalController(ITribunalService tribunalService)
        {
            _tribunalService = tribunalService;
        }

        [HttpGet("Index")]
        public IActionResult Get()
        {
            return Ok("web api tribunal - minsa");
        }

        [EnableRateLimiting("FixedPolicy")]
        [HttpGet("findbyid")]
        public async Task<IActionResult> FindById(string id)
        {
            if (String.IsNullOrEmpty(id))
                return BadRequest("error: debe enviar la cédula");

            var response = await _tribunalService.GetPatientById(id);
            await _tribunalService.StoreUserPetitionData(id, response.Success, response.Message);

            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);

            var publicResponse = new
            {
                persona = response.Data?.Person,
                mensaje = response.Data?.Message,
            };

            return Ok(publicResponse);
        }
    }
}