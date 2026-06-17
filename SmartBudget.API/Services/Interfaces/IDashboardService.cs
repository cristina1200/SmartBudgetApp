using SmartBudget.Server.DTOs.Dashboard;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(int userId);
    }
}