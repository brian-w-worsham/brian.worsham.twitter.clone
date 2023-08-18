using brian.worsham.twitter.clone2.Data;
using brian.worsham.twitter.clone2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace brian.worsham.twitter.clone2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("TwitterCloneConnectionString") ?? throw new InvalidOperationException("Connection string 'TwitterCloneConnectionString' not found.");

            builder.Services.AddDbContext<TwitterCloneContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;

                // Configure password options
                options.Password.RequireDigit = false;           // No requirement for a number
                options.Password.RequiredLength = 8;             // Minimum length of 8 characters
                options.Password.RequiredUniqueChars = 6;        // Number of unique characters required (special, lowercase, uppercase)
                options.Password.RequireLowercase = true;        // Require at least one lowercase letter
                options.Password.RequireUppercase = true;        // Require at least one uppercase letter
                options.Password.RequireNonAlphanumeric = true;  // Require at least one special character
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
            }
            else
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
            app.MapRazorPages();

            app.Run();
        }
    }
}
