using Microsoft.AspNetCore.Mvc;
using WebApplication3.Token;

namespace WebApplication3.Controllers.Token
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(ITokenService tokenService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateClientToken(GetAccessTokenRequestDto request)
        {
            var response = await tokenService.CreateClientAccessToken(request);
            return CreateActionResult(response);
        }

        private IActionResult CreateActionResult(ApiResponse<TokenResponseDto> response)
        {
            if (response.Success)
            {
                return Ok(response.Value);
                
            }
            return BadRequest(response.Message);
        }
    }
}
