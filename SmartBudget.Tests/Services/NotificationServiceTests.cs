using FluentAssertions;
using Moq;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Implementations;

namespace SmartBudget.Tests.Services
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task CheckMonthlyBudgetAsync_ShouldCreateDangerNotification_WhenBudgetIsExceeded()
        {
            // Arrange
            var userId = 1;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var notificationRepositoryMock = new Mock<INotificationRepository>();
            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var transactionRepositoryMock = new Mock<ITransactionRepository>();

            var budget = new Budget
            {
                Id = 1,
                UserId = userId,
                Month = currentMonth,
                Year = currentYear,
                MonthlyLimit = 100
            };

            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 1,
                    UserId = userId,
                    Amount = 70,
                    Type = TransactionType.Expense,
                    Date = DateTime.Now
                },
                new Transaction
                {
                    Id = 2,
                    UserId = userId,
                    Amount = 50,
                    Type = TransactionType.Expense,
                    Date = DateTime.Now
                }
            };

            budgetRepositoryMock
                .Setup(repo => repo.GetCurrentBudgetAsync(userId, currentMonth, currentYear))
                .ReturnsAsync(budget);

            transactionRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(transactions);

            notificationRepositoryMock
                .Setup(repo => repo.ExistsByReferenceKeyAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false);

            Notification? createdNotification = null;

            notificationRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<Notification>()))
                .Callback<Notification>(notification =>
                {
                    createdNotification = notification;
                })
                .ReturnsAsync((Notification notification) => notification);

            var service = new NotificationService(
                notificationRepositoryMock.Object,
                budgetRepositoryMock.Object,
                transactionRepositoryMock.Object);

            // Act
            await service.CheckMonthlyBudgetAsync(userId);

            // Assert
            createdNotification.Should().NotBeNull();
            createdNotification!.UserId.Should().Be(userId);
            createdNotification.Title.Should().Be("Monthly budget exceeded");
            createdNotification.Type.Should().Be(NotificationType.Danger);
            createdNotification.Message.Should().Contain("You exceeded your monthly budget");
        }

        [Fact]
        public async Task CheckMonthlyBudgetAsync_ShouldCreateWarningNotification_WhenExpensesReachEightyPercent()
        {
            // Arrange
            var userId = 1;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var notificationRepositoryMock = new Mock<INotificationRepository>();
            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var transactionRepositoryMock = new Mock<ITransactionRepository>();

            var budget = new Budget
            {
                Id = 1,
                UserId = userId,
                Month = currentMonth,
                Year = currentYear,
                MonthlyLimit = 100
            };

            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 1,
                    UserId = userId,
                    Amount = 80,
                    Type = TransactionType.Expense,
                    Date = DateTime.Now
                }
            };

            budgetRepositoryMock
                .Setup(repo => repo.GetCurrentBudgetAsync(userId, currentMonth, currentYear))
                .ReturnsAsync(budget);

            transactionRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(transactions);

            notificationRepositoryMock
                .Setup(repo => repo.ExistsByReferenceKeyAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false);

            Notification? createdNotification = null;

            notificationRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<Notification>()))
                .Callback<Notification>(notification =>
                {
                    createdNotification = notification;
                })
                .ReturnsAsync((Notification notification) => notification);

            var service = new NotificationService(
                notificationRepositoryMock.Object,
                budgetRepositoryMock.Object,
                transactionRepositoryMock.Object);

            // Act
            await service.CheckMonthlyBudgetAsync(userId);

            // Assert
            createdNotification.Should().NotBeNull();
            createdNotification!.UserId.Should().Be(userId);
            createdNotification.Title.Should().Be("Budget warning");
            createdNotification.Type.Should().Be(NotificationType.Warning);
            createdNotification.Message.Should().Contain("Careful");
        }

        [Fact]
        public async Task CheckMonthlyBudgetAsync_ShouldNotCreateNotification_WhenBudgetDoesNotExist()
        {
            // Arrange
            var userId = 1;

            var notificationRepositoryMock = new Mock<INotificationRepository>();
            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var transactionRepositoryMock = new Mock<ITransactionRepository>();

            budgetRepositoryMock
                .Setup(repo => repo.GetCurrentBudgetAsync(
                    userId,
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync((Budget?)null);

            var service = new NotificationService(
                notificationRepositoryMock.Object,
                budgetRepositoryMock.Object,
                transactionRepositoryMock.Object);

            // Act
            await service.CheckMonthlyBudgetAsync(userId);

            // Assert
            notificationRepositoryMock.Verify(
                repo => repo.CreateAsync(It.IsAny<Notification>()),
                Times.Never);
        }

        [Fact]
        public async Task CheckMonthlyBudgetAsync_ShouldNotCreateDuplicateNotification_WhenReferenceKeyAlreadyExists()
        {
            // Arrange
            var userId = 1;
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var notificationRepositoryMock = new Mock<INotificationRepository>();
            var budgetRepositoryMock = new Mock<IBudgetRepository>();
            var transactionRepositoryMock = new Mock<ITransactionRepository>();

            var budget = new Budget
            {
                Id = 1,
                UserId = userId,
                Month = currentMonth,
                Year = currentYear,
                MonthlyLimit = 100
            };

            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 1,
                    UserId = userId,
                    Amount = 150,
                    Type = TransactionType.Expense,
                    Date = DateTime.Now
                }
            };

            budgetRepositoryMock
                .Setup(repo => repo.GetCurrentBudgetAsync(userId, currentMonth, currentYear))
                .ReturnsAsync(budget);

            transactionRepositoryMock
                .Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(transactions);

            notificationRepositoryMock
                .Setup(repo => repo.ExistsByReferenceKeyAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(true);

            var service = new NotificationService(
                notificationRepositoryMock.Object,
                budgetRepositoryMock.Object,
                transactionRepositoryMock.Object);

            // Act
            await service.CheckMonthlyBudgetAsync(userId);

            // Assert
            notificationRepositoryMock.Verify(
                repo => repo.CreateAsync(It.IsAny<Notification>()),
                Times.Never);
        }
    }
}