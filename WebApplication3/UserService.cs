using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication3.DTOs;
using WebApplication3.Entities;
using WebApplication3.Repositories;

namespace WebApplication3.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        public async Task<ApiResponse<string>> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            
            var existingUser = await _unitOfWork.ReadUsers.GetByUsernameAsync(userRegisterDto.Username);
            if (existingUser != null)
            {
                return new ApiResponse<string>(false, "Bu kullanıcı adı zaten alınmış.", null);
            }

            var user = new User
            {
                FirstName = userRegisterDto.FirstName,
                Surname = userRegisterDto.Surname,
                Email = userRegisterDto.Email,
                Username = userRegisterDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password),
                Role = "User",
                CreatedDate = DateTime.UtcNow

            };

            var added = await _unitOfWork.Users.AddAsync(user);
            if (!added)
            {
                return new ApiResponse<string>(false, "Kullanıcı kaydedilemedi.", null);
            }

            await _unitOfWork.SaveAsync();
            return new ApiResponse<string>(true, "Kullanıcı başarıyla kaydedildi.", null);
        }

        

       
    }
}