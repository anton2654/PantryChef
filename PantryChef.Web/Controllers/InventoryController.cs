using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;


namespace PantryChef.Web.Controllers
{
    [Authorize]
    public class InventoryController : BaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly PantryChefSettings _settings;
        private readonly int _currentUserId = 1; // Тимчасово Alice Smith

        public InventoryController(IInventoryService inventoryService, IOptions<PantryChefSettings> options)
        {
            _inventoryService = inventoryService;
            _settings = options?.Value ?? new PantryChefSettings();
        }

        [HttpGet]
        public async Task<IActionResult> Index(string category = null, string searchQuery = null)
        {
            var inventory = await _inventoryService.GetUserInventoryAsync(CurrentUserId, category, searchQuery);
            var categories = await _inventoryService.GetUserInventoryCategoriesAsync(CurrentUserId);
            var ingredients = await _inventoryService.GetAvailableIngredientsAsync();

            var model = new InventoryIndexViewModel
            {
                Inventory = inventory ?? Enumerable.Empty<UserIngredient>(),
                SelectedCategory = category,
                SearchQuery = searchQuery, 
                AvailableCategories = categories ?? Enumerable.Empty<string>(),
                AvailableIngredients = ingredients ?? Enumerable.Empty<Ingredient>(),
                AddQuantity = _settings.Inventory.DefaultAddQuantity,
                MinSearchLength = _settings.Inventory.MinSearchLength
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIngredient(int ingredientId, double quantity)
        {
            var result = await _inventoryService.AddOrUpdateIngredientAsync(CurrentUserId, ingredientId, quantity);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            SetSuccessMessage("Інгредієнт успішно додано до холодильника."); 
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            var inventory = await _inventoryService.GetUserInventoryAsync(_currentUserId);
            
            var item = inventory.FirstOrDefault(i => i.IngredientId == id);

            if (item == null) return NotFound();

            var model = new IngredientDetailsViewModel
            {
                Ingredient = item.Ingredient,
                Quantity = item.Quantity
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int ingredientId, double quantity)
        {
            var result = await _inventoryService.UpdateIngredientQuantityAsync(_currentUserId, ingredientId, quantity);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Кількість інгредієнта успішно оновлено.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveIngredient(int ingredientId)
        {
            var result = await _inventoryService.RemoveIngredientAsync(_currentUserId, ingredientId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Інгредієнт видалено з холодильника.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}