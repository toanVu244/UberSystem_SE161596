using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Net;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Models;
using UberSystem.Domain.Request;
using UberSystem.Api.Authentication.Controllers;
//using UberSystem.Api.Driver.Controllers;

namespace UberSystem.Api.Authentication.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }


        /// <summary>
        /// Verifies the user's email based on the provied token
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpPost("Sign Up")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest userRequest, [FromQuery] string? code)
        {
            if (userRequest == null)
            {
                return BadRequest("Invalid user data.");
            }
            try
            {
                return Ok(await userService.Add(userRequest, code));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// Get user based on email
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser([FromQuery] string email)
        {

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is not null!!!");
            }
            var user = await userService.FindByEmail(email);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound("Email is not exist!!!");
            }

        }

        /// <summary>
        /// Delete user by user id
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] long id)
        {
            try
            {
                await userService.Delete(id);
                return Ok("Delete user is successs!!!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update user by user id
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserRequest userRequest)
        {
            try
            {
                await userService.Update(userRequest);
                return Ok("Update user is success!!!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }


        /// <summary>
        /// User login based on username and password
        /// </summary>
        /// <response code="200">Return successfully</response>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            //try
            //{
            //    return Ok(await userService.Login(loginRequest));
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, $"Internal server error: {ex.Message}");
            //}
            try
            {
                var loginResult = await userService.Login(loginRequest);
                if (loginResult != null)
                {
                    if (loginResult.Role == "Driver")
                    {
                        var userId = loginResult.UserId;

                        var driverService = HttpContext.RequestServices.GetService<DriverDataSenderService>();
                        driverService?.ReceiveUserId(userId);
                    }
                    return Ok(loginResult);
                }

                return Unauthorized("Invalid credentials");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenModel tokenModel)
        {
            try
            {
                return Ok(await userService.RenewToken(tokenModel));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
