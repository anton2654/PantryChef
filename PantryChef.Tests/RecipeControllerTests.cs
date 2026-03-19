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

public class RecipeControllerTests
{
    [Fact]
    public async Task Filter_WhenCategoryIsNull_ReturnsAllRecipesAndMarksAllAsSelected()
    {
        var allRecipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Scrambled Eggs", Description = "desc", Photo = "img.jpg", Category = DishCategory.Breakfast },
            new() { Id = 2, Name = "Pasta", Description = "desc", Photo = "img.jpg", Category = DishCategory.Lunch }
        };

        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock
            .Setup(repository => repository.GetAllRecipesWithIngredientsAsync())
            .ReturnsAsync(allRecipes);

        var sut = CreateController(recipeRepoMock);

        var result = await sut.Filter(null);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);

        var model = Assert.IsType<RecipeIndexViewModel>(viewResult.Model);
        Assert.Equal(string.Empty, model.SelectedCategory);
        Assert.Equal(2, model.Recipes.Count());
        Assert.Contains(model.Categories, category => category.Value == string.Empty && category.IsSelected);

        recipeRepoMock.Verify(repository => repository.GetAllRecipesWithIngredientsAsync(), Times.Once);
        recipeRepoMock.Verify(repository => repository.GetRecipesByCategoryAsync(It.IsAny<DishCategory>()), Times.Never);
    }

    [Fact]
    public async Task Filter_WhenCategoryIsValid_ReturnsFilteredRecipesAndMarksSelectedCategory()
    {
        var dinnerRecipes = new List<Recipe>
        {
            new() { Id = 3, Name = "Grilled Chicken", Description = "desc", Photo = "img.jpg", Category = DishCategory.Dinner }
        };

        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock
            .Setup(repository => repository.GetRecipesByCategoryAsync(DishCategory.Dinner))
            .ReturnsAsync(dinnerRecipes);

        var sut = CreateController(recipeRepoMock);

        var result = await sut.Filter("dinner");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeIndexViewModel>(viewResult.Model);

        Assert.Equal("Dinner", model.SelectedCategory);
        Assert.Single(model.Recipes);

        var selectedCategory = Assert.Single(model.Categories, category => category.IsSelected);
        Assert.Equal("Dinner", selectedCategory.Value);
        Assert.Equal("Вечеря", selectedCategory.Label);

        recipeRepoMock.Verify(repository => repository.GetRecipesByCategoryAsync(DishCategory.Dinner), Times.Once);
        recipeRepoMock.Verify(repository => repository.GetAllRecipesWithIngredientsAsync(), Times.Never);
    }

    [Fact]
    public async Task Filter_WhenCategoryIsInvalid_FallsBackToAllRecipesAndSetsErrorMessage()
    {
        var allRecipes = new List<Recipe>
        {
            new() { Id = 1, Name = "Scrambled Eggs", Description = "desc", Photo = "img.jpg", Category = DishCategory.Breakfast }
        };

        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock
            .Setup(repository => repository.GetAllRecipesWithIngredientsAsync())
            .ReturnsAsync(allRecipes);

        var sut = CreateController(recipeRepoMock);

        var result = await sut.Filter("not-a-category");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeIndexViewModel>(viewResult.Model);

        Assert.Equal(string.Empty, model.SelectedCategory);
        Assert.Equal("Невідома категорія фільтра. Показано всі страви.", sut.TempData["ErrorMessage"]);

        recipeRepoMock.Verify(repository => repository.GetAllRecipesWithIngredientsAsync(), Times.Once);
        recipeRepoMock.Verify(repository => repository.GetRecipesByCategoryAsync(It.IsAny<DishCategory>()), Times.Never);
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
            Category = DishCategory.Breakfast,
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

        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock
            .Setup(repository => repository.GetRecipeWithIngredientsByIdAsync(recipe.Id))
            .ReturnsAsync(recipe);

        var sut = CreateController(recipeRepoMock);

        var result = await sut.Details(recipe.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<RecipeDetailsViewModel>(viewResult.Model);

        Assert.Equal(190.4, model.Calories);
        Assert.Equal(9.0, model.Proteins);
        Assert.Equal(17.2, model.Fats);
        Assert.Equal(0.9, model.Carbohydrates);
    }

    private static RecipeController CreateController(Mock<IRecipeRepository> recipeRepoMock)
    {
        var controller = new RecipeController(recipeRepoMock.Object, Mock.Of<INutritionService>())
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };

        return controller;
    }
}
