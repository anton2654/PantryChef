using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryChef.Business.Models;
using PantryChef.Business.Services;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;

namespace PantryChef.Tests;

public class RecipeServiceTests
{
    [Fact]
    public async Task AddRecipeAsync_WhenModelIsValid_ShouldAddRecipeAndReturnNewId()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.AddRecipeAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(recipe => recipe.Id = 15)
            .Returns(Task.CompletedTask);

        var sut = CreateService(recipeRepositoryMock);

        var model = new RecipeCreateModel
        {
            Name = "Сирники",
            Description = "Класичні сирники зі сметаною",
            Category = "Сніданки",
            Photo = "syrnyky.jpg",
            Calories = 230,
            Proteins = 12,
            Fats = 9,
            Carbohydrates = 24
        };

        // Act
        var result = await sut.AddRecipeAsync(model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Data);

        recipeRepositoryMock.Verify(repository => repository.AddRecipeAsync(It.Is<Recipe>(recipe =>
            recipe.Name == "Сирники" &&
            recipe.Description == "Класичні сирники зі сметаною" &&
            recipe.Category == "Сніданки" &&
            recipe.Photo == "syrnyky.jpg")), Times.Once);

        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddRecipeAsync_WhenModelIsNull_ShouldReturnFailure()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        var sut = CreateService(recipeRepositoryMock);

        // Act
        var result = await sut.AddRecipeAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Дані для створення страви не передані.", result.ErrorMessage);

        recipeRepositoryMock.Verify(repository => repository.AddRecipeAsync(It.IsAny<Recipe>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AddRecipeAsync_WhenModelContainsInvalidData_ShouldReturnFailure()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        var sut = CreateService(recipeRepositoryMock);

        var model = new RecipeCreateModel
        {
            Name = "  ",
            Description = "Опис",
            Category = "Сніданки",
            Photo = "image.jpg",
            Calories = 120,
            Proteins = 5,
            Fats = 4,
            Carbohydrates = 15
        };

        // Act
        var result = await sut.AddRecipeAsync(model);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Назва страви є обов'язковою.", result.ErrorMessage);

        recipeRepositoryMock.Verify(repository => repository.AddRecipeAsync(It.IsAny<Recipe>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EditRecipeAsync_WhenRecipeExistsAndModelIsValid_ShouldUpdateRecipe()
    {
        // Arrange
        var existingRecipe = new Recipe
        {
            Id = 7,
            Name = "Стара назва",
            Description = "Старий опис",
            Category = "Обіди",
            Photo = "old.jpg",
            Calories = 100,
            Proteins = 5,
            Fats = 2,
            Carbohydrates = 12
        };

        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeByIdAsync(7))
            .ReturnsAsync(existingRecipe);

        var sut = CreateService(recipeRepositoryMock);

        var model = new RecipeEditModel
        {
            Id = 7,
            Name = "Нова назва",
            Description = "Новий опис",
            Category = "Вечері",
            Photo = "new.jpg",
            Calories = 250,
            Proteins = 18,
            Fats = 11,
            Carbohydrates = 20
        };

        // Act
        var result = await sut.EditRecipeAsync(model);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Нова назва", existingRecipe.Name);
        Assert.Equal("Новий опис", existingRecipe.Description);
        Assert.Equal("Вечері", existingRecipe.Category);
        Assert.Equal("new.jpg", existingRecipe.Photo);
        Assert.Equal(250, existingRecipe.Calories);
        Assert.Equal(18, existingRecipe.Proteins);
        Assert.Equal(11, existingRecipe.Fats);
        Assert.Equal(20, existingRecipe.Carbohydrates);

        recipeRepositoryMock.Verify(repository => repository.UpdateRecipe(existingRecipe), Times.Once);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task EditRecipeAsync_WhenRecipeDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeByIdAsync(404))
            .ReturnsAsync((Recipe)null!);

        var sut = CreateService(recipeRepositoryMock);

        var model = new RecipeEditModel
        {
            Id = 404,
            Name = "Салат",
            Description = "Опис",
            Category = "Обіди",
            Photo = "salad.jpg",
            Calories = 90,
            Proteins = 4,
            Fats = 3,
            Carbohydrates = 10
        };

        // Act
        var result = await sut.EditRecipeAsync(model);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Страву з ID 404 не знайдено.", result.ErrorMessage);

        recipeRepositoryMock.Verify(repository => repository.UpdateRecipe(It.IsAny<Recipe>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EditRecipeAsync_WhenModelIsNull_ShouldReturnFailure()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        var sut = CreateService(recipeRepositoryMock);

        // Act
        var result = await sut.EditRecipeAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Дані для редагування страви не передані.", result.ErrorMessage);

        recipeRepositoryMock.Verify(repository => repository.GetRecipeByIdAsync(It.IsAny<int>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.UpdateRecipe(It.IsAny<Recipe>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_WhenRecipeExists_ShouldDeleteRecipe()
    {
        // Arrange
        var existingRecipe = new Recipe
        {
            Id = 9,
            Name = "Паста",
            Description = "Паста з томатним соусом",
            Category = "Обіди",
            Photo = "pasta.jpg"
        };

        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeByIdAsync(9))
            .ReturnsAsync(existingRecipe);

        var sut = CreateService(recipeRepositoryMock);

        // Act
        var result = await sut.DeleteRecipeAsync(9);

        // Assert
        Assert.True(result.IsSuccess);

        recipeRepositoryMock.Verify(repository => repository.DeleteRecipe(existingRecipe), Times.Once);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_WhenRecipeDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeByIdAsync(301))
            .ReturnsAsync((Recipe)null!);

        var sut = CreateService(recipeRepositoryMock);

        // Act
        var result = await sut.DeleteRecipeAsync(301);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Страву з ID 301 не знайдено.", result.ErrorMessage);

        recipeRepositoryMock.Verify(repository => repository.DeleteRecipe(It.IsAny<Recipe>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_WhenRecipeIdIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        var sut = CreateService(recipeRepositoryMock);

        // Act
        var result = await sut.DeleteRecipeAsync(0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Некоректний ідентифікатор страви.", result.ErrorMessage);

        recipeRepositoryMock.Verify(repository => repository.GetRecipeByIdAsync(It.IsAny<int>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.DeleteRecipe(It.IsAny<Recipe>()), Times.Never);
        recipeRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAvailableCategoriesAsync_WhenCalledTwice_UsesCache()
    {
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetAvailableCategoriesAsync())
            .ReturnsAsync(new List<string> { "Сніданки", "Обіди" });

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = CreateService(recipeRepositoryMock, cache);

        var firstResult = (await sut.GetAvailableCategoriesAsync()).ToList();
        var secondResult = (await sut.GetAvailableCategoriesAsync()).ToList();

        Assert.Equal(2, firstResult.Count);
        Assert.Equal(2, secondResult.Count);
        recipeRepositoryMock.Verify(repository => repository.GetAvailableCategoriesAsync(), Times.Once);
    }

    private static RecipeService CreateService(Mock<IRecipeRepository> recipeRepositoryMock, IMemoryCache? cache = null)
    {
        var settings = Options.Create(new PantryChefSettings
        {
            Caching = new CachingSettings
            {
                AvailableRecipeCategoriesTtlMinutes = 30
            }
        });

        return new RecipeService(
            recipeRepositoryMock.Object,
            Mock.Of<ILogger<RecipeService>>(),
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            settings);
    }
}
