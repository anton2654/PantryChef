using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryChef.Business.Models;
using PantryChef.Business.Services;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;

namespace PantryChef.Tests;

public class ProfileServiceTests
{
    [Fact]
    public async Task GetProfileAsync_WhenUserExists_ReturnsMappedProfileData()
    {
        var user = CreateUser();
        user.CurrentWeightKg = 82.4;
        user.TargetWeightKg = 75.0;
        user.HeightCm = 178;
        user.Age = 29;
        user.IsCalorieGoalManuallySet = true;
        var sut = CreateService(user);

        var result = await sut.Service.GetProfileAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data.UserId);
        Assert.Equal(user.Name, result.Data.Name);
        Assert.Equal(user.Email, result.Data.Email);
        Assert.Equal(user.CurrentWeightKg, result.Data.CurrentWeightKg);
        Assert.Equal(user.TargetWeightKg, result.Data.TargetWeightKg);
        Assert.Equal(user.HeightCm, result.Data.HeightCm);
        Assert.Equal(user.Age, result.Data.Age);
        Assert.Equal(user.CalorieGoals, result.Data.DailyCalorieGoal);
        Assert.Equal(user.IsCalorieGoalManuallySet, result.Data.IsCalorieGoalManuallySet);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.GetProfileAsync(user.Id + 100);

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
    }

    [Fact]
    public async Task SetWeightGoalAsync_WhenValidData_UpdatesUserAndSaves()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.SetWeightGoalAsync(user.Id, 82.4, 75.0);

        Assert.True(result.IsSuccess);
        Assert.Equal(82.4, user.CurrentWeightKg);
        Assert.Equal(75.0, user.TargetWeightKg);
        sut.UserRepositoryMock.Verify(repository => repository.Update(user), Times.Once);
        sut.UserRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SetWeightGoalAsync_WhenWeightInvalid_ReturnsFailure()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.SetWeightGoalAsync(user.Id, 10, 70);

        Assert.False(result.IsSuccess);
        Assert.Equal("Вага має бути в діапазоні від 20 до 350 кг.", result.ErrorMessage);
        sut.UserRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ResetGoalsAsync_WhenUserExists_ResetsGoalsToDefaults()
    {
        var user = CreateUser();
        user.CurrentWeightKg = 84.3;
        user.TargetWeightKg = 77.0;
        user.HeightCm = 180;
        user.Age = 30;
        user.CalorieGoals = 2500;
        user.IsCalorieGoalManuallySet = true;

        var sut = CreateService(user, defaultCalorieGoal: 2000);

        var result = await sut.Service.ResetGoalsAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Null(user.CurrentWeightKg);
        Assert.Null(user.TargetWeightKg);
        Assert.Null(user.HeightCm);
        Assert.Null(user.Age);
        Assert.False(user.IsCalorieGoalManuallySet);
        Assert.Equal(2000, user.CalorieGoals);
    }

    [Fact]
    public async Task ResetGoalsAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.ResetGoalsAsync(user.Id + 100);

        Assert.False(result.IsSuccess);
        Assert.Equal("Користувача не знайдено.", result.ErrorMessage);
        sut.UserRepositoryMock.Verify(repository => repository.Update(It.IsAny<User>()), Times.Never);
        sut.UserRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CalculateDailyCaloriesAsync_WhenValidInput_UsesFormulaAndStoresResult()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.CalculateDailyCaloriesAsync(user.Id, 80, 180, 30);

        Assert.True(result.IsSuccess);
        Assert.Equal(1780, result.Data);
        Assert.Equal(2000, user.CalorieGoals);
        Assert.Null(user.CurrentWeightKg);
        Assert.Null(user.HeightCm);
        Assert.Null(user.Age);
        Assert.False(user.IsCalorieGoalManuallySet);
        sut.UserRepositoryMock.Verify(repository => repository.Update(It.IsAny<User>()), Times.Never);
        sut.UserRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CalculateDailyCaloriesAsync_WhenAgeInvalid_ReturnsFailure()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.CalculateDailyCaloriesAsync(user.Id, 80, 180, 5);

        Assert.False(result.IsSuccess);
        Assert.Equal("Вік має бути в діапазоні від 10 до 100 років.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateManualCalorieGoalAsync_WhenValid_SetsManualMode()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.UpdateManualCalorieGoalAsync(user.Id, 2100);

        Assert.True(result.IsSuccess);
        Assert.Equal(2100, user.CalorieGoals);
        Assert.True(user.IsCalorieGoalManuallySet);
    }

    [Theory]
    [InlineData(799)]
    [InlineData(6001)]
    public async Task UpdateManualCalorieGoalAsync_WhenOutsideAllowedRange_ReturnsFailure(int dailyCalories)
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.UpdateManualCalorieGoalAsync(user.Id, dailyCalories);

        Assert.False(result.IsSuccess);
        Assert.Equal("Норма калорій має бути в діапазоні від 800 до 6000 ккал.", result.ErrorMessage);
        sut.UserRepositoryMock.Verify(repository => repository.Update(It.IsAny<User>()), Times.Never);
        sut.UserRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetGoalProgressAsync_WhenValuesPresent_ReturnsDifferences()
    {
        var user = CreateUser();
        user.CurrentWeightKg = 85;
        user.TargetWeightKg = 78;
        user.CalorieGoals = 2000;

        var sut = CreateService(user);

        var result = await sut.Service.GetGoalProgressAsync(user.Id, consumedCaloriesToday: 1300);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(-7, result.Data.WeightDifferenceKg);
        Assert.Equal(7, result.Data.WeightRemainingKg);
        Assert.Equal(700, result.Data.CalorieDifference);
        Assert.Equal(700, result.Data.CalorieRemaining);
        Assert.Equal(0, result.Data.CalorieExceeded);
    }

    [Fact]
    public async Task GetGoalProgressAsync_WhenCaloriesExceeded_ReturnsExceededWithoutNegativeRemaining()
    {
        var user = CreateUser();
        user.CalorieGoals = 1578;

        var sut = CreateService(user);

        var result = await sut.Service.GetGoalProgressAsync(user.Id, consumedCaloriesToday: 1700);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(-122, result.Data.CalorieDifference);
        Assert.Equal(0, result.Data.CalorieRemaining);
        Assert.Equal(122, result.Data.CalorieExceeded);
    }

    [Fact]
    public async Task GetGoalProgressAsync_WhenConsumedCaloriesNegative_ReturnsFailure()
    {
        var user = CreateUser();
        var sut = CreateService(user);

        var result = await sut.Service.GetGoalProgressAsync(user.Id, consumedCaloriesToday: -10);

        Assert.False(result.IsSuccess);
        Assert.Equal("Спожиті калорії не можуть бути від'ємними.", result.ErrorMessage);
    }

    private static (ProfileService Service, Mock<IUserRepository> UserRepositoryMock) CreateService(
        User user,
        int defaultCalorieGoal = 2000)
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int requestedUserId) => requestedUserId == user.Id ? user : null!);

        var settings = Options.Create(new PantryChefSettings
        {
            DefaultCalorieGoals = defaultCalorieGoal
        });

        var service = new ProfileService(
            userRepositoryMock.Object,
            Mock.Of<ILogger<ProfileService>>(),
            settings);

        return (service, userRepositoryMock);
    }

    private static User CreateUser()
    {
        return new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "IDENTITY_MANAGED",
            CalorieGoals = 2000,
            Allergies = "none",
            IdentityUserId = "identity-1"
        };
    }
}
