using Microsoft.AspNetCore.Mvc;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Interfaces;
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

            var recipes = await _recipeRepo.GetAllRecipesWithIngredientsAsync();

            return View(recipes);
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