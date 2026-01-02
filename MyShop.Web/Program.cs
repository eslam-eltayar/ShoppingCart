using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess.Data;
using MyShop.DataAccess.Repositories.Imp;
using MyShop.Entities.Repositories;
using Microsoft.AspNetCore.Identity;
using MyShop.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using MyShop.Web.Services;

namespace MyShop.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(4))
                            .AddDefaultTokenProviders().AddDefaultUI()
                            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddSingleton<IEmailSender, EmailSender>();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Session Cart Service
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ISessionCartService, SessionCartService>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.MapControllerRoute(
                name: "customer",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "admin",
                pattern: "{area=Admin}/{controller=Dashboard}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
