using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PantryChef.Data.Repositories
{
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(PantryChefDbContext context) : base(context)
        {
        }

        public async Task AddRecipeAsync(Recipe recipe)
        {
            await AddAsync(recipe);
        }

        public void UpdateRecipe(Recipe recipe)
        {
            Update(recipe);
        }

        public void DeleteRecipe(Recipe recipe)
        {
            Delete(recipe);
        }

        public async Task<Recipe> GetRecipeByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(recipe => recipe.Id == id);
        }

        public async Task<IEnumerable<string>> GetAvailableCategoriesAsync()
        {
            return await _dbSet
                .Where(recipe => !string.IsNullOrWhiteSpace(recipe.Category))
                .Select(recipe => recipe.Category)
                .Distinct()
                .OrderBy(category => category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync()
        {
            return await _dbSet
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .ToListAsync();
        }

        public async Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category)
        {
            return await _dbSet
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.Category == category)
                .ToListAsync();
        }
    }
}