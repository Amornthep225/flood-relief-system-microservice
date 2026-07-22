namespace FloodRelief.DTOs.Donation
{
    public class DonationResponseDto
    {
        public string Id { get; set; } = string.Empty;


        public string UserId { get; set; } = string.Empty;


        public string DonorName { get; set; } = string.Empty;


        public string DonationType { get; set; } = string.Empty;


        public string Description { get; set; } = string.Empty;


        public int Quantity { get; set; }


        public string Unit { get; set; } = string.Empty;


        public string Status { get; set; } = string.Empty;


        public DateTime CreatedAt { get; set; }
    }
}