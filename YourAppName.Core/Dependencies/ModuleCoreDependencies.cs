using MediatR;
using Microsoft.Extensions.DependencyInjection;
using YourAppName.Core.Behaviors;
using System.Reflection;
using FluentValidation;

namespace YourAppName.Core.Dependencies
{
    public static class ModuleCoreDependencies
    {

        public static IServiceCollection AddCoreDependencies(this IServiceCollection services)
        {
            // 1. Register MediatR and add the ValidationBehavior
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());

                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // 2. Register all your FluentValidation rules automatically
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // 🟢 3. Register AutoMapper (THIS FIXES YOUR ERROR)
            // It will scan the current assembly (YourAppName.Core) and find all your Profile classes automatically!
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

            return services;
        }
    }
}
