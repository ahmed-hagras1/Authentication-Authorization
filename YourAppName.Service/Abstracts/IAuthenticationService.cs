using Microsoft.AspNetCore.Identity;
using YourAppName.Data.Entities.Identity;
using YourAppName.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Service.Abstracts
{
    public interface IAuthenticationService
    {
        Task<JWTAuthResult> GetJWTToken(ApplicationUser user);
        Task<JWTAuthResult> GetRefreshToken(string accessToken, string refreshToken);
        Task<string> ValidateToken(string accessToken);
        Task<string> RevokeRefreshToken(string accessToken);
        Task<string> RevokeToken(string refreshToken);
        Task<string> RevokeAllSessions(string userId);
        Task<string> ForgotPasswordAsync(string emailOrPhone, CancellationToken cancellationToken);
        Task<string> VerifyResetCodeAsync(string emailOrPhone, string code, CancellationToken cancellationToken);
        Task<IdentityResult> ResetPasswordAsync(string emailOrPhone, string code, string newPassword, CancellationToken cancellationToken);
        Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
