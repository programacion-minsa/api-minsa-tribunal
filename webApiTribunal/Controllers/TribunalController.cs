using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using webApiTribunal.Models.Responses;
using webApiTribunal.Repositories.Interfaces;

namespace webApiTribunal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TribunalController : ControllerBase
    {
        private readonly ITribunalService _tribunalService;
        private readonly IAccessService _accessService;
        private readonly string _filesPath;

        public TribunalController(ITribunalService tribunalService, IOptions<FileSettings> settings, IAccessService accessService)
        {
            _tribunalService = tribunalService;
            _accessService = accessService;
            _filesPath = settings.Value.FilesPath;
        }

        [HttpGet("Index")]
        public IActionResult Get()
        {
            return Ok("web api tribunal - minsa");
        }

        [EnableRateLimiting("FixedPolicy")]
        [HttpGet("findbyid")]
        public async Task<IActionResult> FindById([FromQuery] string id, [FromHeader(Name = "X-Api-Key")] string apiKey)
        {
            if (String.IsNullOrEmpty(id))
                return BadRequest("error: debe enviar la cédula");

            var response = await _tribunalService.GetPatientById(id, apiKey);
            await _tribunalService.StoreUserPetitionData(id, response.Success, response.Message);

            if (!response.Success)
                return StatusCode(response.StatusCode, response.Message);
            
            var validate = await _accessService.ValidateToDownloadImages(apiKey);

            if (validate.Success && validate.Data)
            {
                var publicResponse = new
                {
                    persona = response.Data?.Person,
                    mensaje = response.Data?.Message,
                    images = response.Data?.CedulaImagenes,
                };
                return Ok(publicResponse);
            }
            else
            {
                var publicResponse = new
                {
                    persona = response.Data?.Person,
                    mensaje = response.Data?.Message,
                };
                return Ok(publicResponse);
            }
        }

        [EnableRateLimiting("FixedPolicy")]
        [HttpGet("downloadcedulaimage")]
        public async Task<IActionResult> DownloadCedulaImage(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return BadRequest("error: debe enviar la imagen");

            var filePath = Path.Combine(_filesPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"El archivo '{fileName}' no fue encontrado.");

            var contentType = Path.GetExtension(fileName).ToLower() switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
        }
    }
}