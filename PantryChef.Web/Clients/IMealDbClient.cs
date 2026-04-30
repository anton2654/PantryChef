using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PantryChef.Web.Clients
{
    public interface IMealDbClient
    {
        Task<IReadOnlyList<MealDbAreaItem>> GetAreasAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MealDbMealItem>> GetMealsByAreaAsync(string area, CancellationToken cancellationToken = default);

        Task<string> GetMealInstructionsAsync(string mealId, CancellationToken cancellationToken = default);
    }
}
