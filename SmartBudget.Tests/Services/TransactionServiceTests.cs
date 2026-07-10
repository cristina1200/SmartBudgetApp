using FluentAssertions;
using Moq;
using SmartBudget.Server.DTOs.Transactions;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Implementations;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Tests.Services
{
    public class TransactionServiceTests
    {
        [Fact]
        public async Task CreateAsync_ShouldCreateExpenseTransaction_AndCheckMonthlyBudget()
        {
            // Arrange
            var userId = 1;
            var categoryId = 2;

            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var categoryRepositoryMock = new Mock<ICategoryRepository>();
            var notificationServiceMock = new Mock<INotificationService>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var category = new Category
            {
                Id = categoryId,
                Name = "Shopping market",
                Type = TransactionType.Expense,
                UserId = userId
            };

            var dto = new CreateTransactionDto
            {
                Amount = 70,
                Description = "Shopping market",
                Date = DateTime.Now,
                Type = TransactionType.Expense,
                UserId = userId,
                CategoryId = categoryId
            };

            var createdTransaction = new Transaction
            {
                Id = 10,
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                UserId = userId,
                CategoryId = categoryId
            };

            var completeTransaction = new Transaction
            {
                Id = 10,
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                UserId = userId,
                CategoryId = categoryId,
                Category = category
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            transactionRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync(createdTransaction);

            transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(createdTransaction.Id))
                .ReturnsAsync(completeTransaction);

            var service = new TransactionService(
                transactionRepositoryMock.Object,
                userRepositoryMock.Object,
                categoryRepositoryMock.Object,
                notificationServiceMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
            result.Amount.Should().Be(70);
            result.Type.Should().Be("Expense");
            result.CategoryName.Should().Be("Shopping market");

            notificationServiceMock.Verify(
                service => service.CheckMonthlyBudgetAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateIncomeTransaction_WithoutCheckingMonthlyBudget()
        {
            // Arrange
            var userId = 1;
            var categoryId = 3;

            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var categoryRepositoryMock = new Mock<ICategoryRepository>();
            var notificationServiceMock = new Mock<INotificationService>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var category = new Category
            {
                Id = categoryId,
                Name = "Salary",
                Type = TransactionType.Income,
                UserId = userId
            };

            var dto = new CreateTransactionDto
            {
                Amount = 100,
                Description = "Salary",
                Date = DateTime.Now,
                Type = TransactionType.Income,
                UserId = userId,
                CategoryId = categoryId
            };

            var createdTransaction = new Transaction
            {
                Id = 11,
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                UserId = userId,
                CategoryId = categoryId
            };

            var completeTransaction = new Transaction
            {
                Id = 11,
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                UserId = userId,
                CategoryId = categoryId,
                Category = category
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            transactionRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<Transaction>()))
                .ReturnsAsync(createdTransaction);

            transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(createdTransaction.Id))
                .ReturnsAsync(completeTransaction);

            var service = new TransactionService(
                transactionRepositoryMock.Object,
                userRepositoryMock.Object,
                categoryRepositoryMock.Object,
                notificationServiceMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Type.Should().Be("Income");
            result.CategoryName.Should().Be("Salary");

            notificationServiceMock.Verify(
                service => service.CheckMonthlyBudgetAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var categoryRepositoryMock = new Mock<ICategoryRepository>();
            var notificationServiceMock = new Mock<INotificationService>();

            var dto = new CreateTransactionDto
            {
                Amount = 50,
                Description = "Expense",
                Date = DateTime.Now,
                Type = TransactionType.Expense,
                UserId = 99,
                CategoryId = 1
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.UserId))
                .ReturnsAsync((User?)null);

            var service = new TransactionService(
                transactionRepositoryMock.Object,
                userRepositoryMock.Object,
                categoryRepositoryMock.Object,
                notificationServiceMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().BeNull();

            transactionRepositoryMock.Verify(
                repo => repo.CreateAsync(It.IsAny<Transaction>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var userId = 1;

            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var categoryRepositoryMock = new Mock<ICategoryRepository>();
            var notificationServiceMock = new Mock<INotificationService>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateTransactionDto
            {
                Amount = 50,
                Description = "Expense",
                Date = DateTime.Now,
                Type = TransactionType.Expense,
                UserId = userId,
                CategoryId = 999
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(dto.CategoryId))
                .ReturnsAsync((Category?)null);

            var service = new TransactionService(
                transactionRepositoryMock.Object,
                userRepositoryMock.Object,
                categoryRepositoryMock.Object,
                notificationServiceMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().BeNull();

            transactionRepositoryMock.Verify(
                repo => repo.CreateAsync(It.IsAny<Transaction>()),
                Times.Never);
        }
    }
}