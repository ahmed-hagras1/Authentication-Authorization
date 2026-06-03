using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Data.Helpers
{
    public class JWTSettings
    {
        // Changed "Secret" to "Key" to match the JSON
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }

        // Added the missing duration properties as integers
        public int DurationInMinutes { get; set; }
        public int RefreshTokenDurationInDays { get; set; }

        // Changed from string to bool to match the JSON boolean values
        public bool ValidateIssuer { get; set; }
        public bool ValidateAudience { get; set; }
        public bool ValidateLifetime { get; set; } // Matched casing with JSON
        public bool ValidateIssuerSigningKey { get; set; }
    }
}
