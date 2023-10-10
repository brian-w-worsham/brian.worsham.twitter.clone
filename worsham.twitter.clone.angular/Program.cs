using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using worsham.twitter.clone.angular.Models.EntityModels;
using worsham.twitter.clone.angular.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using System.Text;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;

namespace worsham.twitter.clone.angular
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddUserSecrets<Program>();

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Listen(IPAddress.Any, 5140); // HTTP
                serverOptions.Listen(IPAddress.Any, 7232, listenOptions => // HTTPS
                {
                    var password = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");
                    var pathToCertificate = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
                    var certificate = new X509Certificate2(pathToCertificate, password);
                    listenOptions.UseHttps(certificate);
                });
            });

            // Add services to the container.

            var connectionString = Environment.GetEnvironmentVariable("TwitterCloneConnectionString")
                       ?? builder.Configuration.GetConnectionString("TwitterCloneConnectionString")
                ?? throw new InvalidOperationException(
                    "Connection string 'TwitterCloneConnectionString' not found."
                );

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowOrigin",
                    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Authorization")
                );
            });

            builder.Services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
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

            builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("/keys/")).SetDefaultKeyLifetime(TimeSpan.FromDays(9999));

            // retrieve SecretKeyForJwtToken from secrets.json
            string secretKeyForJwtToken = builder.Configuration["SecretKeyForJwtToken"] ?? Environment.GetEnvironmentVariable("TwitterCloneConnectionString") ?? throw new InvalidOperationException("SecretKeyForJwtToken not found in configuration or environment variables.");

            string validIssuer = builder.Configuration["ValidIssuer"] ?? Environment.GetEnvironmentVariable("ValidIssuer") ?? throw new InvalidOperationException("ValidIssuer not found in configuration or environment variables.");
            string validAudience = builder.Configuration["ValidAudience"] ?? Environment.GetEnvironmentVariable("ValidAudience") ?? throw new InvalidOperationException("ValidAudience not found in configuration or environment variables.");

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

            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "index.html" },
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "dist"))
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "dist")),
                RequestPath = ""
            });

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"After middleware: {context.Request.Path} - StatusCode: {context.Response.StatusCode}");

                if (context.Request.Path.Value == "/")
                {
                    context.Request.Path = "/index.html";
                    await next();
                }
                else
                {
                    await next();

                    if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value) && !context.Request.Path.Value.StartsWith("/api/"))
                    {
                        context.Request.Path = "/index.html";
                        await next();
                    }
                }
            });

            app.MapGet("/", context =>
            {
                context.Request.Path = "/index.html";
                return context.Response.SendFileAsync("ClientApp/dist/index.html");
            });

            app.UseRouting();
            app.UseCors("AllowOrigin");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
