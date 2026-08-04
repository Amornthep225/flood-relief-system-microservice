namespace FloodRelief.DTOs.Donation
{
    public class DonationReceiveLookupDto
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;
        public string DonorPhoneNumber { get; set; } = string.Empty;
        public string CenterId { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool CanReceive { get; set; }
        public List<DonationReceiveLookupItemDto> Items { get; set; } = new();
    }

    public class DonationReceiveLookupItemDto
    {
        public string DonationItemId { get; set; } = string.Empty;
        public string ReliefItemId { get; set; } = string.Empty;
        public string ReliefItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
