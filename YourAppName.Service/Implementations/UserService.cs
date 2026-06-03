using Microsoft.AspNetCore.Identity;
using YourAppName.Data.Entities.Identity;
using YourAppName.Service.Abstracts;
using System.Linq;
using System.Threading.Tasks;

namespace YourAppName.Service.Implementations
{
    public class UserService : IUserService
    {
        #region Fields
        private readonly UserManager<ApplicationUser> _userManager;
        #endregion
        #region Constructor
        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        #endregion
        #region Methods
        public async Task<bool> IsEmailOrPhoneRegisteredAsync(string emailOrPhone)
        {
            var existingUser = await _userManager.FindByNameAsync(emailOrPhone);
            return existingUser != null;
        }

        public async Task<string?> RegisterUserAsync(ApplicationUser user, string password, string role)
        {
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                return string.Join(" - ", createResult.Errors.Select(e => e.Description));
            }

            // 🟢 ASSIGN THE DYNAMIC ROLE
            await _userManager.AddToRoleAsync(user, role);

            return null;
        }

        public async Task<string> GenerateVerificationCodeAsync(ApplicationUser user, string method)
        {
            var tokenProvider = method == "Email" ? TokenOptions.DefaultEmailProvider : TokenOptions.DefaultPhoneProvider;
            return await _userManager.GenerateTwoFactorTokenAsync(user, tokenProvider);
        }

        public async Task<bool> VerifyCodeAsync(ApplicationUser user, string code, string method)
        {
            var tokenProvider = method == "Email" ? TokenOptions.DefaultEmailProvider : TokenOptions.DefaultPhoneProvider;
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, code);

            if (isValid)
            {
                if (method == "Email") user.EmailConfirmed = true;
                else user.PhoneNumberConfirmed = true;

                await _userManager.UpdateAsync(user);
            }

            return isValid;
        }
        #endregion
    }
}