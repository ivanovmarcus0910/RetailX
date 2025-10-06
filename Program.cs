using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using RetailX.Auth;
using RetailX.Components;
using RetailX.Data;

namespace RetailX
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddScoped<AuthenticationStateProvider, SimpleAuthStateProvider>();
            builder.Services.AddScoped<IAuthService, SimpleAuthStateProvider>();
            builder.Services.AddAuthorizationCore();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    db.Database.CanConnect();
                    Console.WriteLine("✅ DB connected successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ DB connect error: {ex.Message}");
                }
            }
            app.Run();
           
        }
    }
}
