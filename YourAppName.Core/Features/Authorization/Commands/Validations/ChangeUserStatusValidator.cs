using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using YourAppName.Core.Features.Authorization.Commands.Models;
using YourAppName.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Commands.Validations
{
    public class ChangeUserStatusValidator : AbstractValidator<ChangeUserStatusCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeUserStatusValidator(IStringLocalizer<SharedResources> localizer, IHttpContextAccessor httpContextAccessor)
        {
            _localizer = localizer;
            _httpContextAccessor = httpContextAccessor;
            ApplyValidationsRules();
        }

        public void ApplyValidationsRules()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.Required])
                .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required])
                .Must(NotBeCurrentAdmin).WithMessage((_localizer[SharedResourcesKeys.CannotDeactivateOwnAccount])); // Add Arabic localization for this if needed!
        }

        // Prevent self-lockout
        private bool NotBeCurrentAdmin(string targetUserId)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return currentUserId != targetUserId;
        }
    }
}
