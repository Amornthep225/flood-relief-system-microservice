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

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}