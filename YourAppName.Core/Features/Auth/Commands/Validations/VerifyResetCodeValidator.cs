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
    public class VerifyResetCodeValidator : AbstractValidator<VerifyResetCodeCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public VerifyResetCodeValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            RuleFor(x => x.EmailOrPhone)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                .Must(IsValidEmailOrPhone).WithMessage(_localizer[SharedResourcesKeys.InvalidEmailOrPhone]);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                // Must be exactly 6 characters long
                .Length(6).WithMessage(_localizer[SharedResourcesKeys.CodeMustBe6Digits])
                // Must be only numbers
                .Matches(@"^\d{6}$").WithMessage(_localizer[SharedResourcesKeys.InvalidCode]);
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
