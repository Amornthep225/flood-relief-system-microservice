namespace FloodRelief.DTOs.Sos
{
    public class SosStockCheckResponseDto
    {
        public string SosRequestId { get; set; } = string.Empty;

        public string CenterId { get; set; } = string.Empty;

        public bool IsAllEnough { get; set; }

        public List<SosStockCheckItemDto> Items { get; set; }
            = new();

        public List<SosPendingRequestDto> PendingRequests { get; set; } = new();
    }

    public class SosStockCheckItemDto
    {
        public string SosRequestItemId { get; set; } = string.Empty;
        public string ReliefItemId { get; set; } = string.Empty;

        public string ReliefItemName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        // SOS ต้องการเท่าไร
        public int RequestedQuantity { get; set; }

        // ในคลังมีเท่าไร
        public int AvailableQuantity { get; set; }

        // ถ้าจ่ายแล้วจะเหลือเท่าไร
        public int RemainingQuantity { get; set; }

        // ขาดอีกเท่าไรจากจำนวนที่ร้องขอ
        public int ShortageQuantity { get; set; }

        public bool IsEnough { get; set; }

        public List<SosPendingRequestDto> PendingRequests { get; set; } = new();
    }

    public class SosPendingRequestDto
    {
        public string SosRequestId { get; set; } = string.Empty;
        public string ReliefItemId { get; set; } = string.Empty;
        public string ReliefItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
    }
}
