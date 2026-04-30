using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
        var ingredients = new List<Ingredient>
        {
            new() { Id = 10, Name = "Морква", Category = "Овочі" }
        };
        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(service => service.GetUserInventoryAsync(1, null, null))
            .ReturnsAsync(inventoryItems);
        inventoryServiceMock
            .Setup(service => service.GetUserInventoryCategoriesAsync(1))
            .ReturnsAsync(categories);
        inventoryServiceMock
            .Setup(service => service.GetAvailableIngredientsAsync())
            .ReturnsAsync(ingredients);

        var controller = CreateInventoryController(inventoryServiceMock);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InventoryIndexViewModel>(viewResult.Model);
        Assert.Same(inventoryItems, model.Inventory);
        Assert.Equal(categories, model.AvailableCategories);
        Assert.Equal(ingredients, model.AvailableIngredients);
        inventoryServiceMock.Verify(service => service.GetUserInventoryAsync(1, null, null), Times.Once);
        inventoryServiceMock.Verify(service => service.GetUserInventoryCategoriesAsync(1), Times.Once);
        inventoryServiceMock.Verify(service => service.GetAvailableIngredientsAsync(), Times.Once);
    }

    [Fact]
    public async Task RecipeDetails_WhenRecipeMissing_ReturnsNotFound()
    {
        var recipeServiceMock = new Mock<IRecipeService>();
        recipeServiceMock
            .Setup(service => service.GetRecipeWithIngredientsByIdAsync(42))
            .ReturnsAsync((Recipe)null!);

        var nutritionServiceMock = new Mock<INutritionService>();
        var controller = CreateRecipeController(recipeServiceMock, nutritionServiceMock);

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

        var controller = CreateRecipeController(recipeServiceMock, nutritionServiceMock);

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

        var controller = CreateRecipeController(recipeServiceMock, nutritionServiceMock);

        var result = await controller.CalculateNutrition(3);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipeController.Details), redirectResult.ActionName);
        Assert.Equal(3, redirectResult.RouteValues?["id"]);
        nutritionServiceMock.Verify(service => service.UpdateRecipeNutritionAsync(3), Times.Once);
        Assert.Equal("КБЖВ для рецепта успішно перераховано.", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task RemoveIngredient_Success_SetsSuccessMessageAndRedirects()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(s => s.RemoveIngredientAsync(1, 5))
            .ReturnsAsync(Result.Success());

        var controller = CreateInventoryController(inventoryServiceMock);

        var result = await controller.RemoveIngredient(5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InventoryController.Index), redirect.ActionName);
        Assert.Equal("Інгредієнт видалено з холодильника.", controller.TempData["SuccessMessage"]);
        inventoryServiceMock.Verify(s => s.RemoveIngredientAsync(1, 5), Times.Once);
    }

    [Fact]
    public async Task RemoveIngredient_Failure_SetsErrorMessageAndRedirects()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(s => s.RemoveIngredientAsync(1, 6))
            .ReturnsAsync(Result.Failure("Not allowed"));

        var controller = CreateInventoryController(inventoryServiceMock);

        var result = await controller.RemoveIngredient(6);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InventoryController.Index), redirect.ActionName);
        Assert.Equal("Not allowed", controller.TempData["ErrorMessage"]);
        inventoryServiceMock.Verify(s => s.RemoveIngredientAsync(1, 6), Times.Once);
    }

    [Fact]
    public async Task UpdateQuantity_Success_SetsSuccessMessageAndRedirects()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(s => s.UpdateIngredientQuantityAsync(1, 8, 2.5))
            .ReturnsAsync(Result.Success());

        var controller = CreateInventoryController(inventoryServiceMock);

        var result = await controller.UpdateQuantity(8, 2.5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InventoryController.Index), redirect.ActionName);
        Assert.Equal("Кількість інгредієнта успішно оновлено.", controller.TempData["SuccessMessage"]);
        inventoryServiceMock.Verify(s => s.UpdateIngredientQuantityAsync(1, 8, 2.5), Times.Once);
    }

    [Fact]
    public async Task UpdateQuantity_Failure_SetsErrorMessageAndRedirects()
    {
        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(s => s.UpdateIngredientQuantityAsync(1, 9, 0))
            .ReturnsAsync(Result.Failure("Invalid quantity"));

        var controller = CreateInventoryController(inventoryServiceMock);

        var result = await controller.UpdateQuantity(9, 0);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InventoryController.Index), redirect.ActionName);
        Assert.Equal("Invalid quantity", controller.TempData["ErrorMessage"]);
        inventoryServiceMock.Verify(s => s.UpdateIngredientQuantityAsync(1, 9, 0), Times.Once);
    }

    [Fact]
    public async Task Index_WithSearchQuery_PassesSearchToServiceAndReturnsFilteredModel()
    {
        var inventoryItems = new List<UserIngredient>
        {
            new() { Id = 1, UserId = 1, IngredientId = 10, Quantity = 2, Ingredient = new Ingredient { Id = 10, Name = "Часник", Category = "Спеції" } }
        };

        var categories = new List<string> { "Спеції" };
        var ingredients = new List<Ingredient>
        {
            new() { Id = 10, Name = "Часник", Category = "Спеції" }
        };

        var inventoryServiceMock = new Mock<IInventoryService>();
        inventoryServiceMock
            .Setup(s => s.GetUserInventoryAsync(1, null, "Час"))
            .ReturnsAsync(inventoryItems);
        inventoryServiceMock
            .Setup(s => s.GetUserInventoryCategoriesAsync(1))
            .ReturnsAsync(categories);
        inventoryServiceMock
            .Setup(s => s.GetAvailableIngredientsAsync())
            .ReturnsAsync(ingredients);

        var controller = CreateInventoryController(inventoryServiceMock);

        var result = await controller.Index(null, "Час");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<InventoryIndexViewModel>(viewResult.Model);
        Assert.Equal("Час", model.SearchQuery);
        Assert.Same(inventoryItems, model.Inventory);
        inventoryServiceMock.Verify(s => s.GetUserInventoryAsync(1, null, "Час"), Times.Once);
    }

    private static InventoryController CreateInventoryController(Mock<IInventoryService> inventoryServiceMock)
    {
        var settings = Options.Create(new PantryChefSettings
        {
            Inventory = new InventorySettings
            {
                DefaultAddQuantity = 100,
                MinSearchLength = 2
            }
        });

        return new InventoryController(inventoryServiceMock.Object, settings)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }

    private static RecipeController CreateRecipeController(
        Mock<IRecipeService> recipeServiceMock,
        Mock<INutritionService> nutritionServiceMock)
    {
        var settings = Options.Create(new PantryChefSettings
        {
            RecipeFilter = new RecipeFilterSettings
            {
                AllCategoryLabel = "Всі страви",
                Categories = ["Сніданки", "Обіди", "Вечері"]
            }
        });

        var inventoryServiceMock = new Mock<IInventoryService>();

        return new RecipeController(recipeServiceMock.Object, inventoryServiceMock.Object, nutritionServiceMock.Object, settings)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
    }
}