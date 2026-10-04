using System.ComponentModel.DataAnnotations;

namespace FloodRelief.DTOs.Sos
{
    public class CreateSosVictimSeverityCountDto
    {
        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = string.Empty;

        [Range(0, 1000)] public int ChildCount { get; set; }
        [Range(0, 1000)] public int AdultCount { get; set; }
        [Range(0, 1000)] public int ElderlyCount { get; set; }
        [Range(0, 1000)] public int DisabledCount { get; set; }
        [Range(0, 1000)] public int PatientCount { get; set; }
        [Range(0, 1000)] public int DeathCount { get; set; }
    }

    public class SosVictimSeverityCountDto
    {
        public string Severity { get; set; } = string.Empty;
        public int ChildCount { get; set; }
        public int AdultCount { get; set; }
        public int ElderlyCount { get; set; }
        public int DisabledCount { get; set; }
        public int PatientCount { get; set; }
        public int DeathCount { get; set; }

        public int TotalCount =>
            ChildCount + AdultCount + ElderlyCount + DisabledCount + PatientCount + DeathCount;
    }
}
