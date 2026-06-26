using Microsoft.AspNetCore.Mvc;
using SmartBudget.Server.DTOs.Transactions;
using SmartBudget.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace SmartBudget.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _transactionService.GetAllAsync();

            return Ok(transactions);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var transactions = await _transactionService.GetByUserIdAsync(userId);

            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);

            if (transaction == null)
            {
                return NotFound("Transaction not found.");
            }

            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionDto dto)
        {
            var transaction = await _transactionService.CreateAsync(dto);

            if (transaction == null)
            {
                return BadRequest("Invalid user id or category id.");
            }

            return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _transactionService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Transaction not found.");
            }

            return NoContent();
        }
    }
}