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
            return await Filter(null);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string category = null)
        {
            var recipes = string.IsNullOrWhiteSpace(category)
                ? await _recipeService.GetAllRecipesWithIngredientsAsync()
                : await _recipeService.GetRecipesByCategoryAsync(category);

            var dbCategories = new List<string> { "Сніданки", "Обіди", "Вечері", "Десерти", "Салати", "Гарніри", "Закуски", "Снеки", "Пісні страви", "Перші страви", "Другі страви" };

            var options = new List<RecipeCategoryOptionViewModel>
            {
                new() { Value = string.Empty, Label = "Всі страви", IsSelected = string.IsNullOrWhiteSpace(category) }
            };

            foreach (var cat in dbCategories)
            {
                options.Add(new RecipeCategoryOptionViewModel
                {
                    Value = cat,
                    Label = cat,
                    IsSelected = string.Equals(category, cat, StringComparison.OrdinalIgnoreCase)
                });
            }

            var model = new RecipeIndexViewModel
            {
                Recipes = recipes,
                SelectedCategory = category ?? string.Empty,
                Categories = options
            };

            return View(nameof(Index), model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            var recipe = await _recipeService.GetRecipeWithIngredientsByIdAsync(id);

            if (recipe == null) return NotFound();

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
            if (id <= 0) return BadRequest();

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