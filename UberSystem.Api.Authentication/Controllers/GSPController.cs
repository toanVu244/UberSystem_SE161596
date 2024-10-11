using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Request;
using UberSystem.Service;

namespace UberSystem.Api.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GSPController : ControllerBase
    {
        private readonly IGSPService _gSPService;

        public GSPController(IGSPService gSPService)
        {
            _gSPService = gSPService;
        }

        [HttpPost]
        public async Task<IActionResult> ImportGSP(IFormFile file)
        {
            if (file == null || file.Length == 0) 
            {
                return BadRequest("Please upload a valid Excel file.");
            }
            try
            {
                await _gSPService.ImportFile(file);
                return Ok("Import file is success!!!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
