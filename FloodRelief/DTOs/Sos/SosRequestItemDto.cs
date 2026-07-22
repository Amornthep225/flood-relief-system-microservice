namespace FloodRelief.DTOs.Sos
{
    public class SosRequestItemDto
    {
        public string Id { get; set; } = string.Empty;

        public string ReliefItemId { get; set; } = string.Empty;

        public string ReliefItemName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string Unit { get; set; } = string.Empty;
    }
}