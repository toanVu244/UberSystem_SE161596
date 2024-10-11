using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Service;

namespace UberSystem.Api.Driver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        /// <summary>
        /// Get notification and Answer order
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpGet("get-notification")]
        public async Task<IActionResult> GetNortification([FromQuery] long driverID, [FromQuery] long? tripId, [FromQuery] string? response) 
        {
            try
            {
                string nortification = null;
                if (string.IsNullOrWhiteSpace(response) && string.IsNullOrEmpty(tripId.ToString()))
                {
                    nortification = await _driverService.NortificationBookingforDriver(driverID, null, null);
                }
                else if (!string.IsNullOrWhiteSpace(response) && !string.IsNullOrEmpty(tripId.ToString()))
                {
                    nortification = await _driverService.NortificationBookingforDriver(driverID, tripId, response);
                }
                return Ok(nortification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Start tracking GPS
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpPost("start-service")]
        public async Task<IActionResult> StartService(long driverID)
        {
            
            try
            {
                return Ok(await _driverService.UpdateLocation(driverID));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }  
}
