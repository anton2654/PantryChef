using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;

namespace PantryChef.Data.Repositories
{
    public class IngredientRepository : Repository<Ingredient>, IIngredientRepository
    {
        public IngredientRepository(PantryChefDbContext context) : base(context)
        {
        }
    }
}