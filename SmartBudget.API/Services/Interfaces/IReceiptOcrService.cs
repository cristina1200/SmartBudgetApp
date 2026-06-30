using Microsoft.AspNetCore.Http;
using SmartBudget.Server.DTOs.Receipts;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface IReceiptOcrService
    {
        Task<ReceiptScanResponseDto> ScanReceiptAsync(IFormFile file);
    }
}