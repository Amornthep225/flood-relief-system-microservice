namespace FloodRelief.DTOs.Sos
{
    public class SosRequestListDto
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string UserFullName { get; set; } = string.Empty;

        public string UserPhoneNumber { get; set; } = string.Empty;

        public string? CenterId { get; set; }

        public string? CenterName { get; set; }

        public string? AssignedStaffId { get; set; }

        public string? AssignedStaffName { get; set; }
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string AddressDetail { get; set; } = string.Empty;

        public string RequestType { get; set; } = "Relief";

        public string? ReceiveMethod { get; set; }

        public string? EmergencyType { get; set; }

        public int VictimCount { get; set; }

        public int ChildCount { get; set; }

        public int ElderlyCount { get; set; }

        public int DisabledCount { get; set; }

        public int PatientCount { get; set; }

        public int DeathCount { get; set; }

        public string? Severity { get; set; }

        public decimal? WaterLevel { get; set; }

        public string? EmergencyDetail { get; set; }

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<SosRequestItemDto> Items { get; set; } = new();
    }
}
