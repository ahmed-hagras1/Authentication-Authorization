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
    public class UpdateUserRolesValidator : AbstractValidator<UpdateUserRolesCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public UpdateUserRolesValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                .MaximumLength(100).WithMessage(_localizer[SharedResourcesKeys.MaximumLength]);

            RuleFor(x => x.UserRoles)
                .NotNull().WithMessage(_localizer[SharedResourcesKeys.Required]);
        }
    }
}
