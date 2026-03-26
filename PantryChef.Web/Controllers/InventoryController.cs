using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using PantryChef.Web.Models;
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

            var model = new InventoryIndexViewModel
            {
                Inventory = inventory,
                SelectedCategory = category,
                AvailableCategories = categories
            };

            return View(model);
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