using InstHub.Data.Identity;
using System.ComponentModel.DataAnnotations;

namespace InstHub.Models
{
    public class UserGroup
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppIdentityUser User { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
