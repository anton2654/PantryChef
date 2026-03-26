using Microsoft.Extensions.Logging;
using Moq;
using PantryChef.Business.Services;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;

namespace PantryChef.Tests;

public class NutritionServiceTests
{
    [Fact]
    public async Task UpdateRecipeNutritionAsync_WhenRecipeExists_UpdatesNutritionAndSaves()
    {
        var recipe = new Recipe
        {
            Id = 1,
            Name = "Test Recipe",
            Description = "desc",
            Photo = "img.jpg",
            Category = "Сніданки",
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    RecipeId = 1,
                    IngredientId = 1,
                    Quantity = 200,
                    Ingredient = new Ingredient
                    {
                        Id = 1,
                        Name = "Chicken",
                        Category = "Meat",
                        Photo = "chicken.jpg",
                        Calories = 165,
                        Proteins = 31,
                        Fats = 3.6,
                        Carbohydrates = 0
                    }
                },
                new RecipeIngredient
                {
                    RecipeId = 1,
                    IngredientId = 5,
                    Quantity = 20,
                    Ingredient = new Ingredient
                    {
                        Id = 5,
                        Name = "Olive Oil",
                        Category = "Oil",
                        Photo = "oil.jpg",
                        Calories = 884,
                        Proteins = 0,
                        Fats = 100,
                        Carbohydrates = 0
                    }
                }
            ]
        };

        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeWithIngredientsByIdAsync(recipe.Id))
            .ReturnsAsync(recipe);

        var sut = new NutritionService(recipeRepositoryMock.Object, Mock.Of<ILogger<NutritionService>>());

        var result = await sut.UpdateRecipeNutritionAsync(recipe.Id);

        Assert.Equal(506.8, recipe.Calories);
        Assert.Equal(62.0, recipe.Proteins);
        Assert.Equal(27.2, recipe.Fats);
        Assert.Equal(0.0, recipe.Carbohydrates);
        Assert.True(result.IsSuccess);

        recipeRepositoryMock.Verify(repository => repository.Update(recipe), Times.Once);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateRecipeNutritionAsync_WhenRecipeDoesNotExist_ReturnsFailureResult()
    {
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeWithIngredientsByIdAsync(404))
            .ReturnsAsync((Recipe)null!);

        var sut = new NutritionService(recipeRepositoryMock.Object, Mock.Of<ILogger<NutritionService>>());

        var result = await sut.UpdateRecipeNutritionAsync(404);

        Assert.False(result.IsSuccess);
        Assert.Equal("Рецепт з ID 404 не існує.", result.ErrorMessage);

        recipeRepositoryMock.Verify(repository => repository.Update(It.IsAny<Recipe>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }
}
