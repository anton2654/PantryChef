using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryChef.Business.Models;
using PantryChef.Business.Services;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;

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
        var service = CreateService(inventoryRepoMock, ingredientRepoMock, loggerMock, cache);

        var firstResult = (await service.GetAvailableIngredientsAsync()).ToList();
        var secondResult = (await service.GetAvailableIngredientsAsync()).ToList();

        Assert.Equal(2, firstResult.Count);
        Assert.Equal(2, secondResult.Count);
        Assert.Equal("Банан", firstResult[0].Name);
        ingredientRepoMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    private static InventoryService CreateService(
        Mock<IUserIngredientRepository> inventoryRepoMock,
        Mock<IIngredientRepository> ingredientRepoMock,
        Mock<ILogger<InventoryService>> loggerMock,
        IMemoryCache? cache = null)
    {
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
            loggerMock.Object,
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            settings);
    }
}
