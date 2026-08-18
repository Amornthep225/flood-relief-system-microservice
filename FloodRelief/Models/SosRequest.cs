using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FloodRelief.Constants;

namespace FloodRelief.Models
{
    [Table("sos_requests")]
    public class SosRequest
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(5)]
        public string? CenterId { get; set; }

        [StringLength(5)]
        public string? AssignedStaffId { get; set; }
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [Required]
        [StringLength(500)]
        public string AddressDetail { get; set; } = string.Empty;

        [StringLength(20)]
        public string RequestType { get; set; } = "Relief";

        [StringLength(50)]
        public string? EmergencyType { get; set; }

        public int VictimCount { get; set; } = 1;

        public int ChildCount { get; set; }

        public int ElderlyCount { get; set; }

        public int DisabledCount { get; set; }

        public int PatientCount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? WaterLevel { get; set; }

        [StringLength(1000)]
        public string? EmergencyDetail { get; set; }

        [StringLength(20)]
        public string Priority { get; set; } = "Normal";

        [StringLength(30)]
        public string Status { get; set; } = SosRequestStatuses.Pending;

        [StringLength(500)]
        public string? UserRemark { get; set; }

        [StringLength(500)]
        public string? StaffRemark { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? AcceptedAt { get; set; }

        public DateTime? PreparingAt { get; set; }

        public DateTime? DeliveringAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(CenterId))]
        public Center? Center { get; set; }

        [ForeignKey(nameof(AssignedStaffId))]
        public Staff? AssignedStaff { get; set; }

        public ICollection<SosRequestItem> Items { get; set; }
            = new List<SosRequestItem>();

        public ICollection<DonationAllocation> DonationAllocations { get; set; }
            = new List<DonationAllocation>();
    }
}
