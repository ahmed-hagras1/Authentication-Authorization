using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YourAppName.Api.Base;
using YourAppName.Core.AppMetaData;
using YourAppName.Core.Features.Auth.Commands.Models;
using System.Security.Claims;
using static YourAppName.Core.AppMetaData.Router;

namespace YourAppName.API.Controllers
{
    [ApiController]
    public class AuthController : AppControllerBase
    {
        [HttpPost(Router.AuthRouting.VerifyCode)]
        [AllowAnonymous]
        [EnableRateLimiting("AuthBruteForcePolicy")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command,cancellationToken);
            return NewResult(response);
        }
        [HttpPost(Router.AuthRouting.ResendCode)]
        [AllowAnonymous]
        [EnableRateLimiting("AuthBruteForcePolicy")]
        public async Task<IActionResult> ResendCode([FromBody] ResendCodeCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [HttpPost(Router.AuthRouting.SignIn)]
        [AllowAnonymous]
        [EnableRateLimiting("AuthBruteForcePolicy")]
        public async Task<IActionResult> SignIn([FromBody] SignInCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [HttpPost(Router.AuthRouting.Logout)]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // The filter already verified the token isn't revoked in the DB!
            // We can just safely proceed with executing the command.
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var command = new LogoutCommand { AccessToken = accessToken! };
            var response = await Mediator.Send(command);

            return NewResult(response);
        }

        [HttpPost(Router.AuthRouting.RefreshToken)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var response = await Mediator.Send(command);
            return NewResult(response);
        }
        [HttpPost(Router.AuthRouting.RevokeToken)]
        [Authorize] // Only logged-in users (or admins) can revoke tokens
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenCommand command)
        {
            var response = await Mediator.Send(command);
            return NewResult(response);
        }
        [HttpPost(Router.AuthRouting.RevokeAllSessions)]
        [Authorize] 
        public async Task<IActionResult> RevokeAllSessions([FromBody] RevokeAllSessionsCommand command)
        {
            var response = await Mediator.Send(command);
            return NewResult(response);
        }
        [HttpPost(AuthRouting.ForgotPassword)]
        [AllowAnonymous] // Accessible by logged-out users
        [EnableRateLimiting("AuthBruteForcePolicy")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var response = await Mediator.Send(command);
            return NewResult(response);
        }
        [HttpPost(AuthRouting.VerifyResetCode)] 
        [AllowAnonymous]
        [EnableRateLimiting("AuthBruteForcePolicy")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [HttpPost(Router.AuthRouting.ResetPassword)]
        [AllowAnonymous]
        [EnableRateLimiting("AuthBruteForcePolicy")] // 🛡️ Protects the final reset action
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
        [HttpPost(Router.AuthRouting.ChangePassword)]
        [Authorize] // Requires a valid JWT token
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            // Extract the secure UserId directly from the token claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Attach it to the command
            command.UserId = userId;

            var response = await Mediator.Send(command, cancellationToken);
            return NewResult(response);
        }
    }
}
