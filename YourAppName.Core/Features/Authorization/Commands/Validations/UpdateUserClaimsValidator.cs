using FluentValidation;
using Microsoft.Extensions.Localization;
using YourAppName.Core.Features.Authorization.Commands.Models;
using YourAppName.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Authorization.Commands.Validations
{
    public class UpdateUserClaimsValidator : AbstractValidator<UpdateUserClaimsCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public UpdateUserClaimsValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
            ApplyValidationsRules();
        }

        public void ApplyValidationsRules()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.Required])
                .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required]);

            RuleFor(x => x.UserClaims)
                .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required]);

            RuleForEach(x => x.UserClaims).ChildRules(claim =>
            {
                claim.RuleFor(c => c.PermissionName)
                     .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.Required])
                     .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required]);
            });
        }
    }
}
