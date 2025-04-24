using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Identities;
using WebApplication3.Services;
using WebApplication3.Users;
using WebApplication3.DTOs;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace WebApplication3.Controllers.Users
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(UserService2 userService) : ControllerBase
    {
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpRequestDto request)
        {
            return CreateActionResult(await userService.SignUp(request));
        }
        private IActionResult CreateActionResult<T>(ApiResponse<T> response)
        {
            if (response.Success)
            {
                return Ok(response.Value); // HTTP 200 - Success
            }
            return BadRequest(response.Message); // HTTP 400 - Failure
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInRequestDto request)
        {
            return CreateActionResult(await userService.SignIn(request));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var response = await userService.ConfirmEmailAsync(userId, token);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Message);
        }


        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await userService.GetCurrentUserAsync(userId);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Value);
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUser(string userId)
        {
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await userService.GetCurrentUserAsync(userId);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }

            return Ok(result.Value);
        }
        [HttpGet("All")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUser([FromQuery] int page = 1, [FromQuery] int pageSize = 8)
        {
            var response = await userService.GetAllUsersAsync(page, pageSize);

            if (!response.Success)
            {
                return NotFound(response);
            }

            return Ok(response);
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto user)
        {
            if (user == null!)
            {
                return BadRequest(new ApiResponse<UpdateUserDto>(false, "Invalid user data", null));
            }

            var response = await userService.UpdateUser(userId, user);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpPut("UpdateMe")]
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto user)
        {
            
            if (user == null)
            {
                return BadRequest(new ApiResponse<UpdateUserDto>(false, "Invalid user data", null));
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await userService.UpdateUser(userId, user);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpDelete("{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var response = await userService.DeleteUserAsync(userId);

            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(new { message = response.Message });
        }
        //UPDATE PASSWORD
        [Authorize(Roles = "user,admin")]
        [HttpPost("change-password")]
        
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }
            if (model == null)
            {
                return BadRequest(new { message = "Invalid pswd" });
            }
            var response = await userService.ChangePasswordAsync(userId, model);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message });
        }

        //forgot password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Invalid email" });
            }
            var response = await userService.ForgotPasswordAsync(email);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message });
        }

        //reset forgotten password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string UserId, string token, string newPassword)
        {
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest(new { message = "Invalid data" });
            }
            var response = await userService.ResetPasswordAsync(UserId, token, newPassword);
            if (!response.Success)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(new { message = response.Message });
        }











    }
}

