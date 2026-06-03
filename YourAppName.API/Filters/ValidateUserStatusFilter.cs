using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using YourAppName.Data.Entities.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using YourAppName.Shared.Resources;

namespace YourAppName.API.Filters
{
    public class ValidateUserStatusFilter : IAsyncAuthorizationFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public ValidateUserStatusFilter(UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResources> localizer)
        {
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userPrincipal = context.HttpContext.User;

            // Skip if the user isn't even authenticated
            if (userPrincipal.Identity == null || !userPrincipal.Identity.IsAuthenticated)
            {
                return;
            }

            var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedObjectResult(_localizer[SharedResourcesKeys.InvalidToken].Value);
                return;
            }

            // Fetch the user from the database
            var user = await _userManager.FindByIdAsync(userId);

            // 3. VALIDATION 1: Does the user still exist? (Reusing your UserNotFound key)
            if (user == null)
            {
                context.Result = new UnauthorizedObjectResult(_localizer[SharedResourcesKeys.UserNotFound].Value);
                return;
            }

            // 4. VALIDATION 2: Is the user active? 
            if (!user.IsActive)
            {
                context.Result = new UnauthorizedObjectResult(_localizer[SharedResourcesKeys.AccountDeactivated].Value);
                return;
            }

            // 5. VALIDATION 3: Has the account been modified? (Security Stamp check)
            var tokenSecurityStamp = userPrincipal.FindFirstValue("SecurityStamp");
            if (tokenSecurityStamp != user.SecurityStamp)
            {
                context.Result = new UnauthorizedObjectResult(_localizer[SharedResourcesKeys.AccountModified].Value);
                return;
            }

            // 💡 ADDITIONAL VALIDATIONS:

            // Validation 4: Is the user currently locked out?
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                context.Result = new UnauthorizedObjectResult(_localizer[SharedResourcesKeys.AccountLocked].Value);
                return;
            }

            // Validation 5: Did you force email confirmation?
            if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            {
                context.Result = new UnauthorizedObjectResult(_localizer[SharedResourcesKeys.EmailNotConfirmed].Value);
                return;
            }
        }
    }
}
