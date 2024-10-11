using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UberSystem.Domain.Request;
using UberSystem.Service;

namespace UberSystem.Api.Customer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        private double ExtractAmountFromResult(string result)
        {
            // Sử dụng Regex để tìm giá trị số từ chuỗi
            var match = Regex.Match(result, @"\d+(\.\d+)?");

            if (match.Success)
            {
                return double.Parse(match.Value);
            }
            return -1;
        }

        /// <summary>
        /// Booking new trip
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpPost("book-new-trip")]
        public async Task<IActionResult> Booking(BookingRequest request, long? tripID, float? amount, string? method, string? status)
        {
            try
            {
                string result = null;
                if (string.IsNullOrEmpty(amount.ToString()) && string.IsNullOrEmpty(tripID.ToString()) && string.IsNullOrEmpty(method) && string.IsNullOrEmpty(status))
                {
                    result = await _customerService.Booking(request, null, null, null, null);
                }
                else if (string.IsNullOrEmpty(amount.ToString()) && !string.IsNullOrEmpty(tripID.ToString()) && string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(status))
                {
                    result = await _customerService.Booking(request, tripID, null, null, status);
                }
                else if(!string.IsNullOrEmpty(amount.ToString()) && !string.IsNullOrEmpty(tripID.ToString()) && !string.IsNullOrEmpty(method) && string.IsNullOrEmpty(status))
                {
                    result = await _customerService.Booking(request, tripID, amount, method, null);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Review trip
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpGet("preview-trip")]
        public async Task<IActionResult> GetStatusTrip([FromQuery] long tripId, [FromQuery] long customerId ,[FromQuery] int? rating, [FromQuery] string? feedback)
        {
            try
            {
                string result = await _customerService.GetStatusTrip(tripId, customerId ,rating, feedback);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
