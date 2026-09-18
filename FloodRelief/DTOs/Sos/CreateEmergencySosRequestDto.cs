using System.ComponentModel.DataAnnotations;
using FloodRelief.Constants;

namespace FloodRelief.DTOs.Sos
{
    public class CreateEmergencySosRequestDto
    {
        [Range(-90, 90, ErrorMessage = "Latitude ต้องอยู่ระหว่าง -90 ถึง 90")]
        public double Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude ต้องอยู่ระหว่าง -180 ถึง 180")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "กรุณาระบุรายละเอียดสถานที่")]
        [StringLength(500)]
        public string AddressDetail { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกประเภทเหตุฉุกเฉิน")]
        [StringLength(50)]
        public string EmergencyType { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "จำนวนผู้ประสบภัยต้องอย่างน้อย 1 คน")]
        public int VictimCount { get; set; } = 1;

        [Range(0, 1000)]
        public int ChildCount { get; set; }

        [Range(0, 1000)]
        public int ElderlyCount { get; set; }

        [Range(0, 1000)]
        public int DisabledCount { get; set; }

        [Range(0, 1000)]
        public int PatientCount { get; set; }

        [Range(0, 1000, ErrorMessage = "จำนวนผู้เสียชีวิตต้องเป็น 0 ขึ้นไป")]
        public int DeathCount { get; set; }

        [Required(ErrorMessage = "กรุณาระบุระดับความรุนแรงของผู้ประสบภัย")]
        [StringLength(20)]
        public string Severity { get; set; } = SosSeverities.Moderate;

        [Range(0, 20, ErrorMessage = "ระดับน้ำต้องอยู่ระหว่าง 0-20 เมตร")]
        public decimal? WaterLevel { get; set; }

        [Required(ErrorMessage = "กรุณาระบุรายละเอียดเหตุฉุกเฉิน")]
        [StringLength(1000)]
        public string EmergencyDetail { get; set; } = string.Empty;
    }
}
