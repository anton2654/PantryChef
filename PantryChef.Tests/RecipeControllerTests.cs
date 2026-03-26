using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Web.Controllers;
using PantryChef.Web.Models;

namespace PantryChef.Tests;

public class RecipeControllerTests
{
    [Fact]
    public async Task Filter_WhenCategoryIsNull_ReturnsAllRecipesAndMarksAllAsSelected()
    {
        var allRecipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Scrambled Eggs", Description = "desc", Photo = "img.jpg", Category = "Сніданки" },
            new() { Id = 2, Name = "Pasta", Description = "desc", Photo = "img.jpg", Category = "Обіди" }
        };

        var recipeServiceMock = new Mock<IRecipeService>();
        recipeServiceMock
            .Setup(service => service.GetAllRecipesWithIngredientsAsync())
            .ReturnsAsync(allRecipes);

        var sut = CreateController(recipeServiceMock);

        var result = await sut.Filter(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);

        var model = Assert.IsType<RecipeIndexViewModel>(viewResult.Model);
        Assert.Equal(string.Empty, model.SelectedCategory);
        Assert.Equal(2, model.Recipes.Count());
        Assert.Contains(model.Categories, category => category.Value == string.Empty && category.IsSelected);

        recipeServiceMock.Verify(service => service.GetAllRecipesWithIngredientsAsync(), Times.Once);
        recipeServiceMock.Verify(service => service.GetRecipesByCategoryAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Filter_WhenCategoryIsValid_ReturnsFilteredRecipesAndMarksSelectedCategory()
    {
        var dinnerRecipes = new List<Recipe>
        {
            new() { Id = 3, Name = "Grilled Chicken", Description = "desc", Photo = "img.jpg", Category = "Вечері" }
        };

        var recipeServiceMock = new Mock<IRecipeService>();
        recipeServiceMock
            .Setup(service => service.GetRecipesByCategoryAsync("Вечері"))
            .ReturnsAsync(dinnerRecipes);

        var sut = CreateController(recipeServiceMock);

        var result = await sut.Filter("Вечері");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeIndexViewModel>(viewResult.Model);

        Assert.Equal("Вечері", model.SelectedCategory);
        Assert.Single(model.Recipes);

        var selectedCategory = Assert.Single(model.Categories, category => category.IsSelected);
        Assert.Equal("Вечері", selectedCategory.Value);
        Assert.Equal("Вечері", selectedCategory.Label);

        recipeServiceMock.Verify(service => service.GetRecipesByCategoryAsync("Вечері"), Times.Once);
        recipeServiceMock.Verify(service => service.GetAllRecipesWithIngredientsAsync(), Times.Never);
    }

    [Fact]
    public async Task Filter_WhenCategoryIsProvided_CallsServiceWithCategory()
    {
        var filteredRecipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Tomato Soup", Description = "desc", Photo = "img.jpg", Category = "Перші страви" }
        };

        var recipeServiceMock = new Mock<IRecipeService>();
        recipeServiceMock
            .Setup(service => service.GetRecipesByCategoryAsync("Перші страви"))
            .ReturnsAsync(filteredRecipes);

        var sut = CreateController(recipeServiceMock);

        var result = await sut.Filter("Перші страви");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeIndexViewModel>(viewResult.Model);

        Assert.Equal("Перші страви", model.SelectedCategory);
        Assert.Single(model.Recipes);

        recipeServiceMock.Verify(service => service.GetRecipesByCategoryAsync("Перші страви"), Times.Once);
        recipeServiceMock.Verify(service => service.GetAllRecipesWithIngredientsAsync(), Times.Never);
    }

    [Fact]
    public async Task Details_WhenRecipeHasIngredients_CalculatesNutritionFromIngredients()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Scrambled Eggs",
            Description = "desc",
            Photo = "img.jpg",
            Category = "Сніданки",
            Calories = 999,
            Proteins = 999,
            Fats = 999,
            Carbohydrates = 999,
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    RecipeId = 1,
                    IngredientId = 2,
                    Quantity = 150,
                    Ingredient = new Ingredient
                    {
                        Id = 2,
                        Name = "Egg",
                        Category = "Dairy",
                        Photo = "egg.jpg",
                        Calories = 68,
                        Proteins = 6,
                        Fats = 4.8,
                        Carbohydrates = 0.6
                    }
                },
                new RecipeIngredient
                {
                    RecipeId = 1,
                    IngredientId = 5,
                    Quantity = 10,
                    Ingredient = new Ingredient
                    {
                        Id = 5,
                        Name = "Olive Oil",
                        Category = "Oil",
                        Photo = "olive_oil.jpg",
                        Calories = 884,
                        Proteins = 0,
                        Fats = 100,
                        Carbohydrates = 0
                    }
                }
            ]
        };

        var recipeServiceMock = new Mock<IRecipeService>();
        recipeServiceMock
            .Setup(service => service.GetRecipeWithIngredientsByIdAsync(recipe.Id))
            .ReturnsAsync(recipe);

        var nutritionServiceMock = new Mock<INutritionService>();
        nutritionServiceMock
            .Setup(service => service.CalculateNutrition(recipe))
            .Returns((190.4, 9.0, 17.2, 0.9));

        var sut = CreateController(recipeServiceMock, nutritionServiceMock);

        var result = await sut.Details(recipe.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);

        Assert.Equal(190.4, model.Calories);
        Assert.Equal(9.0, model.Proteins);
        Assert.Equal(17.2, model.Fats);
        Assert.Equal(0.9, model.Carbohydrates);
    }

    private static RecipeController CreateController(
        Mock<IRecipeService> recipeServiceMock,
        Mock<INutritionService>? nutritionServiceMock = null)
    {
        var controller = new RecipeController(
            recipeServiceMock.Object,
            nutritionServiceMock?.Object ?? Mock.Of<INutritionService>())
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        return controller;
    }
}
