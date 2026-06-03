using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Data.Results.Authorization
{
    public class ManageRoleClaimsResult
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<RoleClaimDto> RoleClaims { get; set; } = new List<RoleClaimDto>();
    }

    public class RoleClaimDto
    {
        public string PermissionName { get; set; } = string.Empty; // e.g., "Permissions.Roles.Create"
        public bool HasPermission { get; set; } // true if checked, false if unchecked
    }
}
