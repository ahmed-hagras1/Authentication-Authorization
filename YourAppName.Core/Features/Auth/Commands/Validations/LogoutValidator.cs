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
    public class LogoutValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NotEmpty]);
        }
    }
}
