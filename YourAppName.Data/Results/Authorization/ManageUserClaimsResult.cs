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
        public bool HasPermission { get; set; }
    }
}
