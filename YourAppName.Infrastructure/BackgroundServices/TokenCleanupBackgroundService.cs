using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YourAppName.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourAppName.Infrastructure.BackgroundServices;

namespace YourAppName.Infrastructure.BackgroundServices
{
    public class TokenCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;

        public TokenCleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<TokenCleanupBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Background Service has started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Calculate the delay until the next 3:00 AM
                var now = DateTime.Now;
                var nextRunTime = DateTime.Today.AddHours(3); // Today at 3:00 AM

                // If it's already past 3:00 AM today, schedule it for 3:00 AM tomorrow
                if (now >= nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1);
                }

                var delay = nextRunTime - now;
                _logger.LogInformation("Next token cleanup scheduled at: {Time}. Sleeping for {Delay}.", nextRunTime, delay);

                try
                {
                    // Wait until 3:00 AM arrives
                    await Task.Delay(delay, stoppingToken);

                    // Execute the cleanup operation
                    await CleanObsoleteTokensAsync();
                }
                catch (TaskCanceledException)
                {
                    // Occurs gracefully when the application is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the automatic token cleanup task.");
                }
            }
        }

        private async Task CleanObsoleteTokensAsync()
        {
            _logger.LogInformation("Starting automatic database cleanup for expired and revoked tokens...");

            // Solve the Singleton trap by creating a temporary scope manually
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // High Performance: ExecuteDeleteAsync sends a direct SQL DELETE statement 
            // without loading the entities into server memory first.
            var deletedRowsCount = await dbContext.UserRefreshTokens
                .Where(x => x.IsRevoked || x.ExpiryDate <= DateTime.UtcNow)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Database cleanup completed successfully. Purged {Count} obsolete tokens.", deletedRowsCount);
        }
    }
}
