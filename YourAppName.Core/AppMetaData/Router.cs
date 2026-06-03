using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.AppMetaData
{
    public static class Router
    {
        // Define root constants
        public const string SignleRoute = "/{id}";
        public const string root = "Api";
        public const string version = "V1";
        public const string Rule = root + "/" + version + "/";

        public static class ClientRouting
        {
            public const string Prefix = Rule + "Client";
            public const string Register = Prefix + "/Register";
            // Final URL: api/v1/Client/Register
        }
        public static class AuthRouting
        {
            public const string Prefix = Rule + "Auth";
            public const string VerifyCode = Prefix + "/VerifyCode";
            public const string SignIn = Prefix + "/SignIn";
            public const string Logout = Prefix + "/Logout";
            public const string ResendCode = Prefix + "/ResendCode";
            public const string RefreshToken = Prefix + "/RefreshToken";
            public const string RevokeToken = Prefix + "/RevokeToken";
            public const string RevokeAllSessions = Prefix + "/RevokeAllSessions";
            public const string ForgotPassword = Prefix + "/ForgotPassword";
            public const string VerifyResetCode = Prefix + "/VerifyResetCode";
            public const string ResetPassword = Prefix + "/ResetPassword";
            public const string ChangePassword = Prefix + "/ChangePassword";
            // Final URL: api/v1/Auth/VerifyCode
        }
        public static class AuthorizationRouting
        {
            public const string Prefix = Rule + "Authorization";
            public const string GetRolesList = Prefix + "/Role/List";
            public const string GetRoleById = Prefix + "/Role" + SignleRoute;
            public const string CreateRole = Prefix + "/Role/Create";
            public const string EditRole = Prefix + "/Role/Edit";
            public const string DeleteRole = Prefix + "/Role/Delete" + SignleRoute;

            public const string ManageUserRoles = Prefix + "/User-Roles" + SignleRoute;
            public const string UpdateUserRoles = Prefix + "/User-Roles/Update";

            public const string ManageUserClaims = Prefix + "/User-Claims" + SignleRoute;
            public const string UpdateUserClaims = Prefix + "/User-Claims/Update";

            public const string ManageRoleClaims = Prefix + "/Role-Claims" + SignleRoute;
            public const string UpdateRoleClaims = Prefix + "/Role-Claims/Update";

            public const string ChangeUserStatus = Prefix + "/User-Status/Change";
        }
    }
}
