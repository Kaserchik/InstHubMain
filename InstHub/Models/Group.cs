using System.ComponentModel.DataAnnotations;

namespace InstHub.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string GroupName { get; set; }
    }
}
