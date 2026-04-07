using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace PantryChef.Web.Controllers
{
    public class RecipeController : BaseController
    {
        private readonly IRecipeService _recipeService;
        private readonly INutritionService _nutritionService;
        private readonly PantryChefSettings _settings;

        public RecipeController(
            IRecipeService recipeService,
            INutritionService nutritionService,
            IOptions<PantryChefSettings> options)
        {
            _recipeService = recipeService;
            _nutritionService = nutritionService;
            _settings = options?.Value ?? new PantryChefSettings();
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

            var categories = _settings.RecipeFilter.Categories
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allCategoryLabel = string.IsNullOrWhiteSpace(_settings.RecipeFilter.AllCategoryLabel)
                ? "Всі страви"
                : _settings.RecipeFilter.AllCategoryLabel;

            var options = new List<RecipeCategoryOptionViewModel>
            {
                new() { Value = string.Empty, Label = allCategoryLabel, IsSelected = string.IsNullOrWhiteSpace(category) }
            };

            foreach (var cat in categories)
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
        [Authorize]
        public async Task<IActionResult> CalculateNutrition(int id)
        {
            if (id <= 0) return BadRequest();

            var result = await _nutritionService.UpdateRecipeNutritionAsync(id);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Details), new { id });
            }

            SetSuccessMessage("КБЖВ для рецепта успішно перераховано.");
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}