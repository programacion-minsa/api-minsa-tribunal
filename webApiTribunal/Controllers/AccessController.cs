using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webApiTribunal.Models.Forms;
using webApiTribunal.Models.Responses;
using webApiTribunal.Repositories.Interfaces;

namespace webApiTribunal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController(IAccessService AccessService) : ControllerBase
    {
        [HttpGet("Index")]
        public IActionResult Get()
        {
            return Ok("web api tribunal - minsa");
        }

        [HttpPost("CreateAccessToken")]
        public async Task<IActionResult> CreateAccessToken(AppModel appModel)
        {
            ResponseModel<AppResponseModel> response =  await AccessService.CreateAppAccessToken(appModel);
            
            if (!response.Success)
                return BadRequest(response);
            
            return Ok(response);
        }
    }
}