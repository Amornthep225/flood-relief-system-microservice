namespace FloodRelief.DTOs.Donation
{
    public class DonationTraceReportDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime ActivityAt { get; set; }
        public DateTime ReceivedAt { get; set; }

        public string DonationId { get; set; } = string.Empty;
        public string DonationBatchId { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;

        public string ReliefItemId { get; set; } = string.Empty;
        public string ReliefItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public string CenterId { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;

        public string FlowStatus { get; set; } = string.Empty;
        public string DestinationType { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public string? DestinationReference { get; set; }
        public string? DestinationAddress { get; set; }
        public string? ReceiveMethod { get; set; }
        public string? RequestStatus { get; set; }
        public string? StaffName { get; set; }
    }
}
