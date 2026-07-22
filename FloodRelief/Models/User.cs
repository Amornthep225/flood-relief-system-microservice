using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        [StringLength(10)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } =
            DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<SosRequest> SosRequests { get; set; }
            = new List<SosRequest>();

        public ICollection<Donation> Donations { get; set; }
    = new List<Donation>();

    }
}