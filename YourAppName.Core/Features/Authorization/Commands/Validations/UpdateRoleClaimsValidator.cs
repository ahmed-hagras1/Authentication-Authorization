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
    public class UpdateRoleClaimsValidator : AbstractValidator<UpdateRoleClaimsCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public UpdateRoleClaimsValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
            ApplyValidationsRules();
        }

        public void ApplyValidationsRules()
        {
            // Validate the RoleId
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.Required])
                .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required]);

            // Validate the List itself (it shouldn't be null, though empty might be okay if they revoke all permissions)
            RuleFor(x => x.RoleClaims)
                .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required]);

            // Validate EVERY item inside the list!
            RuleForEach(x => x.RoleClaims).ChildRules(claim =>
            {
                claim.RuleFor(c => c.PermissionName)
                     .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.Required])
                     .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required]);
            });
        }
    }
}
