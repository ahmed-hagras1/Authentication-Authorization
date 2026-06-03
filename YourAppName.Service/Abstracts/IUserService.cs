using YourAppName.Data.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Service.Abstracts
{
    public interface IUserService
    {
        Task<bool> IsEmailOrPhoneRegisteredAsync(string emailOrPhone);
        Task<string?> RegisterUserAsync(ApplicationUser user, string password, string role);

        Task<string> GenerateVerificationCodeAsync(ApplicationUser user, string method);
        Task<bool> VerifyCodeAsync(ApplicationUser user, string code, string method);
    }
}
