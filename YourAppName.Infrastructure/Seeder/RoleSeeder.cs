using Microsoft.AspNetCore.Identity;
using YourAppName.Data.Entities.Identity;
using YourAppName.Shared.Security;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace YourAppName.Infrastructure.Seeder
{
    public static class RoleSeeder
    {
        //public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager)
        //{
        //    var systemRoles = new List<string> { "Admin", "Coordinator", "ServiceProvider", "Client" };

        //    foreach (var roleName in systemRoles)
        //    {
        //        if (!await roleManager.RoleExistsAsync(roleName))
        //        {
        //            await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        //        }
        //    }

        //    var adminRole = await roleManager.FindByNameAsync("Admin");
        //    if (adminRole != null)
        //    {
        //        var existingClaims = await roleManager.GetClaimsAsync(adminRole);
        //        var existingClaimValues = existingClaims.Select(c => c.Value).ToList();

        //        // Read directly from the Shared Layer
        //        var permissionClasses = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        //        foreach (var module in permissionClasses)
        //        {
        //            var permissions = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        //                                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
        //                                    .Select(fi => fi.GetRawConstantValue()?.ToString());

        //            foreach (var permission in permissions)
        //            {
        //                if (permission != null && !existingClaimValues.Contains(permission))
        //                {
        //                    await roleManager.AddClaimAsync(adminRole, new Claim(Permissions.Type, permission));
        //                }
        //            }
        //        }
        //    }
        //}
    }
}