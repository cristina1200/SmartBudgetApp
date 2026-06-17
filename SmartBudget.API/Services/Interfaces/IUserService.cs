using SmartBudget.Server.DTOs.Users;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync();

        Task<UserResponseDto?> GetByIdAsync(int id);

        Task<UserResponseDto> CreateAsync(CreateUserDto dto);

        Task<bool> DeleteAsync(int id);
    }
}