using YourAppName.Data.Entities.Identity;
using YourAppName.Data.Results.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Service.Abstracts
{
    public interface IAuthorizationService
    {
        Task<List<ApplicationRole>> GetRolesListAsync(CancellationToken cancellationToken);
        Task<ApplicationRole?> GetRoleByIdAsync(string id, CancellationToken cancellationToken);
        Task<string> AddRoleAsync(string roleName);
        Task<string> EditRoleAsync(string id, string newName);
        Task<string> DeleteRoleAsync(string roleId);

        // Returns a Tuple containing all system roles AND the specific user's roles
        Task<(List<ApplicationRole> Roles, IList<string> UserRoles)?> GetManageUserRolesDataAsync(string userId);
        // Accepts a basic Tuple (String, Boolean) to maintain zero Core dependencies
        Task<string> UpdateUserRolesAsync(string userId, List<(string RoleName, bool HasRole)> userRoles);

        Task<ManageUserClaimsResult> ManageUserClaimsAsync(string userId);
        Task<string> UpdateUserClaimsAsync(string userId, List<UserClaimDto> requestClaims);
        Task<ManageRoleClaimsResult> ManageRoleClaimsAsync(string roleId);
        Task<string> UpdateRoleClaimsAsync(string roleId, List<RoleClaimDto> requestClaims);
        Task<string> ChangeUserStatusAsync(string userId, bool isActive);
    }
}
