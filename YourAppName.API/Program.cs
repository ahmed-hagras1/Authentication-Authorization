using YourAppName.Infrastructure.Dependencies;
using YourAppName.Service.Dependencies;
using YourAppName.Core.Dependencies;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

// SEEDING NAMESPACES
using YourAppName.Infrastructure.Data;
using YourAppName.Infrastructure.Seeder;
using Microsoft.AspNetCore.Identity;
using YourAppName.Data.Entities.Identity;

// Add the Middleware namespace
using YourAppName.Api.Middlewares;
using YourAppName.API.Filters;
using YourAppName.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using YourAppName.Shared.Security;

using Microsoft.OpenApi.Models;

namespace YourAppName.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            try
            {
                // Add services to the container.
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    // 1. Define the Security Scheme for JWT Bearer
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter your valid JWT token below.\r\n\r\nExample: 'eyJhGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
                    });

                    // 2. Apply the Security Requirement globally to all endpoints
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                         {
                             new OpenApiSecurityScheme
                             {
                                 Reference = new OpenApiReference
                                 {
                                     Type = ReferenceType.SecurityScheme,
                                     Id = "Bearer"
                                 }
                             },
                             Array.Empty<string>()
                         }
                    });
                });

                // ENABLE CORS
                // This allows the Frontend (React, Angular, Flutter, etc.) 
                // to communicate with your API from any domain.
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowAnyOrigin();
                    });
                });

                // Add Rate Limiting Services
                builder.Services.AddRateLimiter(options =>
                {
                    // Return a 429 status code when the limit is reached
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    // Define the specific policy
                    options.AddFixedWindowLimiter("AuthBruteForcePolicy", fixedOptions =>
                    {
                        fixedOptions.PermitLimit = 5; // Allow 5 requests
                        fixedOptions.Window = TimeSpan.FromMinutes(1); // per 1 minute
                        fixedOptions.QueueLimit = 0; // Reject immediately if over the limit
                    });
                });

                // This converts every string in your Permissions class into an active Security Policy
                builder.Services.AddAuthorization(options =>
                {
                    var permissionClasses = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

                    foreach (var module in permissionClasses)
                    {
                        var permissions = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                                                .Select(fi => fi.GetRawConstantValue()?.ToString());

                        foreach (var permission in permissions)
                        {
                            if (permission != null)
                            {
                                options.AddPolicy(permission, policy =>
                                    policy.RequireClaim(Permissions.Type, permission));
                            }
                        }
                    }
                });

                #region Dependency Injection
                builder.Services.AddInfrastructureDependencies(builder.Configuration)
                    .AddServiceDependencies()
                    .AddCoreDependencies()
                    .AddIdentityDependencies(builder.Configuration);
                #endregion

                #region Localization Services
                builder.Services.AddLocalization();
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ar-EG")
                };

                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                    options.ApplyCurrentCultureToResponseHeaders = true;
                });
                #endregion

                // Register the filter class for Dependency Injection
                builder.Services.AddScoped<TokenValidationFilter>();
                builder.Services.AddScoped<ValidateUserStatusFilter>();

                // Add it to the global filters collection
                builder.Services.AddControllers(options =>
                {
                    options.Filters.AddService<TokenValidationFilter>();
                    options.Filters.AddService<ValidateUserStatusFilter>();
                });

                // Register the background service worker 
                builder.Services.AddHostedService<TokenCleanupBackgroundService>();


                var app = builder.Build();

                // 🛑 MIDDLEWARE ORDER (CRITICAL)

                // Error Handler must be first to catch exceptions from all following layers
                app.UseMiddleware<ErrorHandlerMiddleware>();

                // Move HttpsRedirection up to ensure local SSL works before routing
                app.UseHttpsRedirection();

                // Enable CORS Policy
                // This must come before Authentication and MapControllers
                app.UseCors("AllowAll");

                // Add the Rate Limiter Middleware
                app.UseRateLimiter();

                app.UseRequestLocalization();

                // EXPOSE SWAGGER IN ALL ENVIRONMENTS
                // We removed the 'if (app.Environment.IsDevelopment())' check 
                // so your frontend partner can see the documentation on the host.
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "YourAppName API V1");

                    // This makes Swagger the default home page (e.g., https://your-app.com/)
                    options.RoutePrefix = string.Empty;
                });

                // AUTHENTICATION & AUTHORIZATION
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                #region Seeding Database
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var dbContext = services.GetRequiredService<AppDbContext>();
                        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                        // Seed data in order of dependency
                        // await RoleSeeder.SeedAsync(roleManager);
                        // await UserSeeder.SeedAsync(userManager, dbContext);
                        await CountrySeeder.SeedAsync(dbContext);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred during database seeding: {ex.Message}");
                    }
                }
                #endregion

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                // Log critical startup failures here
                throw;
            }
        }
    }
}