using Microsoft.AspNetCore.Identity;
using YourAppName.Data.Entities.Identity;
using YourAppName.Data.Entities;
using YourAppName.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace YourAppName.Infrastructure.Seeder
{
    public static class UserSeeder
    {
        //public static async Task SeedAsync(UserManager<ApplicationUser> userManager, AppDbContext dbContext)
        //{
        //    // Only seed if no users exist
        //    if (!userManager.Users.Any())
        //    {
        //        // Grab the first country from the database to satisfy the CountryId FK
        //        var defaultCountry = dbContext.Set<Country>().FirstOrDefault();
        //        int defaultCountryId = defaultCountry != null ? defaultCountry.Id : 1;

        //        var defaultAdmin = new ApplicationUser
        //        {
        //            UserName = "admin@YourAppName.com",
        //            Email = "admin@YourAppName.com",
        //            FullName = "YourAppName Admin",
        //            EmailConfirmed = true,
        //            PhoneNumberConfirmed = true,
        //            PreferredLanguage = "ar-EG",
        //            CountryId = defaultCountryId
        //        };

        //        // Create the user with a strong default password
        //        var result = await userManager.CreateAsync(defaultAdmin, "Admin@123");

        //        // If successful, attach the Admin role to this user
        //        if (result.Succeeded)
        //        {
        //            await userManager.AddToRoleAsync(defaultAdmin, "Admin");
        //        }
        //    }
        //}
    }
}