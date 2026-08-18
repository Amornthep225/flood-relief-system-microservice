namespace FloodRelief.DTOs.Donation
{
    public class DonationItemDto
    {
        public string Id { get; set; } = string.Empty;

        public string ReliefItemId { get; set; } = string.Empty;

        public string ReliefItemName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int ForwardedQuantity { get; set; }

        public int InTransitQuantity { get; set; }

        public int RemainingQuantity { get; set; }

        public string Unit { get; set; } = string.Empty;
    }
}
