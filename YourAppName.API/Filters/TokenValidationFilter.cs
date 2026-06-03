using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using YourAppName.Infrastructure.Data;
using YourAppName.Shared.Resources;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;

namespace YourAppName.API.Filters
{
    public class TokenValidationFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _dbContext;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public TokenValidationFilter(AppDbContext dbContext, IStringLocalizer<SharedResources> localizer)
        {
            _dbContext = dbContext;
            _localizer = localizer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Only validate if the mathematical token validation (JWT Bearer middleware) succeeded
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                // Extract the unique cryptographic Token Identifier (JTI Claim) from the current token
                var jti = context.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (!string.IsNullOrEmpty(jti))
                {
                    // Strategic Check: Intercept if the token is flagged as Revoked OR Used
                    var isTokenObsolete = await _dbContext.UserRefreshTokens
                        .AnyAsync(x => x.JWTId == jti && (x.IsRevoked || x.IsUsed));

                    if (isTokenObsolete)
                    {
                        // Short-circuit the request and return a standardized, localized 401 response response
                        var responseModel = new
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            Succeeded = false,
                            Message = _localizer[SharedResourcesKeys.TokenIsInvalid].Value,
                            Errors = new[] { "This session is no longer active. Please log in again." }
                        };

                        context.Result = new UnauthorizedObjectResult(responseModel);
                        return; // Halts the pipeline, blocking the controller action from executing
                    }
                }
            }

            // 5. If the token is healthy and valid, forward control to the target Controller Action
            await next();
        }
    }
}
