using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryChef.Business.Models;
using PantryChef.Business.Services;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using PantryChef.Business.Interfaces;

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
    public async Task RemoveRecipeForUserAsync_WhenRecipeExists_CreatesHiddenLink()
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

        var userRecipeRepoMock = new Mock<IUserRecipeRepository>();
        userRecipeRepoMock
            .Setup(repository => repository.GetAsync(1, 9))
            .ReturnsAsync((UserRecipe)null!);

        var sut = CreateService(recipeRepositoryMock, userRecipeRepoMock: userRecipeRepoMock);

        // Act
        var result = await sut.RemoveRecipeForUserAsync(1, 9);

        // Assert
        Assert.True(result.IsSuccess);
        userRecipeRepoMock.Verify(repository => repository.AddAsync(It.Is<UserRecipe>(link =>
            link.UserId == 1 && link.RecipeId == 9 && link.IsSaved == false)), Times.Once);
        userRecipeRepoMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveRecipeForUserAsync_WhenLinkExists_UpdatesHiddenFlag()
    {
        // Arrange
        var existingRecipe = new Recipe
        {
            Id = 12,
            Name = "Суп",
            Description = "desc",
            Category = "Перші страви",
            Photo = "soup.jpg"
        };

        var link = new UserRecipe
        {
            UserId = 1,
            RecipeId = 12,
            IsSaved = true
        };

        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeByIdAsync(12))
            .ReturnsAsync(existingRecipe);

        var userRecipeRepoMock = new Mock<IUserRecipeRepository>();
        userRecipeRepoMock
            .Setup(repository => repository.GetAsync(1, 12))
            .ReturnsAsync(link);

        var sut = CreateService(recipeRepositoryMock, userRecipeRepoMock: userRecipeRepoMock);

        // Act
        var result = await sut.RemoveRecipeForUserAsync(1, 12);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(link.IsSaved);
        userRecipeRepoMock.Verify(repository => repository.Update(link), Times.Once);
        userRecipeRepoMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveRecipeForUserAsync_WhenRecipeMissing_ReturnsFailure()
    {
        // Arrange
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetRecipeByIdAsync(301))
            .ReturnsAsync((Recipe)null!);

        var userRecipeRepoMock = new Mock<IUserRecipeRepository>();
        var sut = CreateService(recipeRepositoryMock, userRecipeRepoMock: userRecipeRepoMock);

        // Act
        var result = await sut.RemoveRecipeForUserAsync(1, 301);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Страву з ID 301 не знайдено.", result.ErrorMessage);
        userRecipeRepoMock.Verify(repository => repository.AddAsync(It.IsAny<UserRecipe>()), Times.Never);
        userRecipeRepoMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAvailableCategoriesAsync_WhenCalledTwice_UsesCache()
    {
        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetAvailableCategoriesAsync())
            .ReturnsAsync(new List<string> { "Сніданки", "Обіди" });

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = CreateService(recipeRepositoryMock, cache: cache);

        var firstResult = (await sut.GetAvailableCategoriesAsync()).ToList();
        var secondResult = (await sut.GetAvailableCategoriesAsync()).ToList();

        Assert.Equal(2, firstResult.Count);
        Assert.Equal(2, secondResult.Count);
        recipeRepositoryMock.Verify(repository => repository.GetAvailableCategoriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetFullMatchRecipesAsync_WhenInventoryHasAllIngredients_ReturnsOnlyFullMatches()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new()
            {
                Id = 1,
                Name = "Омлет",
                Description = "desc",
                Photo = "img.jpg",
                Category = "Сніданки",
                RecipeIngredients =
                [
                    new RecipeIngredient
                    {
                        RecipeId = 1,
                        IngredientId = 10,
                        Quantity = 100,
                        Ingredient = new Ingredient
                        {
                            Id = 10,
                            Name = "Яйце",
                            Category = "Молочні",
                            Photo = "egg.jpg"
                        }
                    }
                ]
            },
            new()
            {
                Id = 2,
                Name = "Салат",
                Description = "desc",
                Photo = "img.jpg",
                Category = "Обіди",
                RecipeIngredients =
                [
                    new RecipeIngredient
                    {
                        RecipeId = 2,
                        IngredientId = 11,
                        Quantity = 200,
                        Ingredient = new Ingredient
                        {
                            Id = 11,
                            Name = "Помідор",
                            Category = "Овочі",
                            Photo = "tomato.jpg"
                        }
                    }
                ]
            }
        };

        var inventory = new List<UserIngredient>
        {
            new()
            {
                UserId = 1,
                IngredientId = 10,
                Quantity = 150,
                Ingredient = new Ingredient
                {
                    Id = 10,
                    Name = "Яйце",
                    Category = "Молочні",
                    Photo = "egg.jpg"
                }
            }
        };

        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetAllRecipesWithIngredientsAsync())
            .ReturnsAsync(recipes);

        var userIngredientRepoMock = new Mock<IUserIngredientRepository>();
        userIngredientRepoMock
            .Setup(repository => repository.GetUserInventoryAsync(1))
            .ReturnsAsync(inventory);

        var sut = CreateService(recipeRepositoryMock, userIngredientRepoMock);

        // Act
        var result = await sut.GetFullMatchRecipesAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
        Assert.Equal(1, result.Data[0].Recipe.Id);
    }

    [Fact]
    public async Task GetPartialMatchRecipesAsync_WhenIngredientAbsent_ReturnsMissingDeficit()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new()
            {
                Id = 3,
                Name = "Паста",
                Description = "desc",
                Photo = "img.jpg",
                Category = "Обіди",
                RecipeIngredients =
                [
                    new RecipeIngredient
                    {
                        RecipeId = 3,
                        IngredientId = 12,
                        Quantity = 120,
                        Ingredient = new Ingredient
                        {
                            Id = 12,
                            Name = "Макарони",
                            Category = "Крупи",
                            Photo = "pasta.jpg"
                        }
                    }
                ]
            }
        };

        var recipeRepositoryMock = new Mock<IRecipeRepository>();
        recipeRepositoryMock
            .Setup(repository => repository.GetAllRecipesWithIngredientsAsync())
            .ReturnsAsync(recipes);

        var userIngredientRepoMock = new Mock<IUserIngredientRepository>();
        userIngredientRepoMock
            .Setup(repository => repository.GetUserInventoryAsync(1))
            .ReturnsAsync(new List<UserIngredient>());

        var sut = CreateService(recipeRepositoryMock, userIngredientRepoMock);

        // Act
        var result = await sut.GetPartialMatchRecipesAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
        var deficit = Assert.Single(result.Data[0].MissingIngredients);
        Assert.Equal(0, deficit.AvailableQuantity);
        Assert.Equal(120, deficit.MissingQuantity);
    }

    private static RecipeService CreateService(
        Mock<IRecipeRepository> recipeRepositoryMock,
        Mock<IUserIngredientRepository>? userIngredientRepoMock = null,
        Mock<IUserRecipeRepository>? userRecipeRepoMock = null,
        IMemoryCache? cache = null,
        Mock<IInventoryService>? inventoryServiceMock = null,
        Mock<INutritionService>? nutritionServiceMock = null)
    {
        userIngredientRepoMock ??= new Mock<IUserIngredientRepository>();
        userRecipeRepoMock ??= new Mock<IUserRecipeRepository>();
        inventoryServiceMock ??= new Mock<IInventoryService>();
        nutritionServiceMock ??= new Mock<INutritionService>();

        var settings = Options.Create(new PantryChefSettings
        {
            Caching = new CachingSettings
            {
                AvailableRecipeCategoriesTtlMinutes = 30
            }
        });

        return new RecipeService(
            recipeRepositoryMock.Object,
            userIngredientRepoMock.Object,
            userRecipeRepoMock.Object,
            Mock.Of<ILogger<RecipeService>>(),
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new PantryChefSettings()),
            inventoryServiceMock.Object,
            nutritionServiceMock.Object);
    }

    [Fact]
    public async Task CookRecipeAsync_WhenInventorySucceeds_ConsumesIngredientsAndRecordsNutrition()
    {
        var recipe = new Recipe
        {
            Id = 100,
            Name = "Тест блюдо",
            Description = "desc",
            RecipeIngredients = new List<RecipeIngredient>
            {
                new() { IngredientId = 1, Quantity = 100, Ingredient = new Ingredient { Id = 1, Name = "Інгр", Category = "Категорія" } }
            }
        };

        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock.Setup(r => r.GetRecipeWithIngredientsByIdAsync(100)).ReturnsAsync(recipe);

        var userIngredientRepoMock = new Mock<IUserIngredientRepository>();
        var userRecipeRepoMock = new Mock<IUserRecipeRepository>();

        var inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(s => s.CookRecipeAsync(1, 100)).ReturnsAsync(Result.Success());

        var nutritionMock = new Mock<INutritionService>();
        nutritionMock.Setup(n => n.CalculateNutrition(recipe)).Returns((10.0, 1.0, 1.0, 2.0));
        nutritionMock.Setup(n => n.AddConsumedNutritionAsync(1, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime?>())).ReturnsAsync(Result.Success());

        var sut = CreateService(recipeRepoMock, userIngredientRepoMock, userRecipeRepoMock, cache: new MemoryCache(new MemoryCacheOptions()), inventoryServiceMock: inventoryMock, nutritionServiceMock: nutritionMock);

        var result = await sut.CookRecipeAsync(1, 100);

        Assert.True(result.IsSuccess);
        nutritionMock.Verify(n => n.AddConsumedNutritionAsync(1, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime?>()), Times.Once);
    }

    [Fact]
    public async Task CookRecipeAsync_WhenInventoryFails_ReturnsFailure()
    {
        var recipe = new Recipe { Id = 101, Name = "Fail dish", Description = "desc" };

        var recipeRepoMock = new Mock<IRecipeRepository>();
        recipeRepoMock.Setup(r => r.GetRecipeWithIngredientsByIdAsync(101)).ReturnsAsync(recipe);

        var inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(s => s.CookRecipeAsync(1, 101)).ReturnsAsync(Result.Failure("Недостатньо інгредієнтів"));

        var nutritionMock = new Mock<INutritionService>();

        var sut = CreateService(recipeRepoMock, inventoryServiceMock: inventoryMock, nutritionServiceMock: nutritionMock);

        var result = await sut.CookRecipeAsync(1, 101);

        Assert.False(result.IsSuccess);
        Assert.Contains("Недостатньо інгредієнтів", result.ErrorMessage);
    }
}
