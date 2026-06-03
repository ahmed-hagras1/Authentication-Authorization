using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YourAppName.Infrastructure.Abstracts;
using YourAppName.Infrastructure.Data;
using YourAppName.Infrastructure.InfrastructureBases;
using YourAppName.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Infrastructure.Dependencies;
public static class ModuleInfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Database Connection String
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        // Register Generic Repository
        services.AddTransient(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));

        // Register your infrastructure services here
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}


