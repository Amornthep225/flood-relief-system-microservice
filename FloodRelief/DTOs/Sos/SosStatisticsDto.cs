namespace FloodRelief.DTOs.Sos
{
    public class SosStatisticsDto
    {
        public int Total { get; set; }

        public int Pending { get; set; }

        public int Accepted { get; set; }

        public int Preparing { get; set; }

        public int Delivering { get; set; }

        public int Completed { get; set; }

        public int Rejected { get; set; }

        public int Cancelled { get; set; }
    }
}