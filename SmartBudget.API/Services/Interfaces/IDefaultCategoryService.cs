namespace SmartBudget.Server.Services.Interfaces
{
    public interface IDefaultCategoryService
    {
        Task CreateDefaultCategoriesForUserAsync(int userId);
    }
}