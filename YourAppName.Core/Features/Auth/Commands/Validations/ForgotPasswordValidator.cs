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
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
    {
        private readonly IStringLocalizer<SharedResources> _localizer;

        public ForgotPasswordValidator(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            RuleFor(x => x.EmailOrPhone)
                .NotEmpty().WithMessage(_localizer[SharedResourcesKeys.NotEmpty])
                // Restricts string size to optimize indexed lookups in SQL Server
                .MaximumLength(100).WithMessage(_localizer[SharedResourcesKeys.MaximumLength])
                // Evaluates format before passing it down to the database infrastructure layer
                .Must(IsValidEmailOrPhone).WithMessage(_localizer[SharedResourcesKeys.InvalidEmailOrPhone]);
        }

        private bool IsValidEmailOrPhone(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            // Ensures structural check for standard email schemas
            bool isEmail = Regex.IsMatch(input, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

            // Supports local and international dialing digits (between 10 and 15 digits)
            bool isPhone = Regex.IsMatch(input, @"^\+?[0-9]{10,15}$");

            return isEmail || isPhone;
        }
    }
}
