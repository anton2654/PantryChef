using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using System.Threading.Tasks;

namespace PantryChef.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        
        // Тимчасово Alice Smith
        private readonly int _currentUserId = 1;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userInventory = await _inventoryService.GetUserInventoryAsync(_currentUserId);
            return View(userInventory);
        }
    }
}