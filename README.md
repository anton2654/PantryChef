# PantryChef - Інтелектуальне керування продуктами та рецептами

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
