namespace FloodRelief.DTOs.Donation
{
    public class DonationDetailDto
    {
        public string Id { get; set; } = string.Empty;


        // User
        public string UserId { get; set; } = string.Empty;

        public string UserFullName { get; set; } = string.Empty;

        public string UserPhoneNumber { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;



        // Center
        public string? CenterId { get; set; }

        public string? CenterName { get; set; }

        public string? CenterPhoneNumber { get; set; }



        // Donation
        public string Status { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? QRCode { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }



        public List<DonationItemDto> Items { get; set; }
            = new();
    }
}