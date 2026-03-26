using Microsoft.AspNetCore.Mvc;
using Moq;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Services;
using PantryChef.Data.Entities;
using PantryChef.Web.Controllers;
using PantryChef.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PantryChef.Tests
{
    public class FeatureTests
    {
        // RECIPE CONTROLLER

        [Fact]
        public async Task RecipeFilter_ShouldReturnViewWithRecipes_WhenCategoryIsProvided()
        {
            // Arrange
            var recipeServiceMock = new Mock<IRecipeService>();
            var nutritionServiceMock = new Mock<INutritionService>();
            var controller = new RecipeController(recipeServiceMock.Object, nutritionServiceMock.Object);
            
            string category = "Сніданки";
            var recipes = new List<Recipe> { new Recipe { Id = 1, Name = "Яєчня", Description = "Проста страва", Category = "Сніданки" } };
            
            recipeServiceMock.Setup(s => s.GetRecipesByCategoryAsync(category))
                .ReturnsAsync(recipes);

            var result = await controller.Filter(category);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecipeIndexViewModel>(viewResult.Model);
            Assert.Single(model.Recipes);
            Assert.Equal(category, model.SelectedCategory);
            Assert.Equal("Яєчня", model.Recipes.First().Name);
        }

        // INVENTORY CONTROLLER 

        [Fact]
        public async Task InventoryDetails_ShouldReturnView_WhenIngredientExistsInInventory()
        {
            // Arrange
            var inventoryServiceMock = new Mock<IInventoryService>();
            var controller = new InventoryController(inventoryServiceMock.Object);
            
            int ingredientId = 1;
            var inventory = new List<UserIngredient> 
            { 
                new UserIngredient 
                { 
                    IngredientId = 1, 
                    Quantity = 500, 
                    Ingredient = new Ingredient { Id = 1, Name = "Банан", Category = "Фрукти" } 
                } 
            };
            
            inventoryServiceMock.Setup(s => s.GetUserInventoryAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(inventory);

            var result = await controller.Details(ingredientId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IngredientDetailsViewModel>(viewResult.Model);
            Assert.Equal("Банан", model.Ingredient.Name);
            Assert.Equal(500, model.Quantity);
        }

        [Fact]
        public async Task InventoryDetails_ShouldReturnNotFound_WhenIngredientNotInInventory()
        {
            var inventoryServiceMock = new Mock<IInventoryService>();
            var controller = new InventoryController(inventoryServiceMock.Object);
            
            inventoryServiceMock.Setup(s => s.GetUserInventoryAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new List<UserIngredient>());

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // NUTRITION SERVICE

        [Fact]
        public void CalculateNutrition_ShouldSumValuesCorrectlyBasedOnWeight()
        {
            var service = new NutritionService(null, null); 
            
            var recipe = new Recipe
            {
                Name = "Тест",
                Description = "Тестовий рецепт",
                RecipeIngredients = new List<RecipeIngredient>
                {
                    // 200г (коефіцієнт 2.0)
                    new() { Quantity = 200, Ingredient = new Ingredient { Name = "Інгредієнт1", Category = "Категорія1", Calories = 100, Proteins = 10, Fats = 5, Carbohydrates = 20 } },
                    // 50г (коефіцієнт 0.5)
                    new() { Quantity = 50, Ingredient = new Ingredient { Name = "Інгредієнт2", Category = "Категорія2", Calories = 200, Proteins = 20, Fats = 10, Carbohydrates = 40 } }
                }
            };

            // Act
            var result = service.CalculateNutrition(recipe);

            // Assert
            // (100 * 2.0) + (200 * 0.5) = 200 + 100 = 300
            Assert.Equal(300, result.Calories);
            // (10 * 2.0) + (20 * 0.5) = 20 + 10 = 30
            Assert.Equal(30, result.Proteins);
            // (5 * 2.0) + (10 * 0.5) = 10 + 5 = 15
            Assert.Equal(15, result.Fats);
            // (20 * 2.0) + (40 * 0.5) = 40 + 20 = 60
            Assert.Equal(60, result.Carbohydrates);
        }
    }
}