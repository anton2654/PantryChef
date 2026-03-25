using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Web.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly INutritionService _nutritionService;

        public RecipeController(IRecipeService recipeService, INutritionService nutritionService)
        {
            _recipeService = recipeService;
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
                recipes = await _recipeService.GetAllRecipesWithIngredientsAsync();
            }
            else if (Enum.TryParse<DishCategory>(category, true, out var parsedCategory))
            {
                recipes = await _recipeService.GetRecipesByCategoryAsync(parsedCategory);
                selectedCategory = parsedCategory.ToString();
            }
            else
            {
                TempData["ErrorMessage"] = "Невідома категорія фільтра. Показано всі страви.";
                recipes = await _recipeService.GetAllRecipesWithIngredientsAsync();
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

            var recipe = await _recipeService.GetRecipeWithIngredientsByIdAsync(id);

            if (recipe == null)
            {
                return NotFound();
            }

            var nutrition = _nutritionService.CalculateNutrition(recipe);

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

            var result = await _nutritionService.UpdateRecipeNutritionAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "КБЖВ для рецепта успішно перераховано.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}