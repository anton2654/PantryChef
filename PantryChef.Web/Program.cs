using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using Serilog;

namespace PantryChef.Web
{ 
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341") 
                .CreateLogger();

            try
            {
                Log.Information("Запуск веб-додатка PantryChef...");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                builder.Services.AddControllersWithViews();

                builder.Services.AddDbContext<PantryChefDbContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services
                    .AddIdentity<ApplicationUser, IdentityRole>(options =>
                    {
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireUppercase = true;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequiredLength = 8;

                        options.User.RequireUniqueEmail = true;
                    })
                    .AddEntityFrameworkStores<PantryChefDbContext>()
                    .AddDefaultTokenProviders();

                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                });

                builder.Services.AddScoped<PantryChef.Data.Interfaces.IRecipeRepository, PantryChef.Data.Repositories.RecipeRepository>();

                builder.Services.AddScoped<PantryChef.Business.Interfaces.INutritionService, PantryChef.Business.Services.NutritionService>();

                var app = builder.Build();

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

   

