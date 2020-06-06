using Microsoft.AspNetCore.Identity;

namespace TestSingalR.Entity
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; }
    }
}
