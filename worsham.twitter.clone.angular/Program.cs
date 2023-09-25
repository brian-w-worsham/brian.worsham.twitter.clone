using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace worsham.twitter.clone.angular
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddUserSecrets<Program>();

            // Add services to the container.
            var connectionString =
                builder.Configuration.GetConnectionString("TwitterCloneConnectionString")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found."
                );

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowOrigin",
                    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Authorization")
                );
            });

            builder.Services.AddDbContext<TwitterCloneContext>(
                options => options.UseSqlServer(connectionString)
            );
            //builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // var secretKey = GenerateRandomString(64);

            // retrieve SecretKeyForJwtToken from secrets.json
            string secretKeyForJwtToken = builder.Configuration["SecretKeyForJwtToken"] ?? throw new InvalidOperationException("SecretKeyForJwtToken not found in configuration.");

            string validIssuer = builder.Configuration["ValidIssuer"] ?? throw new InvalidOperationException("ValidIssuer not found in configuration.");
            string validAudience = builder.Configuration["ValidAudience"] ?? throw new InvalidOperationException("ValidAudience not found in configuration.");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = validIssuer,
                        ValidAudience = validAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyForJwtToken))
                    };
                });

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();



            app.UseSession();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");

            app.MapFallbackToFile("index.html");

            app.Run();

            string GenerateRandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var random = new Random();
                var result = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                {
                    result.Append(chars[random.Next(chars.Length)]);
                }

                return result.ToString();
            }
        }
    }
}
