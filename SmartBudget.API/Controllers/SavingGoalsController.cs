using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBudget.Server.DTOs.SavingGoals;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SavingGoalsController : ControllerBase
    {
        private readonly ISavingGoalService _savingGoalService;

        public SavingGoalsController(ISavingGoalService savingGoalService)
        {
            _savingGoalService = savingGoalService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var goals = await _savingGoalService.GetAllAsync();

            return Ok(goals);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var goals = await _savingGoalService.GetByUserIdAsync(userId);

            return Ok(goals);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var goal = await _savingGoalService.GetByIdAsync(id);

            if (goal == null)
            {
                return NotFound("Saving goal not found.");
            }

            return Ok(goal);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSavingGoalDto dto)
        {
            try
            {
                var goal = await _savingGoalService.CreateAsync(dto);

                if (goal == null)
                {
                    return BadRequest("Invalid user id.");
                }

                return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSavingGoalDto dto)
        {
            try
            {
                var goal = await _savingGoalService.UpdateAsync(id, dto);

                if (goal == null)
                {
                    return NotFound("Saving goal not found.");
                }

                return Ok(goal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/add-money")]
        public async Task<IActionResult> AddMoney(int id, AddMoneyToGoalDto dto)
        {
            try
            {
                var goal = await _savingGoalService.AddMoneyAsync(id, dto);

                if (goal == null)
                {
                    return NotFound("Saving goal not found.");
                }

                return Ok(goal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _savingGoalService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound("Saving goal not found.");
            }

            return NoContent();
        }
    }
}