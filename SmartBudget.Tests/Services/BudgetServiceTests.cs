using FluentAssertions;
using Moq;
using SmartBudget.Server.DTOs.Budgets;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Implementations;

namespace SmartBudget.Tests.Services
{
    public class BudgetServiceTests
    {
        [Fact]
        public async Task CreateAsync_ShouldCreateBudget_WhenDataIsValid()
        {
            // Arrange
            var userId = 1;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateBudgetDto
            {
                UserId = userId,
                Month = currentMonth,
                Year = currentYear,
                MonthlyLimit = 1000
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            budgetRepositoryMock
                .Setup(repo => repo.GetCurrentBudgetAsync(userId, currentMonth, currentYear))
                .ReturnsAsync((Budget?)null);

            budgetRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<Budget>()))
                .ReturnsAsync((Budget budget) =>
                {
                    budget.Id = 10;
                    return budget;
                });

            var service = new BudgetService(
                budgetRepositoryMock.Object,
                userRepositoryMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
            result.UserId.Should().Be(userId);
            result.Month.Should().Be(currentMonth);
            result.Year.Should().Be(currentYear);
            result.MonthlyLimit.Should().Be(1000);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var dto = new CreateBudgetDto
            {
                UserId = 99,
                Month = 6,
                Year = 2026,
                MonthlyLimit = 1000
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.UserId))
                .ReturnsAsync((User?)null);

            var service = new BudgetService(
                budgetRepositoryMock.Object,
                userRepositoryMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().BeNull();

            budgetRepositoryMock.Verify(
                repo => repo.CreateAsync(It.IsAny<Budget>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenMonthIsInvalid()
        {
            // Arrange
            var userId = 1;

            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateBudgetDto
            {
                UserId = userId,
                Month = 13,
                Year = 2026,
                MonthlyLimit = 1000
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var service = new BudgetService(
                budgetRepositoryMock.Object,
                userRepositoryMock.Object);

            // Act
            var act = async () => await service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Month must be between 1 and 12.");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenMonthlyLimitIsInvalid()
        {
            // Arrange
            var userId = 1;

            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateBudgetDto
            {
                UserId = userId,
                Month = 6,
                Year = 2026,
                MonthlyLimit = 0
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var service = new BudgetService(
                budgetRepositoryMock.Object,
                userRepositoryMock.Object);

            // Act
            var act = async () => await service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Monthly limit must be greater than 0.");
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenBudgetAlreadyExistsForMonth()
        {
            // Arrange
            var userId = 1;

            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateBudgetDto
            {
                UserId = userId,
                Month = 6,
                Year = 2026,
                MonthlyLimit = 1000
            };

            var existingBudget = new Budget
            {
                Id = 1,
                UserId = userId,
                Month = 6,
                Year = 2026,
                MonthlyLimit = 500
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            budgetRepositoryMock
                .Setup(repo => repo.GetCurrentBudgetAsync(userId, dto.Month, dto.Year))
                .ReturnsAsync(existingBudget);

            var service = new BudgetService(
                budgetRepositoryMock.Object,
                userRepositoryMock.Object);

            // Act
            var act = async () => await service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("A budget already exists for this user, month and year.");
        }
    }
}