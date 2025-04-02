using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Authentication_Service.Model
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}
