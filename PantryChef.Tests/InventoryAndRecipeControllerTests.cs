using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
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

        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        inventoryRepoMock
            .Setup(repo => repo.GetUserInventoryAsync(1))
            .ReturnsAsync(inventoryItems);

        var inventoryServiceMock = new Mock<IInventoryService>();
        var controller = new InventoryController(inventoryRepoMock.Object, inventoryServiceMock.Object);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(inventoryItems, viewResult.Model);
        inventoryRepoMock.Verify(repo => repo.GetUserInventoryAsync(1), Times.Once);
    }

    [Fact]
    public async Task RecipeDetails_WhenRecipeMissing_ReturnsNotFound()
    {
        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock
            .Setup(repo => repo.GetRecipeWithIngredientsByIdAsync(42))
            .ReturnsAsync((Recipe)null!);

        var nutritionServiceMock = new Mock<INutritionService>();
        var controller = new RecipeController(recipeRepoMock.Object, nutritionServiceMock.Object);

        var result = await controller.Details(42);

        Assert.IsType<NotFoundResult>(result);
        recipeRepoMock.Verify(repo => repo.GetRecipeWithIngredientsByIdAsync(42), Times.Once);
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

        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock
            .Setup(repo => repo.GetRecipeWithIngredientsByIdAsync(7))
            .ReturnsAsync(recipe);

        var nutritionServiceMock = new Mock<INutritionService>();
        var controller = new RecipeController(recipeRepoMock.Object, nutritionServiceMock.Object);

        var result = await controller.Details(7);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);
        Assert.Same(recipe, model.Recipe);
    }

    [Fact]
    public async Task CalculateNutrition_UpdatesNutritionAndRedirectsToIndex()
    {
        var recipeRepoMock = new Mock<IRecipeRepository>();
        var nutritionServiceMock = new Mock<INutritionService>();
        nutritionServiceMock
            .Setup(service => service.UpdateRecipeNutritionAsync(3))
            .Returns(Task.CompletedTask);

        var controller = new RecipeController(recipeRepoMock.Object, nutritionServiceMock.Object)
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