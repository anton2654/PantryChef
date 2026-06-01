# PantryChef - Інтелектуальне керування продуктами та рецептами
HI!
**PantryChef** - це сучасний вебзастосунок, створений для ефективного управління домашніми запасами продуктів, мінімізації харчових відходів та автоматизованого підбору рецептів відповідно до дієтичних цілей користувача.

---

## Основна ідея

Проєкт вирішує проблему «що приготувати з того, що є в холодильнику», одночасно допомагаючи користувачам дотримуватися їхніх фітнес-цілей та контролювати калорійність раціону.

**Ключові можливості:**

- **Віртуальний холодильник:** повний контроль наявних інгредієнтів.
- **Smart-генератор рецептів:** підбір страв на основі залишків продуктів.
- **Контроль БЖВ:** автоматичний розрахунок калорій, білків, жирів та вуглеводів.
- **Персоналізація:** врахування алергій, обмежень та фітнес-цілей (схуднення/набір маси).

---

## Архітектура системи

Проєкт реалізований із чітким розділенням відповідальностей (Separation of Concerns) і складається з 4-х основних модулів:

### 1. Identity System (Безпека та профілі)

Керує автентифікацією та персональними даними.

- Реєстрація, логін та відновлення доступу.
- Налаштування профілю: цільова вага, добова норма калорій.
- Менеджмент дієтичних обмежень та алергій.

### 2. Inventory Management (Управління запасами)

Центральний вузол для роботи з продуктами.

- Операції CRUD для продуктів у вашому «холодильнику».
- Підтримка різних одиниць виміру (г, мл, шт).
- Автоматичне списання інгредієнтів після вибору рецепта.

### 3. Meal Generation & Analysis (Інтелектуальний підбір)

Алгоритмічна логіка порівняння ресурсів із базою рецептів.

- Категоризація: «Можна приготувати зараз» vs «Майже готові» (якщо бракує 1-2 інгредієнтів).
- Фільтрація за категоріями та складністю.

### 4. Nutrition Calculation (Аналітика)

Модуль для детального розрахунку харчової цінності.

- Аналіз енергетичної цінності кожної страви.
- Порівняння фактичного споживання з денною нормою (Goal Match).

---

## Секретні налаштування (secrets.json)

Чутливі дані (наприклад, `ConnectionStrings:DefaultConnection`) не повинні зберігатися у `appsettings.json`.
Для локальної розробки використовуйте User Secrets (файл `secrets.json` поза репозиторієм).

Приклад налаштування:

```powershell
dotnet user-secrets init --project PantryChef.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=PantryChefDb;Username=postgres;Password=your-password" --project PantryChef.Web
dotnet user-secrets list --project PantryChef.Web
```

У `Program.cs` підключено читання User Secrets для локальних запусків, а також додано перевірку, що `DefaultConnection` присутній у конфігурації.

---

## Середовища ASP.NET Core (Development, Staging, Production)

У проєкті використано стандартний механізм конфігурації ASP.NET Core:

- `appsettings.json` - базові значення для всіх середовищ.
- `appsettings.Development.json` - оверрайди для Development.
- `appsettings.Staging.json` - оверрайди для Staging.
- `appsettings.Production.json` - оверрайди для Production.

Відмінні значення для `PantryChefSettings`:

- Development: `DefaultPageSize=8`, `DefaultAddQuantity=120.0`, `MinSearchLength=1`.
- Staging: `DefaultPageSize=10`, `DefaultAddQuantity=75.0`, `MinSearchLength=2`.
- Production: `DefaultPageSize=20`, `DefaultAddQuantity=50.0`, `MinSearchLength=3`.

### Запуск з потрібним середовищем (PowerShell)

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project PantryChef.Web

$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet run --project PantryChef.Web

