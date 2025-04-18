using WebApplication3.DTOs;

namespace WebApplication3
{
    public interface IUserService
    {
        
         
            Task<ApiResponse<string>> RegisterUserAsync(UserRegisterDto userRegisterDto);
            
    }
}
