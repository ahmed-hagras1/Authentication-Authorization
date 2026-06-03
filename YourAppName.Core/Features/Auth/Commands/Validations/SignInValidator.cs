using FluentValidation;
using Microsoft.Extensions.Localization;
using YourAppName.Core.Features.Auth.Commands.Models;
using YourAppName.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YourAppName.Core.Features.Auth.Commands.Validations
{
    public class SignInValidator : AbstractValidator<SignInCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public SignInValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
            ApplyValidationRules();
        }
        private void ApplyValidationRules()
        {
            RuleFor(x => x.LoginIdentifier)
                    .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                    // Since it's a dual-field, we don't use .EmailAddress() here
                    // so it doesn't block phone numbers.
                    .MaximumLength(100).WithMessage(_localizer[SharedResourcesKeys.MaximumLength])
                     .Must(IsValidEmailOrPhone).WithMessage(_localizer[SharedResourcesKeys.InvalidEmailOrPhone]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                .MaximumLength(100).WithMessage(_localizer[SharedResourcesKeys.MaximumLength]);
        }
        private bool IsValidEmailOrPhone(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            bool isEmail = Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            bool isPhone = Regex.IsMatch(input, @"^\+?[0-9]{10,15}$");
            return isEmail || isPhone;
        }
    }
}
