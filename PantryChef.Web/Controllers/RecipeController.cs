using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            return await Filter(null, 1);
        }

        [HttpGet]
        public async Task<IActionResult> Filter(string category = null, int page = 1)
        {
            var recipes = string.IsNullOrWhiteSpace(category)
                ? await _recipeService.GetAllRecipesWithIngredientsAsync()
                : await _recipeService.GetRecipesByCategoryAsync(category);

            var allRecipes = (recipes ?? Enumerable.Empty<Recipe>()).ToList();
            var pageSize = _settings.Pagination.DefaultPageSize > 0 ? _settings.Pagination.DefaultPageSize : 12;
            var totalItems = allRecipes.Count;
            var totalPages = totalItems == 0
                ? 0
                : (int)Math.Ceiling(totalItems / (double)pageSize);

            var currentPage = page < 1 ? 1 : page;
            if (totalPages > 0 && currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            var pagedRecipes = totalItems == 0
                ? new List<Recipe>()
                : allRecipes
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

            var categories = ((await _recipeService.GetAvailableCategoriesAsync()) ?? Enumerable.Empty<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!categories.Any())
            {
                categories = _settings.RecipeFilter.Categories
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .Distinct(System.StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

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
                Recipes = pagedRecipes,
                SelectedCategory = category ?? string.Empty,
                Categories = options,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(nameof(Index), model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var model = new RecipeCreateViewModel();
            await PopulateAvailableCategoriesAsync(model);
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecipeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAvailableCategoriesAsync(model);
                return View(model);
            }

            var result = await _recipeService.AddRecipeAsync(new RecipeCreateModel
            {
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                Photo = model.Photo,
                Calories = model.Calories,
                Proteins = model.Proteins,
                Fats = model.Fats,
                Carbohydrates = model.Carbohydrates
            });

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                await PopulateAvailableCategoriesAsync(model);
                return View(model);
            }

            SetSuccessMessage("Страву успішно додано.");
            return RedirectToAction(nameof(Details), new { id = result.Data });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var result = await _recipeService.GetRecipeForEditAsync(id);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            var model = new RecipeEditViewModel
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Description = result.Data.Description,
                Category = result.Data.Category,
                Photo = result.Data.Photo,
                Calories = result.Data.Calories,
                Proteins = result.Data.Proteins,
                Fats = result.Data.Fats,
                Carbohydrates = result.Data.Carbohydrates
            };

            await PopulateAvailableCategoriesAsync(model);
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RecipeEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAvailableCategoriesAsync(model);
                return View(model);
            }

            var result = await _recipeService.EditRecipeAsync(new RecipeEditModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Category = model.Category,
                Photo = model.Photo,
                Calories = model.Calories,
                Proteins = model.Proteins,
                Fats = model.Fats,
                Carbohydrates = model.Carbohydrates
            });

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                await PopulateAvailableCategoriesAsync(model);
                return View(model);
            }

            SetSuccessMessage("Страву успішно оновлено.");
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var result = await _recipeService.GetRecipeForDeleteAsync(id);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            return View(new RecipeDeleteViewModel
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                Category = result.Data.Category
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _recipeService.DeleteRecipeAsync(id);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            SetSuccessMessage("Страву успішно видалено.");
            return RedirectToAction(nameof(Index));
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

        private async Task PopulateAvailableCategoriesAsync(RecipeCreateViewModel model)
        {
            var categories = ((await _recipeService.GetAvailableCategoriesAsync()) ?? Enumerable.Empty<string>())
                .ToList();

            if (categories.Count == 0)
            {
                categories = _settings.RecipeFilter.Categories
                    .Where(category => !string.IsNullOrWhiteSpace(category))
                    .Select(category => category.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            var options = categories
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Select(category => category.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(category => category)
                .Select(category => new SelectListItem
                {
                    Value = category,
                    Text = category,
                    Selected = string.Equals(model.Category, category, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(model.Category)
                && !options.Any(option => string.Equals(option.Value, model.Category, StringComparison.OrdinalIgnoreCase)))
            {
                options.Insert(0, new SelectListItem
                {
                    Value = model.Category,
                    Text = model.Category,
                    Selected = true
                });
            }

            model.AvailableCategories = options;
        }
    }
}