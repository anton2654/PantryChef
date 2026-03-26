using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Web.Controllers;
using PantryChef.Web.Models;

namespace PantryChef.Tests;

public class InventoryAndRecipeControllerTests
{
    [Fact]
    public async Task InventoryIndex_ReturnsViewWithUserInventoryForCurrentUser()
    {
        var inventoryItems = new List<UserIngredient>
        {
            new() { Id = 1, UserId = 1, IngredientId = 10, Quantity = 2 }
        };

        var categories = new List<string> { "Овочі" };
        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(service => service.GetUserInventoryAsync(1, null))
            .ReturnsAsync(inventoryItems);
        inventoryServiceMock
            .Setup(service => service.GetUserInventoryCategoriesAsync(1))
            .ReturnsAsync(categories);

        var controller = new InventoryController(inventoryServiceMock.Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InventoryIndexViewModel>(viewResult.Model);
        Assert.Same(inventoryItems, model.Inventory);
        Assert.Equal(categories, model.AvailableCategories);
        inventoryServiceMock.Verify(service => service.GetUserInventoryAsync(1, null), Times.Once);
        inventoryServiceMock.Verify(service => service.GetUserInventoryCategoriesAsync(1), Times.Once);
    }

    [Fact]
    public async Task RecipeDetails_WhenRecipeMissing_ReturnsNotFound()
    {
        var recipeServiceMock = new Mock<IRecipeService>();
        recipeServiceMock
            .Setup(service => service.GetRecipeWithIngredientsByIdAsync(42))
            .ReturnsAsync((Recipe)null!);

        var nutritionServiceMock = new Mock<INutritionService>();
        var controller = new RecipeController(recipeServiceMock.Object, nutritionServiceMock.Object);

        var result = await controller.Details(42);

        Assert.IsType<NotFoundResult>(result);
        recipeServiceMock.Verify(service => service.GetRecipeWithIngredientsByIdAsync(42), Times.Once);
    }

    [Fact]
    public async Task RecipeDetails_WhenRecipeExists_ReturnsViewWithRecipe()
    {
        var recipe = new Recipe
        {
            Id = 7,
            Name = "Omelette",
            Description = "Quick egg dish",
            Photo = "omelette.jpg"
        };

        var recipeServiceMock = new Mock<IRecipeService>();
        recipeServiceMock
            .Setup(service => service.GetRecipeWithIngredientsByIdAsync(7))
            .ReturnsAsync(recipe);

        var nutritionServiceMock = new Mock<INutritionService>();
        nutritionServiceMock
            .Setup(service => service.CalculateNutrition(recipe))
            .Returns((100, 10, 5, 12));

        var controller = new RecipeController(recipeServiceMock.Object, nutritionServiceMock.Object);

        var result = await controller.Details(7);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);
        Assert.Same(recipe, model.Recipe);
    }

    [Fact]
    public async Task CalculateNutrition_UpdatesNutritionAndRedirectsToIndex()
    {
        var nutritionServiceMock = new Mock<INutritionService>();
        nutritionServiceMock
            .Setup(service => service.UpdateRecipeNutritionAsync(3))
            .ReturnsAsync(Result.Success());

        var recipeServiceMock = new Mock<IRecipeService>();

        var controller = new RecipeController(recipeServiceMock.Object, nutritionServiceMock.Object)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        var result = await controller.CalculateNutrition(3);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipeController.Details), redirectResult.ActionName);
        Assert.Equal(3, redirectResult.RouteValues?["id"]);
        nutritionServiceMock.Verify(service => service.UpdateRecipeNutritionAsync(3), Times.Once);
        Assert.Equal("КБЖВ для рецепта успішно перераховано.", controller.TempData["SuccessMessage"]);
    }
}