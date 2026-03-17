using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
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
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Додаток не зміг запуститися коректно");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

   

