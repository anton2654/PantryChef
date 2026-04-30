using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PantryChef.Web.Clients
{
    public class MealDbClient : IMealDbClient
    {
        private readonly HttpClient _httpClient;

        public MealDbClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<MealDbAreaItem>> GetAreasAsync(CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetFromJsonAsync<MealDbAreasResponse>("list.php?a=list", cancellationToken);
            var items = response?.Meals ?? new List<MealDbAreaItem>();

            return items
                .Where(item => !string.IsNullOrWhiteSpace(item.StrArea))
                .OrderBy(item => item.StrCountry)
                .ThenBy(item => item.StrArea)
                .ToList();
        }

        public async Task<IReadOnlyList<MealDbMealItem>> GetMealsByAreaAsync(string area, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(area))
            {
                return Array.Empty<MealDbMealItem>();
            }

            var uri = $"filter.php?a={Uri.EscapeDataString(area)}";
            var response = await _httpClient.GetFromJsonAsync<MealDbMealsResponse>(uri, cancellationToken);
            var items = response?.Meals ?? new List<MealDbMealItem>();

            return items
                .Where(item => !string.IsNullOrWhiteSpace(item.IdMeal) && !string.IsNullOrWhiteSpace(item.StrMeal))
                .OrderBy(item => item.StrMeal)
                .ToList();
        }

        public async Task<string> GetMealInstructionsAsync(string mealId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(mealId))
            {
                return string.Empty;
            }

            var uri = $"lookup.php?i={Uri.EscapeDataString(mealId)}";
            var response = await _httpClient.GetFromJsonAsync<MealDbLookupResponse>(uri, cancellationToken);
            var instructions = response?.Meals?.FirstOrDefault()?.StrInstructions;

            return instructions?.Trim() ?? string.Empty;
        }
    }
}
