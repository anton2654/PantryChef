using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace PantryChef.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly int _currentUserId = 1; // Тимчасово Alice Smith

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string category = null)
        {
            var inventory = await _inventoryService.GetUserInventoryAsync(_currentUserId, category);
            var categories = await _inventoryService.GetUserInventoryCategoriesAsync(_currentUserId);
            var ingredients = await _inventoryService.GetAvailableIngredientsAsync();

            var model = new InventoryIndexViewModel
            {
                Inventory = inventory ?? Enumerable.Empty<UserIngredient>(),
                SelectedCategory = category,
                AvailableCategories = categories ?? Enumerable.Empty<string>(),
                AvailableIngredients = ingredients ?? Enumerable.Empty<Ingredient>(),
                AddQuantity = 100
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIngredient(int ingredientId, double quantity)
        {
            var result = await _inventoryService.AddOrUpdateIngredientAsync(_currentUserId, ingredientId, quantity);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Інгредієнт успішно додано до холодильника.";
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

    }
}