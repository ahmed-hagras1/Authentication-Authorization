using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Shared.Security
{
    public static class Permissions
    {
        public const string Type = "Permission";

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
        }

        public static class Users
        {
            public const string ViewRoles = "Permissions.Users.ViewRoles";
            public const string EditRoles = "Permissions.Users.EditRoles";
            public const string ViewClaims = "Permissions.Users.ViewClaims";
            public const string EditClaims = "Permissions.Users.EditClaims";
            public const string ChangeStatus = "Permissions.Users.ChangeStatus";
        }

        public static class Security
        {
            public const string RevokeOtherUserSessions = "Permissions.Security.RevokeSessions";
        }
    }
}
