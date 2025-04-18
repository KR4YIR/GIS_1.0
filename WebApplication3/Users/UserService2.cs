using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApplication3.DTOs;
using WebApplication3.EmailSender;
using WebApplication3.Entities;
using WebApplication3.Identities;
using WebApplication3.Models;
using WebApplication3.Token;

namespace WebApplication3.Users
{
    public class UserService2(UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IOptions<CustomTokenOptions> tokenOptions,
        IEmailSender _emailSender
        ) : IUserService2
    {
        //burayabak
        

        // signUp
        public async Task<ApiResponse<Guid>> SignUp(SignUpRequestDto request)
        {
            var user = new AppUser
            {
                Name = request.Name,
                Surname = request.Lastname,
                UserName = request.UserName,
                Email = request.Email,
                ConfirmationTokenCreatedDate = DateTime.UtcNow,
            };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                return new ApiResponse<Guid>(false, errors, Guid.Empty);
            }
            
            //adding new user a default role
            var userRoleExists = await roleManager.RoleExistsAsync("user");
            if (!userRoleExists)
            {
                var roleResult = await roleManager.CreateAsync(new AppRole { Name = "user" });
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(x => x.Description));
                    return new ApiResponse<Guid>(false, $"Role creation failed: {roleErrors}", Guid.Empty);
                }
            }

            // Assign the 'user' role to the new user
            var addToRoleResult = await userManager.AddToRoleAsync(user, "user");
            if (!addToRoleResult.Succeeded)
            {
                var roleAssignmentErrors = string.Join(", ", addToRoleResult.Errors.Select(x => x.Description));
                return new ApiResponse<Guid>(false, $"Role assignment failed: {roleAssignmentErrors}", Guid.Empty);
            }

            //Email confirmation token
            
            var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedEmailToken = System.Net.WebUtility.UrlEncode(emailToken);
            var emailConfirmationLink = $"http://localhost:5017/api/Users/confirm-email?userId={user.Id}&token={encodedEmailToken}";
            Console.WriteLine($"Email confirmation link: {emailConfirmationLink}");
            if (_emailSender == null)
            {
                throw new Exception("EmailSender is null");
            }

            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm Your Email",
                $"Please confirm your email by clicking the link: <a href='{emailConfirmationLink}'>Click here</a>"
            );
            // Return success 
            return new ApiResponse<Guid>(true, "Operation succeeded.", user.Id);
        }

        //signIn
        public async Task<ApiResponse<TokenResponseDto>> SignIn(SignInRequestDto request)
        {

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return new ApiResponse<TokenResponseDto>(false, "invalid username or password", null!);
            }

            var result = await userManager.CheckPasswordAsync(user, request.Password);
            if (!result)
            {
                return new ApiResponse<TokenResponseDto>(false, "invalid username or password", null!);
            }
            if (user.ConfirmationTokenCreatedDate.HasValue && DateTime.UtcNow > user.ConfirmationTokenCreatedDate.Value.AddHours(3))
            {
                // Token expired
                var newEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedEmailToken = System.Net.WebUtility.UrlEncode(newEmailToken);
                var emailConfirmationLink = $"http://localhost:5017/api/Users/confirm-email?userId={user.Id}&token={encodedEmailToken}";

                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Resend: Confirm Your Email",
                    $"Your previous token has expired. Please confirm your email using this link: <a href='{emailConfirmationLink}'>Click here</a>"
                );

                user.ConfirmationTokenCreatedDate = DateTime.UtcNow; // Update new token timestamp
                await userManager.UpdateAsync(user);
                return new ApiResponse<TokenResponseDto>(false, "Confirm Your Email", null!);

            }
            if (!user.EmailConfirmed)
            {
                return new ApiResponse<TokenResponseDto>(false, "Confirm Your Email", null!);

            }

            //userid
            //username
            //roller
            //userclaim =>
            //role claim => permission

            var userClaimList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            tokenOptions.Value.Audience.ToList().ForEach(x =>
            {
                userClaimList.Add(new Claim(JwtRegisteredClaimNames.Aud, x));
            });
            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                userClaimList.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var userClaims = await userManager.GetClaimsAsync(user);

            foreach (var userClaim in userClaims)
            {
                userClaimList.Add(new Claim(userClaim.Type, userClaim.Value));
            }


            foreach (var roleName in userRoles)
            {
                var role = await roleManager.FindByNameAsync(roleName);


                if (role is null)
                {
                    continue;
                }
                var roleClaim = await roleManager.GetClaimsAsync(role);

                foreach (var roleAsClaim in roleClaim)
                {
                    userClaimList.Add(roleAsClaim);
                }

            }
            var tokenExpire = DateTime.UtcNow.AddHours(tokenOptions.Value.ExpireByHour);
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.Value.Signature));
            var jwtToken = new JwtSecurityToken(
                claims: userClaimList,
                expires: tokenExpire,
                issuer: tokenOptions.Value.Issuer,

                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature));
            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtToken);
            return new ApiResponse<TokenResponseDto>(true, "success", new TokenResponseDto(token));
        }
        //confirm-email
        public async Task<ApiResponse<bool>> ConfirmEmailAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<bool>(false, "Invalid user.", false);
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ApiResponse<bool>(false, $"Email confirmation failed: {errors}", false);
            }

            return new ApiResponse<bool>(true, "Email confirmed successfully.", true);
        }
        
        //user
        public async Task<ApiResponse<UserProfileDto>> GetCurrentUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<UserProfileDto>(false, "User not found", null!);
            }

            var roles = await userManager.GetRolesAsync(user);

            var userProfile = new UserProfileDto
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Surname = user.Surname,
                Username = user.UserName!,
                Email = user.Email!
            };

            return new ApiResponse<UserProfileDto>(true, "Success", userProfile);
        }
        public async Task<ApiResponse<PagedResult<UserProfileDto>>> GetAllUsersAsync(int page, int pageSize)
        {
            var query = userManager.Users;

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<PagedResult<UserProfileDto>>(false, "No users found", new PagedResult<UserProfileDto>
                {
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize,
                    Data = new List<UserProfileDto>()
                });
            }

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userProfiles = users.Select(user => new UserProfileDto
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Surname = user.Surname,
                Username = user.UserName!,
                Email = user.Email!
            }).ToList();

            var result = new PagedResult<UserProfileDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = userProfiles
            };

            return new ApiResponse<PagedResult<UserProfileDto>>(true, "Success", result);
        }

        //update user
        public async Task<ApiResponse<UpdateUserDto>> UpdateUser(string userId, UpdateUserDto user)
        {
            var existingUser = await userManager.FindByIdAsync(userId);
            if (existingUser == null)
            {
              return new ApiResponse<UpdateUserDto> (false,"User not found" ,null!);
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(user.Name))
            {
                existingUser.Name = user.Name;
            }
            if (!string.IsNullOrEmpty(user.Surname))
            {
                existingUser.Surname = user.Surname;
            }
            if (!string.IsNullOrEmpty(user.Username))
            {
                existingUser.UserName = user.Username;
            }
            if (!string.IsNullOrEmpty(user.Email))
            {
                existingUser.Email = user.Email;
            }

            // Add further field updates here...
            await userManager.UpdateAsync(existingUser);

            return new ApiResponse<UpdateUserDto>
            (
                true,
                "User updated successfully",
                user
            );
        }
        public async Task<ApiResponse<bool>> DeleteUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<bool>(false, "User not found", false);
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ApiResponse<bool>(false, $"Failed to delete user: {errors}", false);
            }

            return new ApiResponse<bool>(true, "User deleted successfully", true);
        }
        //CHANGE PASSWORD
        public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ApiResponse<bool>(false, "User not found.",false);
            }

            var passwordCheck = await userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordCheck)
            {
                return new ApiResponse<bool>(false, "Incorrect current password.", false);
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new ApiResponse<bool>(false, $"Password update failed: {errors}",false);
            }

            return new ApiResponse<bool>(true, "Password updated successfully.", true);
        }

    }
}
