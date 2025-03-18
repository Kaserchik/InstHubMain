using InstHub.Models;
using Microsoft.AspNetCore.Identity;

namespace InstHub.Data.Identity
{
    public class AppIdentityUser: IdentityUser
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }

        public AppIdentityUser()
        {
            FirstName = "";
            SecondName = "";
        }
    }
}
