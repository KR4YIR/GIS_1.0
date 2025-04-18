using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Entities;
using WebApplication3.DTOs;
using System.Security.Claims;

namespace WebApplication3.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class PointController : ControllerBase
    {
        private readonly IPointService _pointService;

        public PointController(IPointService pointService)
        {
            _pointService = pointService;
        }

        [Authorize(Roles = "user,admin")]
        [HttpGet]//listele
        public async Task<IActionResult> GetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Retrieve UserId from the JWT
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList(); // Retrieve roles from JWT

            if (userRoles.Contains("admin"))
            {
                // Admin retrieves all points
                var response = await _pointService.GetAllPointsAsync();
                return Ok(response);
            }

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }

            // Normal user retrieves only their points
            var userId = Guid.Parse(userIdClaim);
            var response2 = await _pointService.GetPointsByUserIdAsync(userId);
            return Ok(response2);
        }
        [Authorize(Roles = "user,admin")]
        [HttpPost] //ekleme
        public async Task<IActionResult> AddAsync([FromBody] PointDto pointDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }

            var userId = Guid.Parse(userIdClaim);

            var point = new Point
            {
                UserId = userId,
                Name = pointDto.Name,
                Wkt = pointDto.Wkt
            };

            var response = await _pointService.AddAsync(point);
            return Ok(response);
        }
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]//id ile getir
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            var response = await _pointService.GetByIdAsync(id);
            if (response.Success)
            {
                return Ok(response);
            }
            return NotFound(response);
        }
        [Authorize(Roles = "user,admin")]
        [HttpDelete("{id}")] // Delete by ID
        public async Task<IActionResult> RemoveAsync(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (userRoles.Contains("admin"))
            {
                // Admin can delete any point
                var response = await _pointService.RemoveAsync(id);
                if (response.Success)
                {
                    return Ok(response);
                }
                return NotFound(response);
            }

            // For normal users, validate ownership
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }

            var userId = Guid.Parse(userIdClaim);
            var pointOwnerResponse = await _pointService.GetPointOwnerAsync(id);
            if (!pointOwnerResponse.Success || pointOwnerResponse.Value != userId)
            {
                return Forbid("You can only delete your own points.");
            }

            // If ownership is verified, proceed to delete
            var responseForUser = await _pointService.RemoveAsync(id);
            if (responseForUser.Success)
            {
                return Ok(responseForUser);
            }
            return NotFound(responseForUser);
        }
        [Authorize(Roles = "user,admin")]
        [HttpPut("{id}")] // Update by ID
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] PointDto pointDto)
        {
            // Check if pointDto is valid
            if (pointDto == null!)
            {
                return BadRequest("PointDto cannot be null.");
            }

            // Get the user's ID and roles from the claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }

            var userId = Guid.Parse(userIdClaim);

            if (userRoles.Contains("admin"))
            {
                // Admin can update any point
                var point = new Point
                {
                    Name = pointDto.Name,
                    Wkt = pointDto.Wkt
                };
                var adminResponse = await _pointService.UpdateAsync(id, point);
                if (adminResponse.Success)
                {
                    return Ok(adminResponse);
                }
                return NotFound(adminResponse);
            }

            // For regular users, verify ownership before updating
            var pointOwnerResponse = await _pointService.GetPointOwnerAsync(id);
            if (!pointOwnerResponse.Success || pointOwnerResponse.Value != userId)
            {
                return Forbid("You can only update your own points.");
            }

            // Ownership verified; allow update
            var point2 = new Point
            {
                Name = pointDto.Name,
                Wkt = pointDto.Wkt
            };
            var userResponse = await _pointService.UpdateAsync(id, point2);
            if (userResponse.Success)
            {
                return Ok(userResponse);
            }
            return NotFound(userResponse);
        }
    }
}