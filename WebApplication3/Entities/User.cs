using WebApplication3.Entities.Common;

namespace WebApplication3.Entities
{
    public class User : BaseEntity
    {
    public string? FirstName { get; set; }
    public string? Surname { get; set; }
    public string Username { get; set; }
    public string? Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    }
}