$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project PantryChef.Web
```

Пояснення:

- Для локальної розробки рядок підключення читається з User Secrets.
- Для прод-розгортання використовуйте змінні середовища або менеджер секретів (наприклад, Azure Key Vault), а не `secrets.json`.


CI/CD and SonarQube Cloud are configured for this project.

---

## Структура проєкту

```
PantryChef/
|
|-- PantryChef.Web/                        ← Веб-шар (MVC, Razor Views)
|   |-- Controllers/
|   |   |-- BaseController.cs              ← Базовий контролер (CurrentUserId)
|   |   |-- AccountController.cs           ← Реєстрація, логін, відновлення паролю
|   |   |-- HomeController.cs              ← Головна сторінка
|   |   |-- InventoryController.cs         ← Управління коморою
|   |   |-- RecipeController.cs            ← Рецепти, планувальник, список покупок
|   |   |-- ProfileController.cs           ← Профіль користувача
|   |   └-- NotificationController.cs      ← Сповіщення
|   |
|   |-- Views/
|   |   |-- Account/                       ← Register, Login, ForgotPassword, ResetPassword
|   |   |-- Home/                          ← Index, Privacy
|   |   |-- Inventory/                     ← Index, Details
|   |   |-- Recipe/                        ← Index, Details, Create, Edit, Delete, Planner, ShoppingList
|   |   |-- Profile/                       ← Index
|   |   └-- Shared/                        ← _Layout, Error, _ValidationScriptsPartial
|   |
|   |-- Models/                            ← ViewModels для кожного контролера
|   |-- Middleware/                        ← ExceptionMiddleware, RequestLoggingMiddleware, RequestTimingMiddleware
|   |-- Filters/                           ← RateLimitAttribute
|   |-- Hubs/                              ← NotificationHub (SignalR)
|   |-- Services/                          ← NotificationBackgroundService (hosted service)
|   |-- Clients/                           ← MealDbClient (TheMealDB API)
|   |-- wwwroot/
|   |   |-- css/                           ← site.css, components.css, notifications.css
|   |   |-- js/                            ← site.js, notifications.js, mealdb-create.js
|   |   └-- lib/                           ← Bootstrap, jQuery
|   |
|   |-- Program.cs                         ← DI реєстрація, middleware, конфігурація
|   |-- appsettings.json
|   |-- appsettings.Development.json
|   |-- appsettings.Staging.json
|   └-- appsettings.Production.json
|
|-- PantryChef.Business/                   ← Бізнес-логіка
|   |-- Interfaces/
|   |   |-- IRecipeService.cs
|   |   |-- IInventoryService.cs
|   |   |-- IAccountService.cs
|   |   |-- IProfileService.cs
|   |   └-- INutritionService.cs
|   |
|   |-- Services/
|   |   |-- RecipeService.cs               ← Підбір рецептів (повний/частковий збіг)
|   |   |-- InventoryService.cs            ← Управління коморою, списання інгредієнтів
|   |   |-- AccountService.cs              ← Реєстрація, зв'язок Identity ↔ User
|   |   |-- ProfileService.cs              ← Цілі профілю, фізичні параметри
|   |   └-- NutritionService.cs            ← Розрахунок калорій і макросів
|   |
|   └-- Models/
|       |-- Result.cs                      ← Обгортка результату (IsSuccess, Data, ErrorMessage)
|       |-- PantryChefSettings.cs          ← POCO для appsettings.json
|       |-- RecipeMatchModels.cs           ← RecipeMatchResult, IngredientDeficit
|       |-- RecipeManagementModels.cs      ← RecipeCreateModel, RecipeEditModel
|       └-- UserProfileModels.cs           ← DTO профілю
|
|-- PantryChef.Data/                       ← Шар доступу до даних
|   |-- Entities/
|   |   |-- ApplicationUser.cs             ← ASP.NET Identity користувач
|   |   |-- User.cs                        ← Доменний користувач (цілі, параметри)
|   |   |-- Ingredient.cs                  ← Каталог інгредієнтів (макроси, категорія)
|   |   |-- Recipe.cs                      ← Рецепт (опис, харчова цінність, категорія)
|   |   |-- RecipeIngredient.cs            ← Зв'язок рецепт ↔ інгредієнт + кількість
|   |   |-- UserIngredient.cs              ← Комора користувача
|   |   |-- ShoppingListItem.cs            ← Список покупок
|   |   |-- UserNutritionLog.cs            ← Щоденний журнал харчування
|   |   |-- UserRecipe.cs                  ← Збережені рецепти користувача
|   |   └-- SystemNotification.cs          ← Системні сповіщення
|   |
|   |-- Context/
|   |   └-- PantryChefDbContext.cs         ← EF Core DbContext, seed-дані
|   |
|   |-- Interfaces/
|   |   |-- IRepository.cs                 ← Базовий generic-інтерфейс
|   |   |-- IRecipeRepository.cs
|   |   |-- IIngredientRepository.cs
|   |   |-- IUserRepository.cs
|   |   |-- IUserIngredientRepository.cs
|   |   |-- IUserRecipeRepository.cs
|   |   |-- IShoppingListRepository.cs
|   |   └-- IUserNutritionLogRepository.cs
|   |
|   |-- Repositories/
|   |   |-- Repository.cs                  ← Базовий generic-репозиторій (CRUD)
|   |   |-- RecipeRepository.cs
|   |   |-- IngredientRepository.cs
|   |   |-- UserRepository.cs
|   |   |-- UserIngredientRepository.cs
|   |   |-- UserRecipeRepository.cs
|   |   |-- ShoppingListRepository.cs
|   |   └-- UserNutritionLogRepository.cs
|   |
|   └-- Migrations/                        ← EF Core міграції
|
|-- PantryChef.Tests/                      ← xUnit тести з Moq
|   |-- AccountControllerRegisterTests.cs
|   |-- AccountControllerLoginTests.cs
|   |-- RecipeControllerTests.cs
|   |-- RecipeServiceTests.cs
|   |-- InventoryServiceTests.cs
|   |-- InventoryAndRecipeControllerTests.cs
|   |-- ProfileControllerTests.cs
|   |-- ProfileServiceTests.cs
|   |-- NutritionServiceTests.cs
|   |-- RateLimitAttributeTests.cs
|   └-- FeatureTests.cs
|
|-- .github/
|   └-- workflows/
|       |-- azure-deploy.yml               ← Build → Test → Deploy на Azure (тільки push до main)
|       └-- sonarcloud.yml                 ← Аналіз якості коду SonarQube Cloud
|
└-- PantryChef.sln
```