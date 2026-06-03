using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YourAppName.Data.Entities.Identity;
using YourAppName.Data.Helpers;
using YourAppName.Infrastructure.Abstracts;
using YourAppName.Infrastructure.Data;
using YourAppName.Service.Abstracts;
using YourAppName.Shared.Resources;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Service.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        #region Fields
        private readonly JWTSettings _jwtSettings;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailService _emailService; 
        private readonly ISmsService _smsService;     
        private readonly AppDbContext _dbContext;     
        #endregion

        #region Constructor
        public AuthenticationService(IOptions<JWTSettings> jwtSettings,
            IRefreshTokenRepository refreshTokenRepository,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ISmsService smsService,
            AppDbContext dbContext,
            RoleManager<ApplicationRole> roleManager)
        {
            _jwtSettings = jwtSettings.Value;
            _refreshTokenRepository = refreshTokenRepository;
            _userManager = userManager;
            _emailService = emailService;
            _smsService = smsService;
            _dbContext = dbContext;
            _roleManager = roleManager;
        }
        #endregion

        #region Public Methods

        public async Task<JWTAuthResult> GetJWTToken(ApplicationUser user)
        {
            var (jwtToken, accessToken) = await GenerateJWTToken(user);
            var refreshTokenString = GenerateRefreshToken();

            var jwtAuthResult = new JWTAuthResult()
            {
                AccessToken = accessToken,
                RefreshToken = new RefreshToken()
                {
                    UserName = user.UserName!,
                    TokenString = refreshTokenString,
                    ExpireAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays)
                }
            };

            var userRefreshToken = new UserRefreshToken()
            {
                UserId = user.Id,
                Token = refreshTokenString,
                JWTId = jwtToken.Id,
                IsUsed = false,
                IsRevoked = false,
                AddedTime = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDurationInDays)
            };

            await _refreshTokenRepository.AddAsync(userRefreshToken);
            return jwtAuthResult;
        }

        public async Task<JWTAuthResult> GetRefreshToken(string accessToken, string refreshToken)
        {
            var jwtToken = ReadJWTToken(accessToken);

            // Use claims defined in UserClaimModel for verification
            var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == nameof(UserClaimModel.UserName))?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userName))
                throw new SecurityTokenException("TokenClaimsMissing");

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) throw new SecurityTokenException("UserNotFound");

            var userRefreshToken = await _refreshTokenRepository.GetTableNoTracking()
                .FirstOrDefaultAsync(x => x.Token == refreshToken && x.UserId == user.Id && x.JWTId == jti);

            if (userRefreshToken == null) throw new SecurityTokenException("RefreshTokenNotFound");

            if (userRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                userRefreshToken.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(userRefreshToken);
                throw new SecurityTokenException("RefreshTokenExpired");
            }

            if (!userRefreshToken.IsActive) throw new SecurityTokenException("RefreshTokenRevoked");

            userRefreshToken.IsUsed = true;
            await _refreshTokenRepository.UpdateAsync(userRefreshToken);

            return await GetJWTToken(user);
        }

        public async Task<string> ValidateToken(string accessToken)
        {
            var jwtToken = ReadJWTToken(accessToken);
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jti)) throw new SecurityTokenException("JtiClaimMissing");
            return jti;
        }

        public async Task<string> RevokeRefreshToken(string accessToken)
        {
            var jti = await ValidateToken(accessToken);
            var userRefreshToken = await _refreshTokenRepository.GetTableNoTracking()
                .FirstOrDefaultAsync(x => x.JWTId == jti);

            if (userRefreshToken == null)
                throw new KeyNotFoundException(SharedResourcesKeys.TokenNotFound);

            userRefreshToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(userRefreshToken);

            return SharedResourcesKeys.LoggedOutSuccessfully;
        }
        public async Task<string> RevokeToken(string refreshToken)
        {
            var userRefreshToken = await _refreshTokenRepository.GetTableNoTracking()
                    .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (userRefreshToken == null)
                throw new KeyNotFoundException(SharedResourcesKeys.TokenNotFound);

            if (userRefreshToken.IsRevoked)
                return SharedResourcesKeys.TokenAlreadyRevoked;

            userRefreshToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(userRefreshToken);

            return SharedResourcesKeys.TokenRevokedSuccessfully;
        }
        public async Task<string> RevokeAllSessions(string userId)
        {
            // Get all active (not yet revoked) tokens for this user
            var userTokens = await _refreshTokenRepository.GetTableNoTracking()
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync();

            if (!userTokens.Any())
                return SharedResourcesKeys.NoActiveSessionsFound;

            // Mark all as revoked
            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                // Your repository likely handles the Attach/Update logic
                await _refreshTokenRepository.UpdateAsync(token);
            }

            return SharedResourcesKeys.AllSessionsRevokedSuccessfully;
        }
        public async Task<string> ForgotPasswordAsync(string emailOrPhone, CancellationToken cancellationToken)
        {
            // 1. Unified Search optimized across Identity Indices
            var user = await _userManager.FindByNameAsync(emailOrPhone)
                       ?? await _userManager.FindByEmailAsync(emailOrPhone)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == emailOrPhone, cancellationToken);

            // Security Guardrail: Fake success text to mitigate username/email enumeration attacks
            if (user == null)
            {
                return SharedResourcesKeys.UserNotFound;
            }

            // Generate the Token (Identity naturally outputs 6 digits due to our Infrastructure setting)
            var numericCode = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Automated Communication Gateway Routing
            if (emailOrPhone.Contains("@"))
            {
                // Send via SMTP Email Service
                string subject = "YourAppName: Password Reset Verification Code";
                string body = $"<p>Your password reset verification code is: <strong>{numericCode}</strong></p>";

                await _emailService.SendEmailAsync(user.Email!, subject, body, cancellationToken);
            }
            else
            {
                // Send via Twilio SMS Service (Using your customized DialCode lookups)
                string formattedPhone = user.PhoneNumber!.TrimStart('0');
                var country = await _dbContext.Countries.FindAsync(new object[] { user.CountryId }, cancellationToken);
                string dialCode = country?.DialCode ?? "+20";
                string fullPhoneNumber = string.Concat(dialCode, formattedPhone);

                string smsMessage = $"Your YourAppName password reset code is: {numericCode}";

                await _smsService.SendSmsAsync(fullPhoneNumber, smsMessage, cancellationToken);
            }

            return SharedResourcesKeys.CodeSentSuccessfully;
        }
        public async Task<string> VerifyResetCodeAsync(string emailOrPhone, string code, CancellationToken cancellationToken)
        {
            // Find the user
            var user = await _userManager.FindByNameAsync(emailOrPhone)
                       ?? await _userManager.FindByEmailAsync(emailOrPhone)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == emailOrPhone, cancellationToken);

            // If the user doesn't exist, we return Invalid Token. 
            // We DO NOT say "User not found" here, because a hacker could use this endpoint to guess emails.
            if (user == null)
            {
                return SharedResourcesKeys.InvalidOrExpiredCode;
            }

            // Cryptographically verify the 6-digit code
            // This checks if the code is valid, but DOES NOT consume it yet.
            bool isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword", // The specific purpose of this token
                code);

            if (!isValid)
            {
                return SharedResourcesKeys.InvalidOrExpiredCode;
            }

            // 3. Success
            return SharedResourcesKeys.CodeVerifiedSuccess;
        }
        public async Task<IdentityResult> ResetPasswordAsync(string emailOrPhone, string code, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(emailOrPhone)
                       ?? await _userManager.FindByEmailAsync(emailOrPhone)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == emailOrPhone, cancellationToken);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = SharedResourcesKeys.UserNotFound });
            }

            // This safely consumes the token and updates the password
            var result = await _userManager.ResetPasswordAsync(user, code, newPassword);

            return result;
        }
        public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = SharedResourcesKeys.UserNotFound });
            }

            // Identity automatically verifies the current password and hashes the new one
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
        #endregion

        #region Private Helpers

        private async Task<(JwtSecurityToken, string)> GenerateJWTToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            // Dynamically determine UserType based on the class instance
            var userType = user.GetType().Name;

            var claims = new List<Claim>
            {
                // Core Identity
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                
                // Mapping properties using UserClaimModel names for consistency
                new Claim(nameof(UserClaimModel.UserName), user.UserName ?? ""),
                new Claim(nameof(UserClaimModel.Email), user.Email ?? ""),
                new Claim(nameof(UserClaimModel.PhoneNumber), user.PhoneNumber ?? ""),
                new Claim("SecurityStamp", user.SecurityStamp ?? string.Empty),
                
                // YourAppName Specific Details
                new Claim(nameof(UserClaimModel.FullName), user.FullName),
                new Claim(nameof(UserClaimModel.PreferredLanguage), user.PreferredLanguage),
                new Claim(nameof(UserClaimModel.CountryId), user.CountryId.ToString()),
                new Claim(nameof(UserClaimModel.UserType), userType)
            };

            claims.AddRange(userClaims);

            // NEW ROLE & PERMISSIONS LOOP 
            foreach (var roleName in userRoles)
            {
                // Add the basic Role claim ("Admin", "Client", etc.)
                claims.Add(new Claim(ClaimTypes.Role, roleName));

                // Fetch the role from the database
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    // Grab all Permissions assigned to this role
                    var roleClaims = await _roleManager.GetClaimsAsync(role);

                    // Add every permission into the JWT token!
                    claims.AddRange(roleClaims);
                }
            }


            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var jwtToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return (jwtToken, new JwtSecurityTokenHandler().WriteToken(jwtToken));
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private JwtSecurityToken ReadJWTToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentNullException(nameof(accessToken));

            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                ValidateIssuer = _jwtSettings.ValidateIssuer,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = _jwtSettings.ValidateAudience,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false
            };

            try
            {
                handler.ValidateToken(accessToken, parameters, out var validatedToken);
                return (JwtSecurityToken)validatedToken;
            }
            catch
            {
                throw new SecurityTokenException("TokenIsInvalid");
            }
        }

        #endregion
    }
}
