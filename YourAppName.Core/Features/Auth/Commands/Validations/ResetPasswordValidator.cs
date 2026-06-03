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
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public ResetPasswordValidator(IStringLocalizer<SharedResources> localizer)
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
                .Length(6).WithMessage(_localizer[SharedResourcesKeys.CodeMustBe6Digits])
                .Matches(@"^\d{6}$").WithMessage(_localizer[SharedResourcesKeys.InvalidCode]);

            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop) // Stops running rules if the previous one fails
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                .MinimumLength(6).WithMessage(_localizer[SharedResourcesKeys.PasswordMinimumLength])
                .Matches("[a-z]").WithMessage(_localizer[SharedResourcesKeys.PasswordRequiresLower])
                .Matches("[0-9]").WithMessage(_localizer[SharedResourcesKeys.PasswordRequiresDigit]);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage(_localizer[SharedResourcesKeys.PasswordsDoNotMatch]);
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
