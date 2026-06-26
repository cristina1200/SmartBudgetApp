using SmartBudget.Server.Models;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}