using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Queries.Results
{
    public class ManageUserRolesResult
    {
        public string UserId { get; set; } = string.Empty;
        public List<UserRoleViewModel> UserRoles { get; set; } = new();
    }

    public class UserRoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool HasRole { get; set; }
    }
}
