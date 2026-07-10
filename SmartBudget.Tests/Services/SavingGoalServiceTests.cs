using FluentAssertions;
using Moq;
using SmartBudget.Server.DTOs.SavingGoals;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Implementations;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Tests.Services
{
    public class SavingGoalServiceTests
    {
        [Fact]
        public async Task CreateAsync_ShouldCreateSavingGoal_AndInitialContribution_WhenCurrentAmountIsGreaterThanZero()
        {
            // Arrange
            var userId = 1;

            var savingGoalRepositoryMock = new Mock<ISavingGoalRepository>();
            var contributionRepositoryMock = new Mock<ISavingGoalContributionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var currencyConverterMock = new Mock<ICurrencyConverterService>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateSavingGoalDto
            {
                Name = "Telefon",
                TargetAmount = 4000,
                CurrentAmount = 1000,
                Deadline = DateTime.Now.AddMonths(6),
                UserId = userId,
                Currency = Currency.RON
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            savingGoalRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<SavingGoal>()))
                .ReturnsAsync((SavingGoal goal) =>
                {
                    goal.Id = 10;
                    return goal;
                });

            SavingGoalContribution? createdContribution = null;

            contributionRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<SavingGoalContribution>()))
                .Callback<SavingGoalContribution>(contribution =>
                {
                    createdContribution = contribution;
                })
                .ReturnsAsync((SavingGoalContribution contribution) => contribution);

            var service = new SavingGoalService(
                savingGoalRepositoryMock.Object,
                contributionRepositoryMock.Object,
                userRepositoryMock.Object,
                currencyConverterMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
            result.Name.Should().Be("Telefon");
            result.TargetAmount.Should().Be(4000);
            result.CurrentAmount.Should().Be(1000);
            result.ProgressPercentage.Should().Be(25);
            result.Currency.Should().Be(Currency.RON);

            createdContribution.Should().NotBeNull();
            createdContribution!.SavingGoalId.Should().Be(10);
            createdContribution.Amount.Should().Be(1000);
            createdContribution.Note.Should().Be("Initial amount");
        }

        [Fact]
        public async Task CreateAsync_ShouldCapCurrentAmount_WhenItIsGreaterThanTargetAmount()
        {
            // Arrange
            var userId = 1;

            var savingGoalRepositoryMock = new Mock<ISavingGoalRepository>();
            var contributionRepositoryMock = new Mock<ISavingGoalContributionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var currencyConverterMock = new Mock<ICurrencyConverterService>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateSavingGoalDto
            {
                Name = "Laptop",
                TargetAmount = 3000,
                CurrentAmount = 5000,
                UserId = userId,
                Currency = Currency.RON
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            savingGoalRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<SavingGoal>()))
                .ReturnsAsync((SavingGoal goal) =>
                {
                    goal.Id = 20;
                    return goal;
                });

            var service = new SavingGoalService(
                savingGoalRepositoryMock.Object,
                contributionRepositoryMock.Object,
                userRepositoryMock.Object,
                currencyConverterMock.Object);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.TargetAmount.Should().Be(3000);
            result.CurrentAmount.Should().Be(3000);
            result.ProgressPercentage.Should().Be(100);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenTargetAmountIsInvalid()
        {
            // Arrange
            var userId = 1;

            var savingGoalRepositoryMock = new Mock<ISavingGoalRepository>();
            var contributionRepositoryMock = new Mock<ISavingGoalContributionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var currencyConverterMock = new Mock<ICurrencyConverterService>();

            var user = new User
            {
                Id = userId,
                FirstName = "Cristina",
                LastName = "Fatan",
                Email = "cristina@example.com"
            };

            var dto = new CreateSavingGoalDto
            {
                Name = "Invalid goal",
                TargetAmount = 0,
                CurrentAmount = 0,
                UserId = userId,
                Currency = Currency.RON
            };

            userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var service = new SavingGoalService(
                savingGoalRepositoryMock.Object,
                contributionRepositoryMock.Object,
                userRepositoryMock.Object,
                currencyConverterMock.Object);

            // Act
            var act = async () => await service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Target amount must be greater than 0.");
        }

        [Fact]
        public async Task AddMoneyAsync_ShouldConvertCurrency_AndCapAmountToRemainingGoal()
        {
            // Arrange
            var goalId = 1;

            var savingGoalRepositoryMock = new Mock<ISavingGoalRepository>();
            var contributionRepositoryMock = new Mock<ISavingGoalContributionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var currencyConverterMock = new Mock<ICurrencyConverterService>();

            var goal = new SavingGoal
            {
                Id = goalId,
                Name = "Telefon",
                TargetAmount = 4000,
                CurrentAmount = 3900,
                Currency = Currency.RON,
                UserId = 1
            };

            var dto = new AddMoneyToGoalDto
            {
                Amount = 200,
                Currency = Currency.RON,
                Note = "Extra saving"
            };

            savingGoalRepositoryMock
                .Setup(repo => repo.GetByIdAsync(goalId))
                .ReturnsAsync(goal);

            currencyConverterMock
                .Setup(service => service.ConvertAsync(200, Currency.RON, Currency.RON))
                .ReturnsAsync(200);

            SavingGoalContribution? createdContribution = null;

            contributionRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<SavingGoalContribution>()))
                .Callback<SavingGoalContribution>(contribution =>
                {
                    createdContribution = contribution;
                })
                .ReturnsAsync((SavingGoalContribution contribution) => contribution);

            savingGoalRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<SavingGoal>()))
                .ReturnsAsync((SavingGoal updatedGoal) => updatedGoal);

            var service = new SavingGoalService(
                savingGoalRepositoryMock.Object,
                contributionRepositoryMock.Object,
                userRepositoryMock.Object,
                currencyConverterMock.Object);

            // Act
            var result = await service.AddMoneyAsync(goalId, dto);

            // Assert
            result.Should().NotBeNull();
            result!.CurrentAmount.Should().Be(4000);
            result.ProgressPercentage.Should().Be(100);

            createdContribution.Should().NotBeNull();
            createdContribution!.Amount.Should().Be(100);
            createdContribution.OriginalAmount.Should().Be(200);
            createdContribution.Note.Should().Be("Extra saving");
        }

        [Fact]
        public async Task GetDetailsAsync_ShouldReturnStatistics_ForExistingGoal()
        {
            // Arrange
            var goalId = 1;

            var savingGoalRepositoryMock = new Mock<ISavingGoalRepository>();
            var contributionRepositoryMock = new Mock<ISavingGoalContributionRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var currencyConverterMock = new Mock<ICurrencyConverterService>();

            var goal = new SavingGoal
            {
                Id = goalId,
                Name = "Telefon",
                TargetAmount = 4000,
                CurrentAmount = 1000,
                Currency = Currency.RON,
                UserId = 1,
                Deadline = DateTime.Now.AddMonths(3)
            };

            var contributions = new List<SavingGoalContribution>
            {
                new SavingGoalContribution
                {
                    Id = 1,
                    SavingGoalId = goalId,
                    Amount = 1000,
                    OriginalAmount = 1000,
                    Currency = Currency.RON,
                    Note = "Initial amount",
                    CreatedAt = DateTime.Now.AddDays(-6)
                }
            };

            savingGoalRepositoryMock
                .Setup(repo => repo.GetByIdAsync(goalId))
                .ReturnsAsync(goal);

            contributionRepositoryMock
                .Setup(repo => repo.GetBySavingGoalIdAsync(goalId))
                .ReturnsAsync(contributions);

            var service = new SavingGoalService(
                savingGoalRepositoryMock.Object,
                contributionRepositoryMock.Object,
                userRepositoryMock.Object,
                currencyConverterMock.Object);

            // Act
            var result = await service.GetDetailsAsync(goalId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Telefon");
            result.TargetAmount.Should().Be(4000);
            result.CurrentAmount.Should().Be(1000);
            result.RemainingAmount.Should().Be(3000);
            result.ProgressPercentage.Should().Be(25);
            result.TotalContributed.Should().Be(1000);
            result.Contributions.Should().HaveCount(1);
            result.SmartMessage.Should().NotBeNullOrWhiteSpace();
        }
    }
}