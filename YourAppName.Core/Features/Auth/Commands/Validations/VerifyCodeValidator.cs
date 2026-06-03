using FluentValidation;
using Microsoft.Extensions.Localization;
using YourAppName.Core.Features.Auth.Commands.Models;
using YourAppName.Shared.Resources;
using System.Text.RegularExpressions;

namespace YourAppName.Core.Features.Auth.Commands.Validations
{
    public class VerifyCodeValidator : AbstractValidator<VerifyCodeCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public VerifyCodeValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // Validate Email or Phone
            RuleFor(x => x.EmailOrPhone)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                .Must(IsValidEmailOrPhone).WithMessage(_localizer[SharedResourcesKeys.InvalidEmailOrPhone]);

            // Validate the 6-Digit Code
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                .Length(6).WithMessage(_localizer[SharedResourcesKeys.CodeMustBe6Digits]) // Add this key to ResX
                .Matches(@"^\d+$").WithMessage(_localizer[SharedResourcesKeys.CodeMustBeNumbersOnly]); // Must be numbers only
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