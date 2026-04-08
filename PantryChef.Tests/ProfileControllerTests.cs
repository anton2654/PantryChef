using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Web.Controllers;
using PantryChef.Web.Models;

namespace PantryChef.Tests;

public class ProfileControllerTests
{
    [Fact]
    public async Task Index_WhenProfileLoaded_ReturnsViewWithProfileModel()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.GetProfileAsync(1))
            .ReturnsAsync(Result<UserProfileData>.Success(new UserProfileData
            {
                UserId = 1,
                Name = "Alice",
                Email = "alice@example.com",
                DailyCalorieGoal = 2000
            }));
        profileServiceMock
            .Setup(service => service.GetGoalProgressAsync(1, null))
            .ReturnsAsync(Result<UserGoalProgress>.Success(new UserGoalProgress()));

        var controller = CreateController(profileServiceMock);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileIndexViewModel>(viewResult.Model);
        Assert.Equal("Alice", model.Name);
        Assert.Equal(2000, model.DailyCalorieGoal);
    }

    [Fact]
    public async Task Index_WhenProfileLoadingFails_ReturnsViewWithErrorMessage()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.GetProfileAsync(1))
            .ReturnsAsync(Result<UserProfileData>.Failure("Profile error"));

        var controller = CreateController(profileServiceMock);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ProfileIndexViewModel>(viewResult.Model);
        Assert.Equal("Profile error", controller.TempData["ErrorMessage"]);

        profileServiceMock.Verify(service => service.GetGoalProgressAsync(It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task Index_WhenGoalProgressFails_ReturnsProfileModelAndSetsError()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.GetProfileAsync(1))
            .ReturnsAsync(Result<UserProfileData>.Success(new UserProfileData
            {
                UserId = 1,
                Name = "Alice",
                Email = "alice@example.com",
                DailyCalorieGoal = 2000
            }));
        profileServiceMock
            .Setup(service => service.GetGoalProgressAsync(1, 1200))
            .ReturnsAsync(Result<UserGoalProgress>.Failure("Progress error"));

        var controller = CreateController(profileServiceMock);

        var result = await controller.Index(consumedCaloriesToday: 1200);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileIndexViewModel>(viewResult.Model);
        Assert.Equal("Alice", model.Name);
        Assert.Equal(1200, model.ConsumedCaloriesToday);
        Assert.Null(model.CalorieDifference);
        Assert.Equal("Progress error", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task SetWeightGoals_WhenServiceFails_SetsErrorAndRedirects()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.SetWeightGoalAsync(1, 10, 60))
            .ReturnsAsync(Result.Failure("Validation error"));

        var controller = CreateController(profileServiceMock);

        var result = await controller.SetWeightGoals(10, 60);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Validation error", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task SetWeightGoals_WhenServiceSucceeds_SetsSuccessAndRedirects()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.SetWeightGoalAsync(1, 82.4, 75.0))
            .ReturnsAsync(Result.Success());

        var controller = CreateController(profileServiceMock);

        var result = await controller.SetWeightGoals(82.4, 75.0);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Цілі по вазі успішно збережено.", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task CalculateDailyCalories_WhenSuccess_SetsSuccessMessage()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.CalculateDailyCaloriesAsync(1, 80, 180, 30))
            .ReturnsAsync(Result<int>.Success(1780));

        var controller = CreateController(profileServiceMock);

        var result = await controller.CalculateDailyCalories(80, 180, 30);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal(
            "Авто-розрахунок: 1780 ккал. Щоб застосувати це значення, натисніть \"Зберегти\" у блоці ручного редагування калорій.",
            controller.TempData["SuccessMessage"]);
        Assert.Equal(80d, redirectResult.RouteValues?["autoWeightKg"]);
        Assert.Equal(180d, redirectResult.RouteValues?["autoHeightCm"]);
        Assert.Equal(30, redirectResult.RouteValues?["autoAge"]);
        Assert.Equal(1780, redirectResult.RouteValues?["autoCalculatedCalories"]);
    }

    [Fact]
    public async Task CalculateDailyCalories_WhenServiceFails_SetsErrorAndRedirectsToIndex()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.CalculateDailyCaloriesAsync(1, 80, 180, 30))
            .ReturnsAsync(Result<int>.Failure("Validation error"));

        var controller = CreateController(profileServiceMock);

        var result = await controller.CalculateDailyCalories(80, 180, 30);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Validation error", controller.TempData["ErrorMessage"]);
        Assert.Null(redirectResult.RouteValues);
    }

    [Fact]
    public async Task ResetGoals_WhenServiceSucceeds_SetsSuccessAndRedirects()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.ResetGoalsAsync(1))
            .ReturnsAsync(Result.Success());

        var controller = CreateController(profileServiceMock);

        var result = await controller.ResetGoals();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Цілі скинуто до стандартних значень.", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task ResetGoals_WhenServiceFails_SetsErrorAndRedirects()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.ResetGoalsAsync(1))
            .ReturnsAsync(Result.Failure("Reset failed"));

        var controller = CreateController(profileServiceMock);

        var result = await controller.ResetGoals();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Reset failed", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task UpdateDailyCalories_WhenServiceSucceeds_SetsSuccessAndRedirects()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.UpdateManualCalorieGoalAsync(1, 2100))
            .ReturnsAsync(Result.Success());

        var controller = CreateController(profileServiceMock);

        var result = await controller.UpdateDailyCalories(2100);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Добову норму калорій збережено вручну.", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task UpdateDailyCalories_WhenServiceFails_SetsErrorAndRedirects()
    {
        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(service => service.UpdateManualCalorieGoalAsync(1, 500))
            .ReturnsAsync(Result.Failure("Calories validation error"));

        var controller = CreateController(profileServiceMock);

        var result = await controller.UpdateDailyCalories(500);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Calories validation error", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public void CalculateGoalProgress_WhenNegativeInput_SetsErrorMessage()
    {
        var profileServiceMock = new Mock<IProfileService>();
        var controller = CreateController(profileServiceMock);

        var result = controller.CalculateGoalProgress(-5);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal("Спожиті калорії не можуть бути від'ємними.", controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public void CalculateGoalProgress_WhenInputValid_RedirectsWithRouteValue()
    {
        var profileServiceMock = new Mock<IProfileService>();
        var controller = CreateController(profileServiceMock);

        var result = controller.CalculateGoalProgress(1350);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);
        Assert.Equal(1350, redirectResult.RouteValues?["consumedCaloriesToday"]);
    }

    private static ProfileController CreateController(Mock<IProfileService> profileServiceMock)
    {
        return new ProfileController(profileServiceMock.Object, Mock.Of<ILogger<ProfileController>>())
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }
}
