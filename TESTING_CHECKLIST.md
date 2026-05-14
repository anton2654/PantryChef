# 🧪 Перевірка всього функціоналу (Testing Checklist)

## 1️⃣ Локальна перевірка — Build & Tests

### 1.1 Перевірити, що проект білдиться без помилок
```powershell
cd c:\Users\loq\Documents\PantryChef
dotnet clean
dotnet restore PantryChef.sln
dotnet build PantryChef.sln --configuration Release
```
**Очікуємо:** ✅ Build успішний, немає errors.

### 1.2 Запустити всі тести локально
```powershell
dotnet test PantryChef.sln --configuration Release --verbosity detailed --logger "console;verbosity=detailed"
```
**Очікуємо:** ✅ Усі тести проходять (у колоні `PantryChef.Tests`).

---

## 2️⃣ GitHub Actions — CI/CD Pipeline

### 2.1 Перевірити статус GitHub Actions workflows

Відкрити репозиторій на GitHub → Actions і переглянути:
- **`Deploy PantryChef to Azure`** workflow
- **`SonarQube Cloud Analysis`** workflow

**Очікуємо:** ✅ Обидва workflows запускаються при push або PR.

### 2.2 Запустити workflow вручну (Manual Trigger)
1. Перейти на GitHub → Actions
2. Обрати `Deploy PantryChef to Azure` → "Run workflow" → Run
3. Дочекатися завершення

**Очікуємо:** ✅ Workflow завершено успішно (всі етапи green).

---

## 3️⃣ Azure Deployment

### 3.1 Перевірити Azure Web App та Database

```powershell
# Логін до Azure
az login

# Перегляд Web App статусу
az webapp list --output table

# Перегляд поточних конфігурацій Web App
az webapp config show -n pantrychef-web -g <YOUR_RESOURCE_GROUP>

# Перегляд логів Web App (останні 100 рядків)
az webapp log tail -n pantrychef-web -g <YOUR_RESOURCE_GROUP> --lines 100
```

**Очікуємо:** ✅ Web App работає (status: "Running"), логи без fatal errors.

### 3.2 Перевірити, що додаток розгорнутий та доступний

```powershell
# HTTP GET до Azure Web App
$url = "https://pantrychef-web.azurewebsites.net"
Invoke-WebRequest -Uri $url -Method Get -UseBasicParsing | Select-Object StatusCode, StatusDescription
```

**Очікуємо:** ✅ StatusCode: 200 OK.

---

## 4️⃣ Azure Key Vault Integration

### 4.1 Перевірити наявність секретів в Key Vault

```powershell
# Перегляд всіх секретів
az keyvault secret list --vault-name <YOUR_KEYVAULT_NAME> --output table

# Отримати конкретний секрет (без значення)
az keyvault secret show --vault-name <YOUR_KEYVAULT_NAME> -n "SqlConnectionString"
```

**Очікуємо:** ✅ Мінімум 1 секрет присутній (наприклад, `SqlConnectionString`).

### 4.2 Перевірити, що Web App читає секрети з Key Vault

1. Логіни до Azure Web App за допомогою SSH або через Azure Portal Console
2. Перевірити, що додаток читає `KeyVaultName` з конфігурації:

```powershell
# Через Azure Portal: App Service → Settings → Configuration
# Переглянути environment variables:
# - KeyVaultName
# - ASPNETCORE_ENVIRONMENT
```

**Очікуємо:** ✅ Конфігурація присутня та правильна.

---

## 5️⃣ SonarCloud Integration

### 5.1 Перевірити SonarCloud project та analysis

1. Перейти на https://sonarcloud.io
2. Логін та перегляд вашого проекту (`anton2654_PantryChef`)
3. Переглянути:
   - **Quality Gate Status** — повинна бути ✅ PASSED або 🔴 FAILED
   - **Code Smell, Bugs, Vulnerabilities** — наявність проблем

**Очікуємо:** ✅ Project аналізований, Quality Gate логічний (не занадто багато issues).

### 5.2 Перевірити SonarCloud в PR

1. Створити feature branch та відкрити PR на GitHub
2. Перечекати, поки `SonarQube Cloud Analysis` workflow запуститься
3. Переглянути PR комментарій від SonarCloud

**Очікуємо:** ✅ SonarCloud прокоментував PR з результатами аналізу (Quality Gate status).

