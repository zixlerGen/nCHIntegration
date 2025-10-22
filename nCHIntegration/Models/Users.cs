using Microsoft.AspNetCore.Identity;

namespace nCHIntegration.Models
{
    public class Users: IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
    }
}
