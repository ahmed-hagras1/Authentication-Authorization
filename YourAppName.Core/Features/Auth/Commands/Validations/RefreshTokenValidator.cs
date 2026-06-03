using FluentValidation;
using Microsoft.Extensions.Localization;
using YourAppName.Core.Features.Auth.Commands.Models;
using YourAppName.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Auth.Commands.Validations
{
    public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public RefreshTokenValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
            ApplyValidationRules();
        }
        private void ApplyValidationRules()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty]);

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty]);
        }
    }
}