---

## 6️⃣ Branch Protection Rules

### 6.1 Перевірити налаштування Branch Protection

1. Перейти на GitHub → Settings → Branches
2. Переглянути правила для гілки `main`:

**Мають бути налаштовані:**
- ✅ **Require pull request reviews** — мінімум 1 approval
- ✅ **Require status checks to pass before merging**:
  - `build-test-deploy` (GitHub Actions)
  - `sonarcloud / SonarQube Cloud Analysis` (SonarCloud)
- ✅ **Require linear history** (optional)
- ✅ **Require deployments to succeed before merging** (optional)

### 6.2 Тест: Спроба пушити напряму в main (має бути заблоковано)

```powershell
# Спробувати пушити напряму в main
git push origin main
```

**Очікуємо:** 🔴 Push заблоковано — "Updates were rejected because the tip of your current branch is behind its remote counterpart" або "protected branch".

### 6.3 Тест: Створити PR та перевірити умови merge

1. Створити feature branch:
```powershell
git checkout -b feature/test-pr
echo "test" >> test.txt
git add test.txt
git commit -m "Test PR"
git push origin feature/test-pr
```

2. Відкрити PR на GitHub

3. Перевірити:
   - 🔴 **Merge заблоковано** — поки немає approval та поки не успішні всі checks
   - ✅ GitHub Actions workflows запускаються автоматично
   - ✅ SonarCloud аналіз запускається

4. Затвердити PR (Approve):
```
1. Додати review "Approve"
2. Очекати, поки SonarCloud закінчить аналіз
```

5. Перевірити, що merge тепер можливий:
   - ✅ Merge button більше не disabled
   - ✅ Усі checks пройшли ✅

---

## 7️⃣ End-to-End Test Scenario

### Сценарій: Feature → Test → Deploy

**Крок 1: Розробка (Development)**
```powershell
# 1. Створити нову feature branch
git checkout -b feature/add-new-endpoint
# 2. Зробити зміни в коді
# 3. Локально запустити тести
dotnet test PantryChef.sln
# 4. Пушити
git push origin feature/add-new-endpoint
```

**Крок 2: PR Review (Code Review)**
- ✅ GitHub Actions: Build → Test → успішні
- ✅ SonarCloud: Analysis → Quality Gate результат присутній
- ✅ Code review: мінімум 1 approval
- ⚠️ Merge заборонений доки не виконані умови

**Крок 3: Merge**
```powershell
# На GitHub: Merge Pull Request → Confirm merge
```
**Очікуємо:** ✅ PR merged в main, feature branch видалена.

**Крок 4: Deployment (Automatic via CI/CD)**
- ✅ GitHub Actions `Deploy PantryChef to Azure` workflow запускається
- ✅ Білд + тести + публікація успішні
- ✅ Deploy на Azure успішний
- ✅ Додаток доступний на Azure Web App

**Крок 5: Verify**
```powershell
# Перевірити, що зміни в production
curl https://pantrychef-web.azurewebsites.net/api/new-endpoint
```

---

## 📋 Чек-ліст перевірки (Quick Reference)

- [ ] Локальна build успішна
- [ ] Локальні тести успішні
- [ ] GitHub Actions workflows запускаються
- [ ] Azure Web App доступна (200 OK)
- [ ] Key Vault налаштований та читається
- [ ] SonarCloud project існує й аналізується
- [ ] SonarCloud PR comments виглядають коректно
- [ ] Branch protection rules налаштовані
- [ ] PR merge вимагає 1 approval + успішні checks
- [ ] Automatic deployment на main працює

---

## 🆘 Якщо щось не працює

| Проблема | Що перевірити |
|----------|--------------|
| Build fails локально | `dotnet restore`, .NET 8.0 інстальований |
| Тести failять | Перевірити connection strings, DB миграції |
| GitHub Actions не запускаються | Перевірити `.github/workflows/*.yml`, secrets (SONAR_TOKEN, AZURE_WEBAPP_PUBLISH_PROFILE) |
| Azure deployment fails | Перевірити `AZURE_WEBAPP_PUBLISH_PROFILE` secret, Web App існує |
| SonarCloud не аналізує | Перевірити `SONAR_TOKEN` secret, проект ключ у workflows |
| Branch protection не працює | Перевірити GitHub Settings → Branches → main rule налаштування |

