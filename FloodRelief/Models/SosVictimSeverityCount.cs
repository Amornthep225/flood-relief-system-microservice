using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FloodRelief.Models
{
    [Table("sos_victim_severity_counts")]
    public class SosVictimSeverityCount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string SosRequestId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = string.Empty;

        public int ChildCount { get; set; }
        public int AdultCount { get; set; }
        public int ElderlyCount { get; set; }
        public int DisabledCount { get; set; }
        public int PatientCount { get; set; }
        public int DeathCount { get; set; }

        [ForeignKey(nameof(SosRequestId))]
        public SosRequest? SosRequest { get; set; }
    }
}
