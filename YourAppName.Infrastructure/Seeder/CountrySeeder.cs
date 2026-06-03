using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YourAppName.Data.Entities;
using YourAppName.Infrastructure.Data;

namespace YourAppName.Infrastructure.Seeder
{
    public static class CountrySeeder
    {
        public static async Task SeedAsync(AppDbContext dbContext) // Use your actual DbContext name
        {
            // Check if the table is empty
            if (!await dbContext.Set<Country>().AnyAsync())
            {
                var countries = new List<Country>
            {
                new Country { Name = "Egypt", DialCode = "+20" },
                new Country { Name = "Saudi Arabia", DialCode = "+966" },
                new Country { Name = "United Arab Emirates", DialCode = "+971" },
                new Country { Name = "Kuwait", DialCode = "+965" },
                new Country { Name = "Qatar", DialCode = "+974" }
            };

                await dbContext.Set<Country>().AddRangeAsync(countries);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
