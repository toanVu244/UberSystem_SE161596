using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces.Services;
using UberSystem.Domain.Models;
using UberSystem.Domain.Request;

namespace UberSystem.Api.Authentication.Controllers
{
    [ApiController]
    [Route("odata/[controller]")]
    public class UserController : ODataController
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Get user with OData query
        /// </summary>
        /// <response code="200">Return successfully</response>
        [EnableQuery]
        [HttpGet]
        [Authorize(Policy = "CustomerPolicy")]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            try
            {
                var users = await userService.GetAll();
                return Ok(users.AsQueryable());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        /// <summary>
        /// Get user by email with OData query
        /// </summary>
        /// <response code="200">Return successfully</response>
        [EnableQuery]
        [HttpGet("odata/User({key})")]
        public async Task<ActionResult<User>> GetUser([FromRoute] int key)
        {
            if (key == null)
            {
                return BadRequest("Key is not null!!!");
            }
            var userList = await userService.GetAll();
            var user = userList.SingleOrDefault(d => d.Id == key);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AddUserRequest userRequest, [FromQuery] string? code)
        {
            if (userRequest == null)
            {
                return BadRequest("Invalid user data.");
            }
            try
            {
                return Created(await userService.Add(userRequest, code));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromQuery] int key, [FromBody]UserRequest userRequest)
        {
            if (userRequest == null)
            {
                return BadRequest("Invalid user data.");
            }
            try
            {
                await userService.Update(userRequest);
                return Ok("User updated successfully!!!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromQuery]int key)
        {
            try
            {
                await userService.Delete(key);
                return Ok("User deleted successfully!!!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
