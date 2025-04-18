using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication3.Token
{

    public interface ITokenService
    {
        Task<ApiResponse<TokenResponseDto>> CreateClientAccessToken(GetAccessTokenRequestDto request);
    }

    public class TokenService(IOptions<CustomTokenOptions> tokenOptions,IOptions<Clients> clients) : ITokenService
    {
        public Task<ApiResponse<TokenResponseDto>> CreateClientAccessToken(GetAccessTokenRequestDto request)
        {
            
            if (clients?.Value?.Items == null || !clients.Value.Items.Any(x => x.Id == request.ClientId && x.Secret == request.ClientSecret))
            {
                
                return Task.FromResult(
                    new ApiResponse<TokenResponseDto>(false, "client not found", null));
            }


            var claims = new List<Claim>()
            {
                new Claim("clientId", request.ClientId),
                
            };
            tokenOptions.Value.Audience.ToList().ForEach(x =>
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud,x));
            });
            
            var tokenExpire = DateTime.Now.AddHours(tokenOptions.Value.ExpireByHour);
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.Value.Signature));
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                expires: tokenExpire,
                issuer: tokenOptions.Value.Issuer,
                
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature));
            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtToken);
            return Task.FromResult(
                new ApiResponse<TokenResponseDto>(true, "success", new TokenResponseDto(token)));


        }
    }
}
