using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourAppName.Data.Entities.Identity;
using YourAppName.Data.Results.Authorization;
using YourAppName.Infrastructure.Data;
using YourAppName.Service.Abstracts;
using YourAppName.Shared.Resources;
using YourAppName.Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Service.Implementations
{
    public class AuthorizationService : IAuthorizationService
    {
        #region Fields
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        #endregion
        #region Constractor
        // _userManager omitted for brevity if not used in this specific method

        public AuthorizationService(RoleManager<ApplicationRole> roleManager,
            AppDbContext appDbContext,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _appDbContext = appDbContext;
            _userManager = userManager;
        }
        #endregion
        #region Methods
        public async Task<List<ApplicationRole>> GetRolesListAsync(CancellationToken cancellationToken)
        {
            return await _roleManager.Roles.ToListAsync(cancellationToken);
        }
        public async Task<ApplicationRole?> GetRoleByIdAsync(string id, CancellationToken cancellationToken)
        {
            return await _roleManager.Roles.FirstOrDefaultAsync( r =>  r.Id == id, cancellationToken);
        }
        public async Task<string> AddRoleAsync(string roleName)
        {
            // 1. Check if the role already exists
            var isExist = await _roleManager.RoleExistsAsync(roleName);
            if (isExist)
            {
                return "RoleIsExist";
            }

            // 2. Create the new role
            var identityRole = new ApplicationRole
            {
                Name = roleName
            };

            var result = await _roleManager.CreateAsync(identityRole);

            if (result.Succeeded)
            {
                return "Success";
            }

            return "Failed";
        }
        public async Task<string> EditRoleAsync(string id, string newName)
        {
            // Check if the role exists
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return "NotFound";
            }

            // Update the name
            role.Name = newName;

            // Save changes to the database
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return "Success";
            }

            return "Failed";
        }
        public async Task<string> DeleteRoleAsync(string roleId)
        {
            // 1. Check if the role actually exists
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return "NotFound";

            var usersInRole = await _appDbContext.UserRoles.AnyAsync(ur => ur.RoleId ==  roleId);

            if (usersInRole)
            {
                return "HasUsers";
            }

            // Safe to delete
            var result = await _roleManager.DeleteAsync(role);

            return result.Succeeded ? "Success" : "Failed";
        }
        public async Task<(List<ApplicationRole> Roles, IList<string> UserRoles)?> GetManageUserRolesDataAsync(string userId)
        {
            // Check if user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Get ALL roles in the system
            var allRoles = await _roleManager.Roles.ToListAsync();

            // Get the roles assigned to this specific user (returns a list of Role Names)
            var userRoles = await _userManager.GetRolesAsync(user);

            // Return them as a raw Tuple to the Core layer
            return (allRoles, userRoles);
        }
        public async Task<string> UpdateUserRolesAsync(string userId, List<(string RoleName, bool HasRole)> userRoles)
        {
            // Find the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "UserNotFound";

            // Get the user's CURRENT roles from the database
            var currentRoles = await _userManager.GetRolesAsync(user);

            // 3. Find roles to ADD 
            var rolesToAdd = userRoles
                .Where(x => x.HasRole && !currentRoles.Contains(x.RoleName))
                .Select(x => x.RoleName)
                .ToList();

            // Find roles to REMOVE
            var rolesToRemove = userRoles
                .Where(x => !x.HasRole && currentRoles.Contains(x.RoleName))
                .Select(x => x.RoleName)
                .ToList();

            // START THE TRANSACTION
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                int num1 = 0, num2 = 0;
                //int x = num1 / num2;

                // Execute Removals
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    if (!removeResult.Succeeded)
                    {
                        // No need to rollback yet, nothing was added, but we exit safely
                        await transaction.RollbackAsync();
                        return "FailedToRemoveOldRoles";
                    }
                }

                // int x = num1 / num2;

                // 3. Execute Additions
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

                    

                    if (!addResult.Succeeded)
                    {
                        // We MUST undo the removals so the user isn't left broken.
                        await transaction.RollbackAsync();
                        return "FailedToAddNewRoles";
                    }
                }
                // int x = num1 / num2; 

                // EVERYTHING SUCCEEDED! Commit the changes permanently to the database.
                await transaction.CommitAsync();
                return "Success";
            }
            catch (Exception)
            {
                // If the server crashes, database connection drops, or an exception is thrown,
                // this guarantees the database undoes any partial work.
                await transaction.RollbackAsync();
                return "FailedToUpdateRoles"; // You can add this to your localization keys!
            }
        }
        public async Task<ManageUserClaimsResult> ManageUserClaimsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            // Get the claims this USER already has directly assigned to them
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var existingClaimValues = existingClaims
                .Where(x => x.Type == Permissions.Type) // Only look at Permission claims!
                .Select(x => x.Value)
                .ToList();

            var userClaimsList = new List<UserClaimDto>();

            // Use Reflection to grab ALL possible permissions in the system
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
                        // 3. Add to the checklist and check if the user currently holds it
                        userClaimsList.Add(new UserClaimDto
                        {
                            PermissionName = permission,
                            HasPermission = existingClaimValues.Contains(permission)
                        });
                    }
                }
            }

            return new ManageUserClaimsResult
            {
                UserId = userId,
                UserClaims = userClaimsList
            };
        }
        public async Task<string> UpdateUserClaimsAsync(string userId, List<UserClaimDto> requestClaims)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "UserNotFound";

            // Get ALL current claims for this user
            var allExistingClaims = await _userManager.GetClaimsAsync(user);

            // Filter out non-permission claims (like Email, FullName) so we don't accidentally delete them!
            var existingPermissionClaims = allExistingClaims.Where(x => x.Type == Permissions.Type).ToList();
            var existingClaimValues = existingPermissionClaims.Select(x => x.Value).ToList();

            // Figure out which claims to ADD
            var claimsToAdd = requestClaims
                .Where(x => x.HasPermission && !existingClaimValues.Contains(x.PermissionName))
                .ToList();

            // Figure out which claims to REMOVE
            // We match against existingPermissionClaims because Identity requires the actual Claim object to remove it
            var claimsToRemove = existingPermissionClaims
                .Where(x => requestClaims.Any(req => req.PermissionName == x.Value && !req.HasPermission))
                .ToList();

            // 🛡️ START TRANSACTION 🛡️
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                // Execute the deletions
                foreach (var claim in claimsToRemove)
                {
                    var removeResult = await _userManager.RemoveClaimAsync(user, claim);
                    if (!removeResult.Succeeded)
                    {
                        await transaction.RollbackAsync(); // Revert everything!
                        return "FailedToRemove";
                    }
                }

                // Execute the insertions
                foreach (var claim in claimsToAdd)
                {
                    var addResult = await _userManager.AddClaimAsync(user, new Claim(Permissions.Type, claim.PermissionName));
                    if (!addResult.Succeeded)
                    {
                        await transaction.RollbackAsync(); // Revert everything!
                        return "FailedToAdd";
                    }
                }

                // If we reach this line, EVERYTHING was successful!
                await transaction.CommitAsync();
                return "Success";
            }
            catch (Exception)
            {
                // Catch database connection drops or unexpected exceptions
                await transaction.RollbackAsync();
                return "FailedToUpdate";
            }
        }
        public async Task<ManageRoleClaimsResult> ManageRoleClaimsAsync(string roleId)
        {
            // Check if the role exists
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return null; // We will handle this null in the Command Handler

            // Get the permissions this role ALREADY has in the database
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var existingClaimValues = existingClaims.Select(x => x.Value).ToList();

            // Prepare our empty checklist
            var roleClaimsList = new List<RoleClaimDto>();

            // Use Reflection to grab ALL possible permissions in the system
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
                        // Add to the checklist and check if the role currently holds it
                        roleClaimsList.Add(new RoleClaimDto
                        {
                            PermissionName = permission,
                            HasPermission = existingClaimValues.Contains(permission)
                        });
                    }
                }
            }

            // Return the perfectly formatted result for the UI
            return new ManageRoleClaimsResult
            {
                RoleId = roleId,
                RoleName = role.Name,
                RoleClaims = roleClaimsList
            };
        }
        public async Task<string> UpdateRoleClaimsAsync(string roleId, List<RoleClaimDto> requestClaims)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return "RoleNotFound";

            // Get current claims from the database
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var existingClaimValues = existingClaims.Select(x => x.Value).ToList();

            // Figure out which claims to ADD
            var claimsToAdd = requestClaims
                .Where(x => x.HasPermission && !existingClaimValues.Contains(x.PermissionName))
                .ToList();

            // Figure out which claims to REMOVE
            var claimsToRemove = existingClaims
                .Where(x => requestClaims.Any(req => req.PermissionName == x.Value && !req.HasPermission))
                .ToList();

            // START TRANSACTION
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                // Execute the deletions
                foreach (var claim in claimsToRemove)
                {
                    var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
                    if (!removeResult.Succeeded)
                    {
                        await transaction.RollbackAsync(); // Revert everything!
                        return "FailedToRemove";
                    }
                }

                // Execute the insertions
                foreach (var claim in claimsToAdd)
                {
                    var addResult = await _roleManager.AddClaimAsync(role, new Claim(Permissions.Type, claim.PermissionName));
                    if (!addResult.Succeeded)
                    {
                        await transaction.RollbackAsync(); // Revert everything!
                        return "FailedToAdd";
                    }
                }

                // If we reach this line, EVERYTHING was successful!
                await transaction.CommitAsync();
                return "Success";
            }
            catch (Exception)
            {
                // Catch any unexpected database crashes (like connection timeouts)
                await transaction.RollbackAsync();
                return "FailedToUpdate";
            }
        }
        public async Task<string> ChangeUserStatusAsync(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "UserNotFound";

            // Update the status
            user.IsActive = isActive;

            // Execute the update
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // CRITICAL: If deactivating, change the security stamp. 
                // This instantly invalidates their current tokens across the system!
                if (!isActive)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
                return "Success";
            }

            return "FailedToUpdateStatus";
        }
        #endregion
    }
}
