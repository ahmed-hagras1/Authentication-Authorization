using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Data.Results.Authorization
{
    public class ManageUserClaimsResult
    {
        public string UserId { get; set; } = string.Empty;
        // You can add UserName or Email here if you want to display it on the UI
        public List<UserClaimDto> UserClaims { get; set; } = new List<UserClaimDto>();
    }
    public class UserClaimDto
    {
        public string PermissionName { get; set; } = string.Empty;

        // True if the claim is in AspNetUserClaims
        public bool HasDirectPermission { get; set; }

        // True if the claim is inherited via AspNetRoleClaims
        public bool InheritedFromRole { get; set; }

        // Helper for the UI to know if the user is authorized either way
        public bool IsAuthorized => HasDirectPermission || InheritedFromRole;
    }
}
