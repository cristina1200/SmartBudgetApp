namespace SmartBudget.Server.DTOs.Receipts
{
    public class ReceiptScanResponseDto
    {
        public bool Success { get; set; }

        public decimal? SuggestedAmount { get; set; }

        public DateTime? SuggestedDate { get; set; }

        public string SuggestedMerchant { get; set; } = string.Empty;

        public string SuggestedDescription { get; set; } = string.Empty;

        public string RawText { get; set; } = string.Empty;

        public float Confidence { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}