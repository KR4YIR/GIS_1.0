using WebApplication3.DTOs;
using WebApplication3.Token;

namespace WebApplication3.Users
{
    public interface IUserService2
    {
        Task<ApiResponse<Guid>> SignUp(SignUpRequestDto request);
        Task<ApiResponse<TokenResponseDto>> SignIn(SignInRequestDto request);
        Task<ApiResponse<UserProfileDto>> GetCurrentUserAsync(string userId);
        Task<ApiResponse<UpdateUserDto>> UpdateUser(string userId, UpdateUserDto user);
        Task<ApiResponse<bool>> DeleteUserAsync(string userId);
    }
}
