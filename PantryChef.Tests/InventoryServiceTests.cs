using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryChef.Business.Models;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Services;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System;

namespace PantryChef.Tests;

public class InventoryServiceTests
{
    [Fact]
    public async Task RemoveIngredientAsync_WhenIngredientExists_RemovesAndSaves()
    {
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var loggerMock = new Mock<ILogger<InventoryService>>();
        var item = new UserIngredient { UserId = 1, IngredientId = 5, Quantity = 120 };

        inventoryRepoMock
            .Setup(repo => repo.GetUserIngredientAsync(1, 5))
            .ReturnsAsync(item);

        var service = CreateService(inventoryRepoMock, ingredientRepoMock, loggerMock);

        var result = await service.RemoveIngredientAsync(1, 5);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        inventoryRepoMock.Verify(repo => repo.GetUserIngredientAsync(1, 5), Times.Once);
        inventoryRepoMock.Verify(repo => repo.Delete(item), Times.Once);
        inventoryRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveIngredientAsync_WhenIngredientMissing_ReturnsFailure()
    {
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var loggerMock = new Mock<ILogger<InventoryService>>();

        inventoryRepoMock
            .Setup(repo => repo.GetUserIngredientAsync(1, 999))
            .ReturnsAsync((UserIngredient)null!);

        var service = CreateService(inventoryRepoMock, ingredientRepoMock, loggerMock);

        var result = await service.RemoveIngredientAsync(1, 999);

        Assert.False(result.IsSuccess);
        Assert.Equal("Інгредієнт не знайдено у холодильнику.", result.ErrorMessage);
        inventoryRepoMock.Verify(repo => repo.GetUserIngredientAsync(1, 999), Times.Once);
        inventoryRepoMock.Verify(repo => repo.Delete(It.IsAny<UserIngredient>()), Times.Never);
        inventoryRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateIngredientQuantityAsync_WhenValidInput_UpdatesQuantityAndSaves()
    {
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var loggerMock = new Mock<ILogger<InventoryService>>();
        var item = new UserIngredient { UserId = 1, IngredientId = 8, Quantity = 2.0 };

        inventoryRepoMock
            .Setup(repo => repo.GetUserIngredientAsync(1, 8))
            .ReturnsAsync(item);

        var service = CreateService(inventoryRepoMock, ingredientRepoMock, loggerMock);

        var result = await service.UpdateIngredientQuantityAsync(1, 8, 2.5);

        Assert.True(result.IsSuccess);
        Assert.Equal(2.5, item.Quantity);
        inventoryRepoMock.Verify(repo => repo.GetUserIngredientAsync(1, 8), Times.Once);
        inventoryRepoMock.Verify(repo => repo.Update(item), Times.Once);
        inventoryRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateIngredientQuantityAsync_WhenNonPositiveQuantity_ReturnsFailure()
    {
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var loggerMock = new Mock<ILogger<InventoryService>>();

        var service = CreateService(inventoryRepoMock, ingredientRepoMock, loggerMock);

        var result = await service.UpdateIngredientQuantityAsync(1, 8, 0);

        Assert.False(result.IsSuccess);
        Assert.Equal("Кількість має бути більшою за нуль. Якщо хочете видалити продукт, скористайтеся відповідною кнопкою.", result.ErrorMessage);
        inventoryRepoMock.Verify(repo => repo.GetUserIngredientAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        inventoryRepoMock.Verify(repo => repo.Update(It.IsAny<UserIngredient>()), Times.Never);
        inventoryRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetUserInventoryAsync_WithSearchQuery_FiltersByIngredientNameCaseInsensitive()
    {
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var loggerMock = new Mock<ILogger<InventoryService>>();

        var inventory = new List<UserIngredient>
        {
            new()
            {
                UserId = 1,
                IngredientId = 10,
                Quantity = 100,
                Ingredient = new Ingredient { Id = 10, Name = "Часник", Category = "Спеції" }
            },
            new()
            {
                UserId = 1,
                IngredientId = 11,
                Quantity = 200,
                Ingredient = new Ingredient { Id = 11, Name = "Цибуля", Category = "Овочі" }
            }
        };

        inventoryRepoMock
            .Setup(repo => repo.GetUserInventoryAsync(1))
            .ReturnsAsync(inventory);

        var service = CreateService(inventoryRepoMock, ingredientRepoMock, loggerMock);

        var result = (await service.GetUserInventoryAsync(1, null, "час")).ToList();

        Assert.Single(result);
        Assert.Equal("Часник", result[0].Ingredient.Name);
        inventoryRepoMock.Verify(repo => repo.GetUserInventoryAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetAvailableIngredientsAsync_WhenCalledTwice_UsesCache()
    {
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var loggerMock = new Mock<ILogger<InventoryService>>();

        ingredientRepoMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Ingredient>
            {
                new() { Id = 2, Name = "Яблуко", Category = "Фрукти" },
                new() { Id = 1, Name = "Банан", Category = "Фрукти" }
            });

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = CreateService(inventoryRepoMock, ingredientRepoMock, loggerMock, cache: cache);

        var firstResult = (await service.GetAvailableIngredientsAsync()).ToList();
        var secondResult = (await service.GetAvailableIngredientsAsync()).ToList();

        Assert.Equal(2, firstResult.Count);
        Assert.Equal(2, secondResult.Count);
        Assert.Equal("Банан", firstResult[0].Name);
        ingredientRepoMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CookRecipeAsync_WhenIngredientsSufficient_UpdatesInventoryAndLogsNutrition()
    {
        // Arrange
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var recipeRepoMock = new Mock<IRecipeRepository>();
        var shoppingListRepoMock = new Mock<IShoppingListRepository>();
        var nutritionServiceMock = new Mock<INutritionService>();
        var loggerMock = new Mock<ILogger<InventoryService>>();

        var recipe = new Recipe
        {
            Id = 5,
            Name = "Омлет",
            Description = "desc",
            Photo = "img.jpg",
            Category = "Сніданки",
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    RecipeId = 5,
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
        };

        var inventoryItem = new UserIngredient
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
        };

        recipeRepoMock
            .Setup(repo => repo.GetRecipeWithIngredientsByIdAsync(5))
            .ReturnsAsync(recipe);

        inventoryRepoMock
            .Setup(repo => repo.GetUserInventoryAsync(1))
            .ReturnsAsync(new List<UserIngredient> { inventoryItem });

        nutritionServiceMock
            .Setup(service => service.CalculateNutrition(recipe))
            .Returns((200, 10, 5, 0));

        nutritionServiceMock
            .Setup(service => service.AddConsumedNutritionAsync(1, 200, 10, 5, 0, It.IsAny<DateTime?>()))
            .ReturnsAsync(Result.Success());

        var service = CreateService(
            inventoryRepoMock,
            ingredientRepoMock,
            loggerMock,
            recipeRepoMock,
            shoppingListRepoMock,
            nutritionServiceMock);

        // Act
        var result = await service.CookRecipeAsync(1, 5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(50, inventoryItem.Quantity);
        inventoryRepoMock.Verify(repo => repo.Update(inventoryItem), Times.Once);
        inventoryRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        nutritionServiceMock.Verify(service => service.AddConsumedNutritionAsync(1, 200, 10, 5, 0, It.IsAny<DateTime?>()), Times.Once);
    }

    [Fact]
    public async Task CookRecipeAsync_WhenIngredientMissing_ReturnsFailure()
    {
        // Arrange
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var recipeRepoMock = new Mock<IRecipeRepository>();
        var shoppingListRepoMock = new Mock<IShoppingListRepository>();
        var nutritionServiceMock = new Mock<INutritionService>();
        var loggerMock = new Mock<ILogger<InventoryService>>();

        var recipe = new Recipe
        {
            Id = 6,
            Name = "Салат",
            Description = "desc",
            Photo = "img.jpg",
            Category = "Обіди",
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    RecipeId = 6,
                    IngredientId = 20,
                    Quantity = 50,
                    Ingredient = new Ingredient
                    {
                        Id = 20,
                        Name = "Огірок",
                        Category = "Овочі",
                        Photo = "cucumber.jpg"
                    }
                }
            ]
        };

        recipeRepoMock
            .Setup(repo => repo.GetRecipeWithIngredientsByIdAsync(6))
            .ReturnsAsync(recipe);

        inventoryRepoMock
            .Setup(repo => repo.GetUserInventoryAsync(1))
            .ReturnsAsync(new List<UserIngredient>());

        var service = CreateService(
            inventoryRepoMock,
            ingredientRepoMock,
            loggerMock,
            recipeRepoMock,
            shoppingListRepoMock,
            nutritionServiceMock);

        // Act
        var result = await service.CookRecipeAsync(1, 6);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Недостатньо інгредієнта", result.ErrorMessage);
        inventoryRepoMock.Verify(repo => repo.Update(It.IsAny<UserIngredient>()), Times.Never);
        nutritionServiceMock.Verify(service => service.AddConsumedNutritionAsync(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime?>()), Times.Never);
    }

    [Fact]
    public async Task AddMissingIngredientsToShoppingListAsync_WhenIngredientAbsent_AddsMissingQuantity()
    {
        // Arrange
        var inventoryRepoMock = new Mock<IUserIngredientRepository>();
        var ingredientRepoMock = new Mock<IIngredientRepository>();
        var recipeRepoMock = new Mock<IRecipeRepository>();
        var shoppingListRepoMock = new Mock<IShoppingListRepository>();
        var nutritionServiceMock = new Mock<INutritionService>();
        var loggerMock = new Mock<ILogger<InventoryService>>();

        var recipe = new Recipe
        {
            Id = 7,
            Name = "Суп",
            Description = "desc",
            Photo = "img.jpg",
            Category = "Перші страви",
            RecipeIngredients =
            [
                new RecipeIngredient
                {
                    RecipeId = 7,
                    IngredientId = 30,
                    Quantity = 120,
                    Ingredient = new Ingredient
                    {
                        Id = 30,
                        Name = "Морква",
                        Category = "Овочі",
                        Photo = "carrot.jpg"
                    }
                }
            ]
        };

        recipeRepoMock
            .Setup(repo => repo.GetRecipeWithIngredientsByIdAsync(7))
            .ReturnsAsync(recipe);

        inventoryRepoMock
            .Setup(repo => repo.GetUserInventoryAsync(1))
            .ReturnsAsync(new List<UserIngredient>());

        shoppingListRepoMock
            .Setup(repo => repo.GetItemAsync(1, 30))
            .ReturnsAsync((ShoppingListItem)null!);

        var service = CreateService(
            inventoryRepoMock,
            ingredientRepoMock,
            loggerMock,
            recipeRepoMock,
            shoppingListRepoMock,
            nutritionServiceMock);

        // Act
        var result = await service.AddMissingIngredientsToShoppingListAsync(1, 7);

        // Assert
        Assert.True(result.IsSuccess);
        shoppingListRepoMock.Verify(repo => repo.AddAsync(It.Is<ShoppingListItem>(item =>
            item.UserId == 1 && item.IngredientId == 30 && item.Quantity == 120)), Times.Once);
        shoppingListRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    private static InventoryService CreateService(
        Mock<IUserIngredientRepository> inventoryRepoMock,
        Mock<IIngredientRepository> ingredientRepoMock,
        Mock<ILogger<InventoryService>> loggerMock,
        Mock<IRecipeRepository>? recipeRepoMock = null,
        Mock<IShoppingListRepository>? shoppingListRepoMock = null,
        Mock<INutritionService>? nutritionServiceMock = null,
        IMemoryCache? cache = null)
    {
        recipeRepoMock ??= new Mock<IRecipeRepository>();
        shoppingListRepoMock ??= new Mock<IShoppingListRepository>();
        nutritionServiceMock ??= new Mock<INutritionService>();

        var settings = Options.Create(new PantryChefSettings
        {
            Inventory = new InventorySettings
            {
                MinSearchLength = 2,
                DefaultAddQuantity = 100
            },
            Caching = new CachingSettings
            {
                AvailableIngredientsTtlMinutes = 30
            }
        });

        return new InventoryService(
            inventoryRepoMock.Object,
            ingredientRepoMock.Object,
            recipeRepoMock.Object,
            shoppingListRepoMock.Object,
            nutritionServiceMock.Object,
            loggerMock.Object,
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            settings);
    }
}
