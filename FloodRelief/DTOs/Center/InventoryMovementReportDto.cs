namespace FloodRelief.DTOs.Center
{
    public class InventoryMovementReportDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int BalanceBefore { get; set; }
        public int BalanceAfter { get; set; }

        public string CenterInventoryId { get; set; } = string.Empty;
        public string CenterId { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;

        public string ReliefItemId { get; set; } = string.Empty;
        public string ReliefItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public string? ReferenceType { get; set; }
        public string? ReferenceId { get; set; }

        public string SourceType { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string? SourceReference { get; set; }

        public string DestinationType { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string? DestinationReference { get; set; }
        public string? DestinationAddress { get; set; }

        public string? StaffName { get; set; }
        public string? Note { get; set; }
    }
}
