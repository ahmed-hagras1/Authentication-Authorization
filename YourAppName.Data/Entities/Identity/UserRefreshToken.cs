using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Data.Entities.Identity
{
    public class UserRefreshToken
    {
        public int Id { get; set; }

        // This MUST be a string because ApplicationUser uses string (GUID)
        public string UserId { get; set; } = string.Empty;

        public string? Token { get; set; }
        public string? JWTId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }

        [NotMapped]
        public bool IsActive => !IsRevoked && !IsUsed && ExpiryDate > DateTime.UtcNow;

        public DateTime AddedTime { get; set; }
        public DateTime ExpiryDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
