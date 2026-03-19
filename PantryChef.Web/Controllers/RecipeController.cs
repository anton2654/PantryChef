using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using PantryChef.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PantryChef.Web.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IRecipeRepository _recipeRepo;
        private readonly INutritionService _nutritionService;

        public RecipeController(IRecipeRepository recipeRepo, INutritionService nutritionService)
        {
            _recipeRepo = recipeRepo;
            _nutritionService = nutritionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return await Filter();
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string category = null)
        {
            var selectedCategory = string.Empty;
            IEnumerable<Recipe> recipes;

            if (string.IsNullOrWhiteSpace(category))
            {
                recipes = await _recipeRepo.GetAllRecipesWithIngredientsAsync();
            }
            else if (Enum.TryParse<DishCategory>(category, true, out var parsedCategory))
            {
                recipes = await _recipeRepo.GetRecipesByCategoryAsync(parsedCategory);
                selectedCategory = parsedCategory.ToString();
            }
            else
            {
                TempData["ErrorMessage"] = "Невідома категорія фільтра. Показано всі страви.";
                recipes = await _recipeRepo.GetAllRecipesWithIngredientsAsync();
            }

            var model = new RecipeIndexViewModel
            {
                Recipes = recipes,
                SelectedCategory = selectedCategory,
                Categories = BuildCategoryOptions(selectedCategory)
            };

            return View(nameof(Index), model);
        }

        private static IEnumerable<RecipeCategoryOptionViewModel> BuildCategoryOptions(string selectedCategory)
        {
            var options = new List<RecipeCategoryOptionViewModel>
            {
                new()
                {
                    Value = string.Empty,
                    Label = "Всі",
                    IsSelected = string.IsNullOrWhiteSpace(selectedCategory)
                }
            };

            foreach (var category in Enum.GetValues<DishCategory>())
            {
                var value = category.ToString();

                options.Add(new RecipeCategoryOptionViewModel
                {
                    Value = value,
                    Label = GetCategoryLabel(category),
                    IsSelected = string.Equals(selectedCategory, value, StringComparison.OrdinalIgnoreCase)
                });
            }

            return options;
        }

        private static string GetCategoryLabel(DishCategory category)
        {
            return category switch
            {
                DishCategory.Breakfast => "Сніданок",
                DishCategory.Lunch => "Обід",
                DishCategory.Dinner => "Вечеря",
                DishCategory.Snack => "Перекус",
                _ => category.ToString()
            };
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(id);

            if (recipe == null)
            {
                return NotFound();
            }

            var nutrition = CalculateNutrition(recipe);

            var model = new RecipeDetailsViewModel
            {
                Recipe = recipe,
                Calories = nutrition.Calories,
                Proteins = nutrition.Proteins,
                Fats = nutrition.Fats,
                Carbohydrates = nutrition.Carbohydrates
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CalculateNutrition(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            await _nutritionService.UpdateRecipeNutritionAsync(id);

            TempData["SuccessMessage"] = "КБЖВ для рецепта успішно перераховано.";

            return RedirectToAction(nameof(Details), new { id });
        }

        private static (double Calories, double Proteins, double Fats, double Carbohydrates) CalculateNutrition(Recipe recipe)
        {
            if (recipe.RecipeIngredients == null || recipe.RecipeIngredients.Count == 0)
            {
                return (
                    Math.Round(recipe.Calories, 1),
                    Math.Round(recipe.Proteins, 1),
                    Math.Round(recipe.Fats, 1),
                    Math.Round(recipe.Carbohydrates, 1));
            }

            var calories = recipe.RecipeIngredients.Sum(item => item.Ingredient.Calories * (item.Quantity / 100.0));
            var proteins = recipe.RecipeIngredients.Sum(item => item.Ingredient.Proteins * (item.Quantity / 100.0));
            var fats = recipe.RecipeIngredients.Sum(item => item.Ingredient.Fats * (item.Quantity / 100.0));
            var carbohydrates = recipe.RecipeIngredients.Sum(item => item.Ingredient.Carbohydrates * (item.Quantity / 100.0));

            return (
                Math.Round(calories, 1),
                Math.Round(proteins, 1),
                Math.Round(fats, 1),
                Math.Round(carbohydrates, 1));
        }
    }
}