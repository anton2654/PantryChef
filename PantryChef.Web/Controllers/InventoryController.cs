using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Interfaces;
using System.Threading.Tasks;

namespace PantryChef.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IUserIngredientRepository _inventoryRepo;
        private readonly IInventoryService _inventoryService;
        
        // Тимчасово Alice Smith
        private readonly int _currentUserId = 1;

        public InventoryController(
            IUserIngredientRepository inventoryRepo, 
            IInventoryService inventoryService)
        {
            _inventoryRepo = inventoryRepo;
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userInventory = await _inventoryRepo.GetUserInventoryAsync(_currentUserId);
            return View(userInventory);
        }
    }
}