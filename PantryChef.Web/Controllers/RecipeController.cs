using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using PantryChef.Web.Models;
using System;
using System.Collections.Generic;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe); 
        }

        [HttpPost]
        public async Task<IActionResult> CalculateNutrition(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _nutritionService.UpdateRecipeNutritionAsync(id);

            TempData["SuccessMessage"] = "КБЖВ для рецепта успішно перераховано.";

            return RedirectToAction(nameof(Index));
        }
    }
}