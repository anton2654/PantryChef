using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using Serilog;
using PantryChef.Business.Models;

namespace PantryChef.Web
{ 
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true, reloadOnChange: true);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration) 
                .CreateLogger();

            try
            {
                Log.Information("Запуск веб-додатка PantryChef...");


                builder.Host.UseSerilog();

                builder.Services.AddControllersWithViews();

                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Не знайдено ConnectionStrings:DefaultConnection. " +
                        "Додайте секрет командою: " +
                        "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<your-connection-string>\" --project PantryChef.Web");
                }

                builder.Services.AddDbContext<PantryChefDbContext>(options =>
                    options.UseNpgsql(connectionString));

                builder.Services.Configure<PantryChefSettings>(builder.Configuration.GetSection("PantryChefSettings"));

                builder.Services
                    .AddIdentity<ApplicationUser, IdentityRole>(options =>
                    {
                        builder.Configuration.GetSection("IdentitySettings").Bind(options);
                    })
                    .AddEntityFrameworkStores<PantryChefDbContext>()
                    .AddDefaultTokenProviders();

                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = builder.Configuration["CookieSettings:LoginPath"];
                    options.AccessDeniedPath = builder.Configuration["CookieSettings:AccessDeniedPath"];
                });

                builder.Services.AddScoped<PantryChef.Data.Interfaces.IRecipeRepository, PantryChef.Data.Repositories.RecipeRepository>();
                builder.Services.AddScoped<PantryChef.Business.Interfaces.INutritionService, PantryChef.Business.Services.NutritionService>();
                builder.Services.AddScoped<PantryChef.Business.Interfaces.IRecipeService, PantryChef.Business.Services.RecipeService>();
                builder.Services.AddScoped<PantryChef.Business.Interfaces.IAccountService, PantryChef.Business.Services.AccountService>();

                builder.Services.AddScoped<PantryChef.Data.Interfaces.IUserIngredientRepository, PantryChef.Data.Repositories.UserIngredientRepository>();
                builder.Services.AddScoped<PantryChef.Data.Interfaces.IIngredientRepository, PantryChef.Data.Repositories.IngredientRepository>();
                builder.Services.AddScoped<PantryChef.Business.Interfaces.IInventoryService, PantryChef.Business.Services.InventoryService>();

             
                
                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<PantryChefDbContext>();
                    dbContext.Database.Migrate();
                }
                
                app.UseMiddleware<PantryChef.Web.Middleware.ExceptionMiddleware>();

                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.Run();
            }
            catch (HostAbortedException)
            {
                // Expected during EF Core design-time host creation.
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критична помилка під час запуску програми");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

   

