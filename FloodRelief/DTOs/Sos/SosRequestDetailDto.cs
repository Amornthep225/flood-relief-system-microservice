namespace FloodRelief.DTOs.Sos
{
    public class SosRequestDetailDto
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string UserFullName { get; set; } = string.Empty;

        public string UserPhoneNumber { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string? CenterId { get; set; }

        public string? CenterName { get; set; }

        public string? CenterPhoneNumber { get; set; }

        public string? AssignedStaffId { get; set; }

        public string? AssignedStaffName { get; set; }

        public string? AssignedStaffPhoneNumber { get; set; }


        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string AddressDetail { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? UserRemark { get; set; }

        public string? StaffRemark { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? AcceptedAt { get; set; }

        public DateTime? PreparingAt { get; set; }

        public DateTime? DeliveringAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<SosRequestItemDto> Items { get; set; } = new();
    }
}