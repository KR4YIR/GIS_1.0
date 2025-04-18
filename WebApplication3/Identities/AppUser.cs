using Microsoft.AspNetCore.Identity;

namespace WebApplication3.Identities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string Name  { get; set; }
        public string Surname { get; set; }
        public DateTime? ConfirmationTokenCreatedDate { get; set; }

    }
}
