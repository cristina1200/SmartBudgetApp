using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBudget.Server.DTOs.Receipts;
using SmartBudget.Server.DTOs.Transactions;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptOcrService _receiptOcrService;
        private readonly ITransactionService _transactionService;

        public ReceiptsController(
            IReceiptOcrService receiptOcrService,
            ITransactionService transactionService)
        {
            _receiptOcrService = receiptOcrService;
            _transactionService = transactionService;
        }

        [HttpPost("scan")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(8 * 1024 * 1024)]
        public async Task<IActionResult> Scan(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            var result = await _receiptOcrService.ScanReceiptAsync(file);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm(ConfirmReceiptExpenseDto dto)
        {
            if (dto.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            if (dto.UserId <= 0)
            {
                return BadRequest("Invalid user id.");
            }

            if (dto.CategoryId <= 0)
            {
                return BadRequest("Invalid category id.");
            }

            var transactionDto = new CreateTransactionDto
            {
                Amount = dto.Amount,
                Date = dto.Date == default ? DateTime.Now : dto.Date,
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? "Receipt expense"
                    : dto.Description.Trim(),
                Type = TransactionType.Expense,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId
            };

            var transaction = await _transactionService.CreateAsync(transactionDto);

            if (transaction == null)
            {
                return BadRequest("Expense could not be created. Check user and category.");
            }

            return Ok(transaction);
        }
    }
}