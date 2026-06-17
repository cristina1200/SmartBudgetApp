using Microsoft.AspNetCore.Mvc;
using SmartBudget.Server.DTOs.Budgets;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetsController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var budgets = await _budgetService.GetAllAsync();

            return Ok(budgets);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var budgets = await _budgetService.GetByUserIdAsync(userId);

            return Ok(budgets);
        }

        [HttpGet("current/{userId}")]
        public async Task<IActionResult> GetCurrentBudget(int userId)
        {
            var budget = await _budgetService.GetCurrentBudgetAsync(userId);

            if (budget == null)
            {
                return NotFound("No budget found for current month.");
            }

            return Ok(budget);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var budget = await _budgetService.GetByIdAsync(id);

            if (budget == null)
            {
                return NotFound("Budget not found.");
            }

            return Ok(budget);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBudgetDto dto)
        {
            try
            {
                var budget = await _budgetService.CreateAsync(dto);

                if (budget == null)
                {
                    return BadRequest("Invalid user id.");
                }

                return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateBudgetDto dto)
        {
            try
            {
                var budget = await _budgetService.UpdateAsync(id, dto);

                if (budget == null)
                {
                    return NotFound("Budget not found.");
                }

                return Ok(budget);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _budgetService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Budget not found.");
            }

            return NoContent();
        }
    }
}