using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Data.Helpers
{
    public class UserClaimModel
    {
        // Core Identity
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Application Specific (New)
        public string FullName { get; set; }
        public string PreferredLanguage { get; set; }
        public int CountryId { get; set; }

        // Logical/Authorization Helpers
        public string UserType { get; set; } // e.g., "Coordinator", "ServiceProvider", or "Client"
    }
}
